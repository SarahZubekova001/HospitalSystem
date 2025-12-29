using Hospital.Api.Data;
using Hospital.Api.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



namespace Hospital.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public AccountController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _db.UserAccounts
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Person)
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                id = u.Id,
                loginEmail = u.LoginEmail,
                role = u.Role.Name,
                birthNumber = u.Person.BirthNumber,
                firstName = u.Person.FirstName,
                lastName = u.Person.LastName,
                streetAddress = u.Person.StreetAddress,
                cityPostalCode = u.Person.CityPostalCode,
                phone = u.Person.Phone,
                contactEmail = u.Person.Email
            })
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();

        object? patient = null;
        object? staff = null;
        List<object>? doctors = null;

        if (user.role == "Pacient")
        {
            patient = await _db.Patients
                .AsNoTracking()
                .Where(p => p.BirthNumber == user.birthNumber)
                .Select(p => new
                {
                    bloodType = p.BloodType,
                    primaryDoctorId = p.PrimaryDoctorId,
                    insuranceCompanyId = p.InsuranceCompanyId,
                    insuranceCompanyName = _db.InsuranceCompanies
                        .Where(i => i.Id == p.InsuranceCompanyId)
                        .Select(i => i.Name)
                        .FirstOrDefault(),
                    primaryDoctorName =
                        (from s in _db.Staff
                         join per in _db.Persons on s.BirthNumber equals per.BirthNumber
                         where s.Id == p.PrimaryDoctorId
                         select per.FirstName + " " + per.LastName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            doctors = await _db.Staff
                .AsNoTracking()
                .Join(_db.Persons.AsNoTracking(),
                    s => s.BirthNumber,
                    p => p.BirthNumber,
                    (s, p) => new
                    {
                        id = s.Id,
                        fullName = p.FirstName + " " + p.LastName
                    })
                .OrderBy(x => x.fullName)
                .Cast<object>()
                .ToListAsync();
        }

        if (user.role == "Lekár")
        {
            staff = await _db.Staff
                .AsNoTracking()
                .Where(s => s.BirthNumber == user.birthNumber)
                .Select(s => new
                {
                    specializationId = s.SpecializationId,
                    specializationName = _db.Specializations
                        .Where(sp => sp.Id == s.SpecializationId)
                        .Select(sp => sp.Name)
                        .FirstOrDefault(),
                    licenseNumber = s.LicenseNumber,
                    workPhone = s.WorkPhone
                })
                .FirstOrDefaultAsync();
        }

        var specializations = await _db.Specializations
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new { id = s.Id, name = s.Name })
            .ToListAsync();

        return Ok(new
        {
            user,
            patient,
            staff,
            doctors,
            specializations
        });
    }

    public sealed record UpdateProfileRequest(
        string? LoginEmail,
        string? BirthNumber,
        string? FirstName,
        string? LastName,
        string? StreetAddress,
        string? CityPostalCode,
        string? Phone,
        string? ContactEmail,
        string? BloodType,
        int? PrimaryDoctorId,
        string? WorkPhone,
        string? LicenseNumber,
        int? SpecializationId
    );

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var ua = await _db.UserAccounts
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);


        if (ua == null) return NotFound();

        var roleName = ua.Role.Name;
        var oldBirthNumber = ua.PersonId;
        var oldLoginEmail = ua.LoginEmail;

        var person = await _db.Persons.FirstOrDefaultAsync(p => p.BirthNumber == oldBirthNumber);
        if (person == null) return NotFound();

        var newLoginEmail = string.IsNullOrWhiteSpace(req.LoginEmail) ? oldLoginEmail : req.LoginEmail.Trim();
        var wantsBirthChange = !string.IsNullOrWhiteSpace(req.BirthNumber) && req.BirthNumber.Trim() != oldBirthNumber;
        var newBirthNumber = wantsBirthChange ? req.BirthNumber!.Trim() : oldBirthNumber;

        if (string.IsNullOrWhiteSpace(newLoginEmail))
            return BadRequest("Prihlasovací email je povinný.");

        if (!string.Equals(newLoginEmail, oldLoginEmail, StringComparison.OrdinalIgnoreCase))
        {
            var loginExists = await _db.UserAccounts.AnyAsync(u => u.LoginEmail == newLoginEmail && u.Id != ua.Id);
            if (loginExists) return Conflict("Tento prihlasovací email už existuje.");
        }

        var newContactEmail = string.IsNullOrWhiteSpace(req.ContactEmail) ? null : req.ContactEmail.Trim();
        if (!string.IsNullOrWhiteSpace(newContactEmail))
        {
            var emailExists = await _db.Persons.AnyAsync(p => p.Email == newContactEmail && p.BirthNumber != oldBirthNumber);
            if (emailExists) return Conflict("Tento kontaktný email už používa iný používateľ.");
        }

        if (roleName == "Pacient" && req.PrimaryDoctorId.HasValue)
        {
            var docOk = await _db.Staff.AnyAsync(s => s.Id == req.PrimaryDoctorId.Value);
            if (!docOk) return BadRequest("Zvolený lekár neexistuje.");
        }

        Staff? staffRow = null;
        if (roleName == "Lekár")
        {
            staffRow = await _db.Staff.FirstOrDefaultAsync(s => s.BirthNumber == oldBirthNumber);

            if (req.SpecializationId.HasValue)
            {
                var specOk = await _db.Specializations.AnyAsync(s => s.Id == req.SpecializationId.Value);
                if (!specOk) return BadRequest("Zvolená špecializácia neexistuje.");
            }

            if (!string.IsNullOrWhiteSpace(req.LicenseNumber))
            {
                var lic = req.LicenseNumber.Trim();
                var licUsed = await _db.Staff.AnyAsync(s => s.LicenseNumber == lic && s.BirthNumber != oldBirthNumber);
                if (licUsed) return Conflict("Toto licenčné číslo už používa iný lekár.");
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        if (wantsBirthChange)
        {
            var birthExists = await _db.Persons.AnyAsync(p => p.BirthNumber == newBirthNumber);
            if (birthExists) return Conflict("Toto rodné číslo už existuje.");

            ua.PersonId = newBirthNumber;
            person.BirthNumber = newBirthNumber;

            var patientRow = await _db.Patients.FirstOrDefaultAsync(p => p.BirthNumber == oldBirthNumber);
            if (patientRow != null) patientRow.BirthNumber = newBirthNumber;

            if (staffRow != null) staffRow.BirthNumber = newBirthNumber;

            oldBirthNumber = newBirthNumber;
        }

        if (!string.Equals(newLoginEmail, oldLoginEmail, StringComparison.OrdinalIgnoreCase))
            ua.LoginEmail = newLoginEmail;

        if (!string.IsNullOrWhiteSpace(req.FirstName)) person.FirstName = req.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(req.LastName)) person.LastName = req.LastName.Trim();

        if (string.IsNullOrWhiteSpace(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName))
            return BadRequest("Meno a priezvisko sú povinné.");

        if (req.StreetAddress != null) person.StreetAddress = string.IsNullOrWhiteSpace(req.StreetAddress) ? null : req.StreetAddress.Trim();
        if (req.CityPostalCode != null) person.CityPostalCode = string.IsNullOrWhiteSpace(req.CityPostalCode) ? null : req.CityPostalCode.Trim();
        if (req.Phone != null) person.Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        person.Email = newContactEmail;

        if (roleName == "Pacient")
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.BirthNumber == oldBirthNumber);
            if (patient != null)
            {
                if (req.BloodType != null) patient.BloodType = string.IsNullOrWhiteSpace(req.BloodType) ? null : req.BloodType.Trim();
                if (req.PrimaryDoctorId.HasValue || req.PrimaryDoctorId == null) patient.PrimaryDoctorId = req.PrimaryDoctorId;
            }
        }

        if (roleName == "Lekár")
        {
            if (staffRow == null)
                staffRow = await _db.Staff.FirstOrDefaultAsync(s => s.BirthNumber == oldBirthNumber);

            if (staffRow != null)
            {
                if (req.WorkPhone != null) staffRow.WorkPhone = string.IsNullOrWhiteSpace(req.WorkPhone) ? null : req.WorkPhone.Trim();
                if (req.LicenseNumber != null) staffRow.LicenseNumber = string.IsNullOrWhiteSpace(req.LicenseNumber) ? staffRow.LicenseNumber : req.LicenseNumber.Trim();
                if (req.SpecializationId.HasValue || req.SpecializationId == null) staffRow.SpecializationId = req.SpecializationId;
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _db.ChangeTracker.Clear();
        var fresh = await _db.UserAccounts
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Person)
            .FirstAsync(u => u.Id == ua.Id);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, fresh.Id.ToString()),
            new(ClaimTypes.Name, fresh.LoginEmail),
            new(ClaimTypes.Role, fresh.Role.Name),
            new("personId", fresh.PersonId),
            new("fullName", $"{fresh.Person.FirstName} {fresh.Person.LastName}")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return NoContent();
    }
}
