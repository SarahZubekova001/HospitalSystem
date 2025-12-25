using System.Security.Claims;
using Hospital.Api.Data;
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

    public AuthController(HospitalDbContext db) => _db = db;

    public record LoginRequest(string Email, string Password);

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
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
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
            name = User.Identity!.Name,
            roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray()
        });
    }
}
