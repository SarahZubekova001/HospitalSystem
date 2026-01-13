using Hospital.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/doctor/prescriptions")]
[Authorize(Roles = "Lekár")]
public sealed class DoctorPrescriptionsController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public DoctorPrescriptionsController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet("medications")]
    public async Task<IActionResult> GetMedications([FromQuery] string? q = null)
    {
        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var items = _db.Medications.AsQueryable();

        if (q != null)
        {
            var qLower = q.ToLower();
            items = items.Where(m => m.Name.ToLower().Contains(qLower));
        }

        var result = await items
            .OrderBy(m => m.Name)
            .Select(m => new
            {
                id = m.Id,
                name = m.Name,
                activeSubstance = m.ActiveSubstance
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("record/{medicalRecordId:int}")]
    public async Task<IActionResult> GetForRecord(int medicalRecordId)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var record = await _db.MedicalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == medicalRecordId);

        if (record == null) return NotFound();
        if (record.StaffId != staffId.Value) return Forbid();

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
                 dosage = p.Dosage,
                 quantity = p.Quantity,
                 validUntil = p.ValidUntil
             })
            .ToListAsync();

        return Ok(items);
    }

    public sealed record CreatePrescriptionRequest(
        int MedicalRecordId,
        int MedicationId,
        string? Dosage,
        int Quantity,
        DateTime? ValidUntil
    );

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest req)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var record = await _db.MedicalRecords.FirstOrDefaultAsync(r => r.Id == req.MedicalRecordId);
        if (record == null) return NotFound("MedicalRecord neexistuje.");

        if (record.StaffId != staffId.Value) return Forbid();

        var medOk = await _db.Medications.AnyAsync(m => m.Id == req.MedicationId);
        if (!medOk) return BadRequest("Medication neexistuje.");

        if (req.Quantity <= 0) return BadRequest("Quantity musí byť > 0.");

        var item = new Entities.Prescription
        {
            MedicalRecordId = req.MedicalRecordId,
            MedicationId = req.MedicationId,
            Dosage = string.IsNullOrWhiteSpace(req.Dosage) ? null : req.Dosage.Trim(),
            Quantity = req.Quantity,
            ValidUntil = req.ValidUntil.HasValue ? DateOnly.FromDateTime(req.ValidUntil.Value) : (DateOnly?)null
        };

        _db.Prescriptions.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = item.Id,
            medicalRecordId = item.MedicalRecordId,
            medicationId = item.MedicationId,
            dosage = item.Dosage,
            quantity = item.Quantity,
            validUntil = item.ValidUntil
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var pres = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id);
        if (pres == null) return NotFound();

        var record = await _db.MedicalRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Id == pres.MedicalRecordId);
        if (record == null) return NotFound();

        if (record.StaffId != staffId.Value) return Forbid();

        _db.Prescriptions.Remove(pres);
        await _db.SaveChangesAsync();
        return NoContent();
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

        return staffId == 0 ? null : staffId;
    }
}
