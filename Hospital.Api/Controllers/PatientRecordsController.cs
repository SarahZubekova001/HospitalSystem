using Hospital.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/patient/records")]
[Authorize(Roles = "Pacient")]
public sealed class PatientRecordsController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public PatientRecordsController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRecords()
    {
        var patientId = await GetCurrentPatientId();
        if (patientId == null) return Unauthorized();

        var items =
            await (from r in _db.MedicalRecords
                   join a in _db.Appointments on r.AppointmentId equals a.Id
                   join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
                   join d in _db.Diagnoses on r.DiagnosisId equals d.Id into dj
                   from d in dj.DefaultIfEmpty()
                   where r.PatientId == patientId.Value
                   orderby s.StartTime descending
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

    [HttpGet("{medicalRecordId:int}/prescriptions")]
    public async Task<IActionResult> GetPrescriptionsForRecord(int medicalRecordId)
    {
        var patientId = await GetCurrentPatientId();
        if (patientId == null) return Unauthorized();

        var recordOk = await _db.MedicalRecords.AnyAsync(r => r.Id == medicalRecordId && r.PatientId == patientId.Value);
        if (!recordOk) return NotFound();

        var items = await
            (from p in _db.Prescriptions
             join m in _db.Medications on p.MedicationId equals m.Id
             where p.MedicalRecordId == medicalRecordId
             orderby p.Id descending
             select new
             {
                 id = p.Id,
                 medicalRecordId = p.MedicalRecordId,
                 medicationId = p.MedicationId,
                 medicationName = m.Name,
                 activeSubstance = m.ActiveSubstance,
                 dosage = p.Dosage,
                 quantity = p.Quantity,
                 validUntil = p.ValidUntil
             })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("~/api/patient/prescriptions")]
    public async Task<IActionResult> GetMyPrescriptions()
    {
        var patientId = await GetCurrentPatientId();
        if (patientId == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var items =
            await (from p in _db.Prescriptions
                   join r in _db.MedicalRecords on p.MedicalRecordId equals r.Id
                   join a in _db.Appointments on r.AppointmentId equals a.Id
                   join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
                   join m in _db.Medications on p.MedicationId equals m.Id
                   where r.PatientId == patientId.Value
                   orderby p.ValidUntil descending, p.Id descending
                   select new
                   {
                       id = p.Id,
                       medicalRecordId = p.MedicalRecordId,
                       recordNumber = r.RecordNumber,
                       appointmentId = r.AppointmentId,
                       appointmentStartTime = s.StartTime,
                       medicationId = p.MedicationId,
                       medicationName = m.Name,
                       activeSubstance = m.ActiveSubstance,
                       dosage = p.Dosage,
                       quantity = p.Quantity,
                       validUntil = p.ValidUntil,
                       isActive = p.ValidUntil == null || p.ValidUntil >= today
                   })
                  .ToListAsync();

        return Ok(items);
    }

    private async Task<int?> GetCurrentPatientId()
    {
        var email = User.FindFirst("email")?.Value ?? User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var birthNumber = await _db.UserAccounts
            .Where(u => u.LoginEmail == email)
            .Select(u => u.PersonId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(birthNumber))
            return null;

        var patientId = await _db.Patients
            .Where(p => p.BirthNumber == birthNumber)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        return patientId == 0 ? null : patientId;
    }
}
