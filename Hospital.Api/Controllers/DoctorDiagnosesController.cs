using Hospital.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/doctor/diagnoses")]
[Authorize(Roles = "Lekár")]
public sealed class DoctorDiagnosesController : ControllerBase
{
    private readonly HospitalDbContext _db;

    public DoctorDiagnosesController(HospitalDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? q = null)
    {
        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim().ToLower();

        var items = _db.Diagnoses.AsNoTracking().AsQueryable();

        if (q != null)
        {
            items = items.Where(d =>
                d.Icd10Code.ToLower().Contains(q) ||
                d.Name.ToLower().Contains(q));
        }

        var result = await items
            .OrderBy(d => d.Icd10Code)
            .Select(d => new
            {
                id = d.Id,
                code = d.Icd10Code,
                name = d.Name
            })
            .Take(50)
            .ToListAsync();

        return Ok(result);
    }
}
