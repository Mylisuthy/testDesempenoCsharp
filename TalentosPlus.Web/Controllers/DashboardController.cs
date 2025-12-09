using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentosPlus.Application.Interfaces;
using TalentosPlus.Domain.Interfaces;
using System.Text.Json;

namespace TalentosPlus.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IAiService _aiService;
    private readonly IEmployeeService _employeeService;

    public DashboardController(IDashboardService dashboardService, IAiService aiService, IEmployeeService employeeService)
    {
        _dashboardService = dashboardService;
        _aiService = aiService;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return View(stats);
    }

    [HttpPost]
    public async Task<IActionResult> AskAi([FromBody] AIRequest request)
    {
        // Gather context
        var stats = await _dashboardService.GetStatsAsync();
        var employees = await _employeeService.GetAllAsync();
        
        // Simplified list to save tokens but provide value
        var simpleList = employees.Select(e => new 
        {
            Name = $"{e.FirstName} {e.LastName}",
            e.Position,
            Department = e.DepartmentName,
            e.Status
        }).ToList();

        var contextData = new 
        {
            Statistics = stats,
            Employees = simpleList
        };

        var contextJson = JsonSerializer.Serialize(contextData);
        
        var answer = await _aiService.AskQuestionAsync(request.Question, contextJson);
        return Json(new { answer });
    }
}

public class AIRequest
{
    public required string Question { get; set; }
}
