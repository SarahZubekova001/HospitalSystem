using Hospital.Api.Data;
using Hospital.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public AdminUsersController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.UserAccounts
            .Include(u => u.Role)
            .Include(u => u.Person)
            .OrderBy(u => u.Role.Name)
            .ThenBy(u => u.Person.LastName)
            .ThenBy(u => u.Person.FirstName)
            .Select(u => new
            {
                id = u.Id,
                email = u.LoginEmail,
                roleId = u.RoleId,
                role = u.Role.Name,
                birthNumber = u.PersonId,
                fullName = u.Person.FirstName + " " + u.Person.LastName
            })
            .ToListAsync();

        var roles = await _db.Roles
            .OrderBy(r => r.Name)
            .Select(r => new { id = r.Id, name = r.Name })
            .ToListAsync();

        return Ok(new { users, roles });
    }

    [HttpGet("lookups/specializations")]
    public async Task<IActionResult> Specializations()
    {
        var list = await _db.Specializations
            .OrderBy(s => s.Name)
            .Select(s => new { id = s.Id, name = s.Name })
            .ToListAsync();

        return Ok(list);
    }

    public record StaffPayload(int SpecializationId, string LicenseNumber, string WorkPhone);
    public record UpdateRoleRequest(int RoleId, StaffPayload? Staff);

    [HttpPut("users/{userId:int}/role")]
    public async Task<IActionResult> UpdateRole(int userId, [FromBody] UpdateRoleRequest req)
    {
        var user = await _db.UserAccounts
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user is null) return NotFound("User not found");

        var targetRole = await _db.Roles.SingleOrDefaultAsync(r => r.Id == req.RoleId);
        if (targetRole is null) return BadRequest("Role not found");

        var personExists = await _db.Persons.AnyAsync(p => p.BirthNumber == user.PersonId);
        if (!personExists) return BadRequest("Person not found");

        var isDoctorRole = targetRole.Name == "Lekár";

        if (isDoctorRole)
        {
            if (req.Staff is null) return BadRequest("Staff data is required for role Lekár");
            if (req.Staff.SpecializationId <= 0) return BadRequest("SpecializationId is required");
            if (string.IsNullOrWhiteSpace(req.Staff.LicenseNumber)) return BadRequest("LicenseNumber is required");
            if (string.IsNullOrWhiteSpace(req.Staff.WorkPhone)) return BadRequest("WorkPhone is required");

            var specExists = await _db.Specializations.AnyAsync(s => s.Id == req.Staff.SpecializationId);
            if (!specExists) return BadRequest("Invalid specialization");
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        user.RoleId = targetRole.Id;

        if (isDoctorRole)
        {
            var staff = await _db.Staff.SingleOrDefaultAsync(s => s.BirthNumber == user.PersonId);

            if (staff is null)
            {
                staff = new Staff
                {
                    BirthNumber = user.PersonId,
                    SpecializationId = req.Staff!.SpecializationId,
                    LicenseNumber = req.Staff!.LicenseNumber,
                    WorkPhone = req.Staff!.WorkPhone
                };

                _db.Staff.Add(staff);
            }
            else
            {
                staff.SpecializationId = req.Staff!.SpecializationId;
                staff.LicenseNumber = req.Staff!.LicenseNumber;
                staff.WorkPhone = req.Staff!.WorkPhone;
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "Role updated" });
    }
}
