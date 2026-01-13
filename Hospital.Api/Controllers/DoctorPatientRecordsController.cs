using Hospital.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/doctor/patients/{patientId:int}/records")]
[Authorize(Roles = "Lekár")]
public sealed class DoctorPatientRecordsController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public DoctorPatientRecordsController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int patientId)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        if (!await IsPatientAllowed(staffId.Value, patientId))
            return Forbid();

        var items =
            await (from r in _db.MedicalRecords
                   join a in _db.Appointments on r.AppointmentId equals a.Id
                   join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
                   join d in _db.Diagnoses on r.DiagnosisId equals d.Id into dj
                   from d in dj.DefaultIfEmpty()
                   where r.PatientId == patientId && r.StaffId == staffId.Value
                   orderby r.Id descending
                   select new
                   {
                       id = r.Id,
                       appointmentId = r.AppointmentId,
                       recordNumber = r.RecordNumber,
                       results = r.Results,
                       appointmentStartTime = s.StartTime,
                       diagnosisId = r.DiagnosisId,
                       diagnosisCode = d != null ? d.Icd10Code : null,
                       diagnosisName = d != null ? d.Name : null
                   })
                  .ToListAsync();

        return Ok(items);
    }

    [HttpGet("eligible-appointments")]
    public async Task<IActionResult> EligibleAppointments(int patientId)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        if (!await IsPatientAllowed(staffId.Value, patientId))
            return Forbid();

        var appointments =
            await (from a in _db.Appointments
                   join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
                   where a.PatientId == patientId && s.StaffId == staffId.Value
                   orderby s.StartTime descending
                   select new
                   {
                       id = a.Id,
                       startTime = s.StartTime
                   })
                  .ToListAsync();

        return Ok(appointments);
    }

    public sealed record CreateRequest(int AppointmentId, string RecordNumber, string? Results, int? DiagnosisId);

    [HttpPost]
    public async Task<IActionResult> Create(int patientId, [FromBody] CreateRequest req)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        if (!await IsPatientAllowed(staffId.Value, patientId))
            return Forbid();

        var recordNumber = (req.RecordNumber ?? "").Trim();
        if (string.IsNullOrWhiteSpace(recordNumber))
            return BadRequest("RecordNumber je povinné.");

        if (req.DiagnosisId.HasValue)
        {
            var diagOk = await _db.Diagnoses.AnyAsync(d => d.Id == req.DiagnosisId.Value);
            if (!diagOk) return BadRequest("Diagnosis neexistuje.");
        }

        var appointment =
            await (from a in _db.Appointments
                   join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
                   where a.Id == req.AppointmentId
                   select new
                   {
                       a.Id,
                       a.PatientId,
                       StaffId = s.StaffId
                   })
                  .FirstOrDefaultAsync();

        if (appointment == null) return NotFound("Objednávka neexistuje.");
        if (appointment.PatientId != patientId) return BadRequest("Appointment nepatrí tomuto pacientovi.");
        if (appointment.StaffId != staffId.Value) return Forbid();

        var exists = await _db.MedicalRecords.AnyAsync(r => r.AppointmentId == req.AppointmentId);
        if (exists) return Conflict("Záznam k tejto objednávke už existuje.");

        var item = new Entities.MedicalRecord
        {
            AppointmentId = req.AppointmentId,
            PatientId = patientId,
            StaffId = staffId.Value,
            RecordNumber = recordNumber,
            Results = req.Results?.Trim(),
            DiagnosisId = req.DiagnosisId
        };

        _db.MedicalRecords.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new { id = item.Id });
    }

    public sealed record UpdateRequest(string RecordNumber, string? Results, int? DiagnosisId);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int patientId, int id, [FromBody] UpdateRequest req)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var item = await _db.MedicalRecords
            .FirstOrDefaultAsync(r => r.Id == id && r.PatientId == patientId && r.StaffId == staffId.Value);

        if (item == null) return NotFound();

        var recordNumber = (req.RecordNumber ?? "").Trim();
        if (string.IsNullOrWhiteSpace(recordNumber))
            return BadRequest("RecordNumber je povinné.");

        if (req.DiagnosisId.HasValue)
        {
            var diagOk = await _db.Diagnoses.AnyAsync(d => d.Id == req.DiagnosisId.Value);
            if (!diagOk) return BadRequest("Diagnosis neexistuje.");
        }

        item.RecordNumber = recordNumber;
        item.Results = req.Results?.Trim();
        item.DiagnosisId = req.DiagnosisId;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int patientId, int id)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var item = await _db.MedicalRecords
            .FirstOrDefaultAsync(r => r.Id == id && r.PatientId == patientId && r.StaffId == staffId.Value);

        if (item == null) return NotFound();

        var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.MedicalRecordId == id);
        if (hasPrescriptions) return Conflict("Nie je možné zmazať záznam, ktorý má predpisy.");

        _db.MedicalRecords.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> IsPatientAllowed(int staffId, int patientId)
    {
        if (await _db.Patients.AnyAsync(p => p.Id == patientId && p.PrimaryDoctorId == staffId))
            return true;

        return await
            (from a in _db.Appointments
             join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
             where a.PatientId == patientId && s.StaffId == staffId
             select a.Id).AnyAsync();
    }

    private async Task<int?> GetCurrentStaffId()
    {
        var email = User.FindFirst("email")?.Value ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email)) return null;

        var staffId = await _db.UserAccounts
            .Where(u => u.LoginEmail == email)
            .Join(_db.Staff,
                u => u.PersonId,
                s => s.BirthNumber,
                (u, s) => s.Id)
            .FirstOrDefaultAsync();

        return staffId == 0 ? null : staffId;
    }
}
