using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentosPlus.Application.DTOs;
using TalentosPlus.Application.Interfaces;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IPdfService _pdfService;

    public EmployeesController(IEmployeeService employeeService, IPdfService pdfService)
    {
        _employeeService = employeeService;
        _pdfService = pdfService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllAsync();
        return View(employees);
    }

    public IActionResult Create()
    {
        // Ideally load Departments for dropdown via ViewBag or ViewModel
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeDto dto)
    {
        if (ModelState.IsValid)
        {
            try 
            {
                await _employeeService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(dto);
    }
    
    // Edit Actions... (omitted for brevity, can generate if requested, focusing on requirements)

    public async Task<IActionResult> DownloadCv(int id)
    {
        var emp = await _employeeService.GetByIdAsync(id);
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

    [HttpPost]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            await _employeeService.ImportFromExcelAsync(stream);
        }
        return RedirectToAction(nameof(Index));
    }
}
