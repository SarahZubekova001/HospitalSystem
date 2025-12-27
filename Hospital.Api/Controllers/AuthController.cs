using System.Security.Claims;
using Hospital.Api.Data;
using Hospital.Api.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

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
        if (await _db.UserAccounts.AnyAsync(u => u.LoginEmail == req.Email))
            return Conflict("Email already exists");

        if (await _db.Persons.AnyAsync(p => p.BirthNumber == req.BirthNumber))
            return Conflict("Person already exists");

        var role = await _db.Roles.SingleOrDefaultAsync(r => r.Name == "Pacient");
        if (role is null)
            return StatusCode(500, "Patient role missing");

        var insuranceExists = await _db.InsuranceCompanies
            .AnyAsync(i => i.Id == req.InsuranceCompanyId);

        if (!insuranceExists)
            return BadRequest("Invalid insurance company");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var person = new Person
        {
            BirthNumber = req.BirthNumber,
            FirstName = req.FirstName,
            LastName = req.LastName,
            StreetAddress = req.StreetAddress,
            CityPostalCode = req.CityPostalCode,
            Phone = req.Phone,
            Email = req.Email
        };

        _db.Persons.Add(person);

        var patient = new Patient
        {
            BirthNumber = req.BirthNumber,
            InsuranceCompanyId = req.InsuranceCompanyId,
            IsActive = true
        };

        _db.Patients.Add(patient);

        var user = new UserAccount
        {
            PersonId = req.BirthNumber,
            LoginEmail = req.Email,
            RoleId = role.Id,
            PasswordHash = _hasher.HashPassword(null!, req.Password)
        };

        _db.UserAccounts.Add(user);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { message = "Registered" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

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
