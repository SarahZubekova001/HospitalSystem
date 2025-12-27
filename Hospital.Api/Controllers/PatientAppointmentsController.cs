using System.Security.Claims;
using Hospital.Api.Data;
using Hospital.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/patient/appointments")]
[Authorize(Roles = "Pacient")]
public class PatientAppointmentsController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public PatientAppointmentsController(HospitalDbContext db)
    {
        _db = db;
    }

    private string GetBirthNumber()
        => User.FindFirstValue("personId") ?? throw new UnauthorizedAccessException("Missing personId claim");

    private async Task<int> GetPatientId()
    {
        var birthNumber = GetBirthNumber();

        var patientId = await _db.Patients
            .Where(p => p.BirthNumber == birthNumber)
            .Select(p => p.Id)
            .SingleOrDefaultAsync();

        if (patientId == 0)
            throw new UnauthorizedAccessException("Patient not found for this user");

        return patientId;
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery] int take = 10)
    {
        var patientId = await GetPatientId();

        var list = await _db.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentSlot.StartTime)
            .Select(a => new
            {
                a.Id,
                startAt = a.AppointmentSlot.StartTime,
                doctor = _db.Persons
                    .Where(p => p.BirthNumber == a.AppointmentSlot.Staff.BirthNumber)
                    .Select(p => p.FirstName + " " + p.LastName)
                    .FirstOrDefault() ?? "",
                type = a.AppointmentType.Name,
                status = a.Status.Name,
                canCancel = a.Status.Name != "Zrušené" && a.AppointmentSlot.StartTime > DateTime.UtcNow
            })
            .Take(take)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var patientId = await GetPatientId();

        var a = await _db.Appointments
            .Where(x => x.Id == id && x.PatientId == patientId)
            .Select(x => new
            {
                x.Id,
                startAt = x.AppointmentSlot.StartTime,
                endAt = x.AppointmentSlot.EndTime,
                doctor = _db.Persons
                    .Where(p => p.BirthNumber == x.AppointmentSlot.Staff.BirthNumber)
                    .Select(p => p.FirstName + " " + p.LastName)
                    .FirstOrDefault() ?? "",
                type = x.AppointmentType.Name,
                status = x.Status.Name,
                reason = x.Reason,
                notes = x.Notes
            })
            .SingleOrDefaultAsync();

        if (a is null) return NotFound();
        return Ok(a);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var patientId = await GetPatientId();

        var appt = await _db.Appointments
            .Include(a => a.AppointmentSlot)
            .SingleOrDefaultAsync(a => a.Id == id && a.PatientId == patientId);

        if (appt is null) return NotFound();

        var cancelledId = await _db.AppointmentStatuses
            .Where(s => s.Name == "Zrušené")
            .Select(s => s.Id)
            .SingleAsync();

        appt.StatusId = cancelledId;

        if (appt.AppointmentSlot is not null)
            appt.AppointmentSlot.IsAvailable = true;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Cancelled" });
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> Doctors()
    {
        var list = await _db.Staff
            .Join(_db.Persons,
                s => s.BirthNumber,
                p => p.BirthNumber,
                (s, p) => new { s.Id, p.FirstName, p.LastName })
            .OrderBy(x => x.LastName)
            .Select(x => new
            {
                id = x.Id,
                name = x.FirstName + " " + x.LastName
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("types")]
    public async Task<IActionResult> Types()
    {
        var list = await _db.AppointmentTypes
            .OrderBy(t => t.Name)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("slots")]
    public async Task<IActionResult> Slots([FromQuery] int staffId, [FromQuery] int take = 30)
    {
        var now = DateTime.UtcNow;

        var list = await _db.AppointmentSlots
            .Where(s => s.StaffId == staffId && s.IsAvailable && s.StartTime > now)
            .OrderBy(s => s.StartTime)
            .Select(s => new
            {
                s.Id,
                startAt = s.StartTime,
                endAt = s.EndTime
            })
            .Take(take)
            .ToListAsync();

        return Ok(list);
    }

    public record CreateRequest(int StaffId, int TypeId, int SlotId, string Reason);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest req)
    {
        var patientId = await GetPatientId();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var slot = await _db.AppointmentSlots
            .SingleOrDefaultAsync(s => s.Id == req.SlotId && s.StaffId == req.StaffId);

        if (slot is null) return BadRequest("Invalid slot");
        if (!slot.IsAvailable) return Conflict("Slot not available");

        slot.IsAvailable = false;

        var plannedId = await _db.AppointmentStatuses
            .Where(s => s.Name == "Naplánované")
            .Select(s => s.Id)
            .SingleAsync();

        var appt = new Appointment
        {
            AppointmentSlotId = req.SlotId,
            PatientId = patientId,
            AppointmentTypeId = req.TypeId,
            StatusId = plannedId,
            Reason = req.Reason,
            Notes = null
        };

        _db.Appointments.Add(appt);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { id = appt.Id });
    }
}
