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

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

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

        var verify = _hasher.VerifyHashedPassword(
            null!, user.PasswordHash, req.CurrentPassword);

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
