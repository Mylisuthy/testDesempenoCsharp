using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentosPlus.Application.DTOs;
using TalentosPlus.Application.Interfaces;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IPdfService _pdfService;

    public EmployeesController(IEmployeeService employeeService, IPdfService pdfService)
    {
        _employeeService = employeeService;
        _pdfService = pdfService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateEmployeeDto dto)
    {
        // Public autoregister
        try
        {
            await _employeeService.CreateAsync(dto);
            return Ok(new { message = "Registration successful. Please check your email." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyInfo()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            var emp = await _employeeService.GetByIdAsync(userId);
            if (emp == null) return NotFound();
            return Ok(emp);
        }
        return Unauthorized();
    }

    [Authorize]
    [HttpGet("me/cv")]
    public async Task<IActionResult> DownloadMyCv()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            var emp = await _employeeService.GetByIdAsync(userId);
            if (emp == null) return NotFound();

             var html = $@"
            <h1>Hoja de Vida</h1>
            <h2>{emp.FirstName} {emp.LastName}</h2>
            <p><strong>Cargo:</strong> {emp.Position}</p>
            <p><strong>Email:</strong> {emp.Email}</p>
            <p><strong>Perfil:</strong> {emp.ProfessionalProfile}</p>
            ";
            
            var pdfBytes = _pdfService.GeneratePdf(html);
            return File(pdfBytes, "application/pdf", $"CV_{emp.FirstName}_{emp.LastName}.pdf");
        }
        return Unauthorized();
    }
}
