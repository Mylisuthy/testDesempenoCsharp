using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TalentosPlus.Application.DTOs;
using TalentosPlus.Application.Interfaces;

namespace TalentosPlus.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IEmployeeService _employeeService;

    public AuthController(IConfiguration configuration, IEmployeeService employeeService)
    {
        _configuration = configuration;
        _employeeService = employeeService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Simple auth logic: Email + Doc Number as credential as per requirements "you define (ej. document + correo)"
        var employee = await _employeeService.GetByEmailAsync(request.Email);
        
        if (employee == null || employee.DocumentNumber != request.DocumentNumber)
        {
            return Unauthorized("Invalid credentials");
        }

        var token = GenerateJwtToken(employee);
        return Ok(new { token });
    }

    private string GenerateJwtToken(EmployeeDto employee)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "SecretKeyForTestingPurposesONLY123456789";
        var key = Encoding.ASCII.GetBytes(jwtKey);
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Email, employee.Email),
                new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}")
            }),
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string DocumentNumber { get; set; }
}
