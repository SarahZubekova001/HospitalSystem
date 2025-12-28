using Hospital.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/doctor")]
[Authorize(Roles = "Lekár")]
public class DoctorController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public DoctorController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments([FromQuery] DateTime date)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var dayStartLocal = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);
        var dayEndLocal = dayStartLocal.AddDays(1);

        var dayStart = dayStartLocal.ToUniversalTime();
        var dayEnd = dayEndLocal.ToUniversalTime();



        var appointments = await
        (from a in _db.Appointments
         join s in _db.AppointmentSlots on a.AppointmentSlotId equals s.Id
         join p in _db.Patients on a.PatientId equals p.Id
         join person in _db.Persons on p.BirthNumber equals person.BirthNumber
         join at in _db.AppointmentTypes on a.AppointmentTypeId equals at.Id
         join st in _db.AppointmentStatuses on a.StatusId equals st.Id
         where s.StaffId == staffId.Value
            && s.StartTime >= dayStart
            && s.StartTime < dayEnd
         orderby s.StartTime
         select new
         {
             id = a.Id,
             slotId = a.AppointmentSlotId,
             startTime = s.StartTime,
             endTime = s.EndTime,
             statusId = a.StatusId,
             status = st.Name,
             appointmentTypeId = a.AppointmentTypeId,
             appointmentType = at.Name,
             patientId = a.PatientId,
             birthNumber = p.BirthNumber,
             fullName = person.FirstName + " " + person.LastName,
             phone = person.Phone,
             email = person.Email,
             reason = a.Reason,
             notes = a.Notes
         })
        .ToListAsync();




        return Ok(appointments);
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetSlots([FromQuery] DateTime date)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var dayStartLocal = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);
        var dayEndLocal = dayStartLocal.AddDays(1);

        var dayStart = dayStartLocal.ToUniversalTime();
        var dayEnd = dayEndLocal.ToUniversalTime();


        var slots = await _db.AppointmentSlots
            .Where(s => s.StaffId == staffId.Value && s.StartTime >= dayStart && s.StartTime < dayEnd)
            .OrderBy(s => s.StartTime)
            .Select(s => new
            {
                id = s.Id,
                startTime = s.StartTime,
                endTime = s.EndTime,
                isAvailable = s.IsAvailable
            })
            .ToListAsync();

        return Ok(slots);
    }

    public record CreateSlotRequest(DateTime StartTime, DateTime EndTime);

    [HttpPost("slots")]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotRequest req)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var startUtc = DateTime.SpecifyKind(req.StartTime, DateTimeKind.Local).ToUniversalTime();
        var endUtc = DateTime.SpecifyKind(req.EndTime, DateTimeKind.Local).ToUniversalTime();

        if (endUtc <= startUtc)
            return BadRequest("EndTime must be after StartTime.");

        var overlaps = await _db.AppointmentSlots.AnyAsync(s =>
            s.StaffId == staffId.Value &&
            startUtc < s.EndTime &&
            endUtc > s.StartTime);

        if (overlaps)
            return Conflict("Slot overlaps with existing slot.");

        var slot = new Entities.AppointmentSlot
        {
            StaffId = staffId.Value,
            StartTime = startUtc,
            EndTime = endUtc,
            IsAvailable = true
        };

        _db.AppointmentSlots.Add(slot);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = slot.Id,
            startTime = slot.StartTime,
            endTime = slot.EndTime,
            isAvailable = slot.IsAvailable
        });
    }


    [HttpDelete("slots/{id:int}")]
    public async Task<IActionResult> DeleteSlot(int id)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var slot = await _db.AppointmentSlots
            .FirstOrDefaultAsync(s => s.Id == id && s.StaffId == staffId.Value);

        if (slot == null)
            return NotFound();

        var used = await _db.Appointments.AnyAsync(a => a.AppointmentSlotId == id);
        if (used)
            return Conflict("Cannot delete slot with existing appointment.");

        _db.AppointmentSlots.Remove(slot);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("slots/{id:int}/availability")]
    public async Task<IActionResult> SetAvailability(int id, [FromQuery] bool isAvailable)
    {
        var staffId = await GetCurrentStaffId();
        if (staffId == null) return Unauthorized();

        var slot = await _db.AppointmentSlots
            .FirstOrDefaultAsync(s => s.Id == id && s.StaffId == staffId.Value);

        if (slot == null)
            return NotFound();

        var used = await _db.Appointments.AnyAsync(a => a.AppointmentSlotId == id);
        if (used)
            return Conflict("Cannot change availability for booked slot.");

        slot.IsAvailable = isAvailable;
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

        if (staffId == 0)
            return null;

        return staffId;
    }
}
