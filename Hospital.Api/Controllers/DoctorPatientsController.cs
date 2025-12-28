using Hospital.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/doctor")]
[Authorize(Roles = "Lekár")]
public class DoctorPatientsController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public DoctorPatientsController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetMyPatients([FromQuery] string? q)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var query = (q ?? "").Trim().ToLower();

        var primaryPatients =
            from pat in _db.Patients
            join per in _db.Persons on pat.BirthNumber equals per.BirthNumber
            where pat.PrimaryDoctorId == staffId.Value
            select new
            {
                patientId = pat.Id,
                birthNumber = pat.BirthNumber,
                firstName = per.FirstName,
                lastName = per.LastName,
                phone = per.Phone,
                email = per.Email,
                isPrimary = true,
                hasAppointment = false
            };

        var appointmentPatients =
            from a in _db.Appointments
            join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
            join pat in _db.Patients on a.PatientId equals pat.Id
            join per in _db.Persons on pat.BirthNumber equals per.BirthNumber
            where s.StaffId == staffId.Value
            select new
            {
                patientId = pat.Id,
                birthNumber = pat.BirthNumber,
                firstName = per.FirstName,
                lastName = per.LastName,
                phone = per.Phone,
                email = per.Email,
                isPrimary = false,
                hasAppointment = true
            };

        var merged = primaryPatients.Concat(appointmentPatients);

        if (!string.IsNullOrWhiteSpace(query))
        {
            merged =
                from x in merged
                where (x.firstName + " " + x.lastName).ToLower().Contains(query)
                   || (x.lastName + " " + x.firstName).ToLower().Contains(query)
                   || x.birthNumber.ToLower().Contains(query)
                   || (x.email ?? "").ToLower().Contains(query)
                select x;
        }

        var list = await merged
            .GroupBy(x => x.patientId)
            .Select(g => new
            {
                patientId = g.Key,
                birthNumber = g.Select(x => x.birthNumber).FirstOrDefault() ?? "",
                fullName = (g.Select(x => x.firstName).FirstOrDefault() ?? "") + " " + (g.Select(x => x.lastName).FirstOrDefault() ?? ""),
                phone = g.Select(x => x.phone).FirstOrDefault(),
                email = g.Select(x => x.email).FirstOrDefault(),
                isPrimary = g.Any(x => x.isPrimary),
                hasAppointment = g.Any(x => x.hasAppointment)
            })
            .OrderBy(x => x.fullName)
            .ToListAsync();

        return Ok(list);
    }

    private async Task<int?> GetCurrentStaffId()
    {
        var email = User.FindFirst("email")?.Value ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var staffId = await _db.UserAccounts
            .Where(u => u.LoginEmail == email)
            .Join(_db.Staff,
                u => u.PersonId,
                s => s.BirthNumber,
                (u, s) => s.Id)
            .FirstOrDefaultAsync();

        if (staffId == 0)
            return null;

        return staffId;
    }
}
