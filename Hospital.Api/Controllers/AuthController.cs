using System.Security.Claims;
using Hospital.Api.Data;
using Hospital.Api.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HospitalDbContext _db;
    private readonly PasswordHasher<object> _hasher = new();

    public AuthController(HospitalDbContext db)
    {
        _db = db;
    }

    public record LoginRequest(string Email, string Password);

    public record RegisterRequest(
        string BirthNumber,
        string FirstName,
        string LastName,
        string Email,
        string Password,
        int InsuranceCompanyId,
        string? StreetAddress,
        string? CityPostalCode,
        string? Phone
    );

    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.UserAccounts
            .Include(u => u.Role)
            .Include(u => u.Person)
            .SingleOrDefaultAsync(u => u.LoginEmail == req.Email);

        if (user is null)
            return Unauthorized("Invalid credentials");

        var verify = _hasher.VerifyHashedPassword(null!, user.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.LoginEmail),
            new(ClaimTypes.Role, user.Role.Name),
            new("personId", user.PersonId),
            new("fullName", $"{user.Person.FirstName} {user.Person.LastName}")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(new
        {
            email = user.LoginEmail,
            role = user.Role.Name,
            fullName = $"{user.Person.FirstName} {user.Person.LastName}"
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var birth = (req.BirthNumber ?? "").Trim();
        var first = (req.FirstName ?? "").Trim();
        var last = (req.LastName ?? "").Trim();
        var email = (req.Email ?? "").Trim();
        var pass = (req.Password ?? "");

        if (string.IsNullOrWhiteSpace(birth)) return BadRequest("Rodné číslo je povinné.");
        if (string.IsNullOrWhiteSpace(first)) return BadRequest("Meno je povinné.");
        if (string.IsNullOrWhiteSpace(last)) return BadRequest("Priezvisko je povinné.");
        if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email je povinný.");
        if (!email.Contains('@')) return BadRequest("Email musí obsahovať @.");
        if (string.IsNullOrWhiteSpace(pass)) return BadRequest("Heslo je povinné.");
        if (pass.Length < 4) return BadRequest("Heslo musí mať aspoň 4 znaky.");

        var insuranceOk = await _db.InsuranceCompanies.AnyAsync(i => i.Id == req.InsuranceCompanyId);
        if (!insuranceOk) return BadRequest("Zvolená poisťovňa neexistuje.");

        var emailExists = await _db.UserAccounts.AnyAsync(u => u.LoginEmail == email);
        if (emailExists) return Conflict("Tento prihlasovací email už existuje.");

        var birthExists = await _db.Persons.AnyAsync(p => p.BirthNumber == birth);
        if (birthExists) return Conflict("Toto rodné číslo už existuje.");

        var roleId = await _db.Roles
            .Where(r => r.Name == "Pacient")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (roleId == 0) return BadRequest("Rola Pacient neexistuje.");

        var street = string.IsNullOrWhiteSpace(req.StreetAddress) ? null : req.StreetAddress.Trim();
        var city = string.IsNullOrWhiteSpace(req.CityPostalCode) ? null : req.CityPostalCode.Trim();
        var phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();

        await using var tx = await _db.Database.BeginTransactionAsync();

        var person = new Person
        {
            BirthNumber = birth,
            FirstName = first,
            LastName = last,
            Email = email,
            StreetAddress = street,
            CityPostalCode = city,
            Phone = phone
        };

        _db.Persons.Add(person);
        await _db.SaveChangesAsync();

        var ua = new UserAccount
        {
            LoginEmail = email,
            PasswordHash = _hasher.HashPassword(null!, pass),
            RoleId = roleId,
            PersonId = birth
        };

        _db.UserAccounts.Add(ua);
        await _db.SaveChangesAsync();

        var patient = new Patient
        {
            BirthNumber = birth,
            InsuranceCompanyId = req.InsuranceCompanyId,
            PrimaryDoctorId = null,
            BloodType = null
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();

        return Ok(new { id = ua.Id });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.CurrentPassword) ||
            string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest("Vyplň všetky polia.");

        if (req.NewPassword.Length < 4)
            return BadRequest("Nové heslo musí mať aspoň 4 znakov.");

        var email = User.Identity?.Name;
        if (email == null) return Unauthorized();

        var user = await _db.UserAccounts
            .FirstOrDefaultAsync(u => u.LoginEmail == email);

        if (user == null) return NotFound();

        var verify = _hasher.VerifyHashedPassword(null!, user.PasswordHash, req.CurrentPassword);

        if (verify == PasswordVerificationResult.Failed)
            return BadRequest("Aktuálne heslo nie je správne.");

        user.PasswordHash = _hasher.HashPassword(null!, req.NewPassword);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Ok(new { isAuthenticated = false });

        return Ok(new
        {
            isAuthenticated = true,
            email = User.Identity!.Name,
            roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value),
            fullName = User.FindFirst("fullName")?.Value
        });
    }
}
