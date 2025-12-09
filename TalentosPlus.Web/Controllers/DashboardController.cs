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

    public DashboardController(IDashboardService dashboardService, IAiService aiService)
    {
        _dashboardService = dashboardService;
        _aiService = aiService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return View(stats);
    }

    [HttpPost]
    public async Task<IActionResult> AskAi([FromBody] AIRequest request)
    {
        var stats = await _dashboardService.GetStatsAsync();
        var contextJson = JsonSerializer.Serialize(stats);
        
        // Also possibly get raw list of employees for better context if token limit allows, 
        // but passing stats is safer for now effectively.
        // Or we could pass a condensed CSV string of all employees.

        var answer = await _aiService.AskQuestionAsync(request.Question, contextJson);
        return Json(new { answer });
    }
}

public class AIRequest
{
    public string Question { get; set; }
}
