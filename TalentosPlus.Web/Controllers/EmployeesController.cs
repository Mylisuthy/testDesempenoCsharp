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
    private readonly IUnitOfWork _unitOfWork;

    public EmployeesController(IEmployeeService employeeService, IPdfService pdfService, IUnitOfWork unitOfWork)
    {
        _employeeService = employeeService;
        _pdfService = pdfService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        var employees = await _employeeService.GetAllAsync();
        
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            searchString = searchString.Trim().ToLower();
            employees = employees.Where(e => 
                e.FirstName.ToLower().Contains(searchString) || 
                e.LastName.ToLower().Contains(searchString) || 
                e.DocumentNumber.Contains(searchString) ||
                e.Email.ToLower().Contains(searchString));
        }

        ViewData["CurrentFilter"] = searchString;
        return View(employees);
    }

    public async Task<IActionResult> Create()
    {
        await LoadViewBagData();
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
                TempData["SuccessMessage"] = "Employee created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating employee: " + ex.Message;
                ModelState.AddModelError("", ex.Message);
            }
        }
        else
        {
             TempData["ErrorMessage"] = "Validation failed. Please check the form.";
        }
        await LoadViewBagData();
        return View(dto);
    }
    
    public async Task<IActionResult> Edit(int id)
    {
        var empDto = await _employeeService.GetByIdAsync(id);
        if (empDto == null) return NotFound();

        var createDto = new CreateEmployeeDto
        {
            FirstName = empDto.FirstName,
            LastName = empDto.LastName,
            DocumentNumber = empDto.DocumentNumber,
            Email = empDto.Email,
            Position = empDto.Position,
            Salary = empDto.Salary,
            JoinDate = empDto.JoinDate,
            DepartmentId = empDto.DepartmentId,
            ProfessionalProfile = empDto.ProfessionalProfile,
            EducationLevel = empDto.EducationLevel,
            ContactPhone = empDto.ContactPhone,
            Status = empDto.Status,
            DateOfBirth = empDto.DateOfBirth,
            Address = empDto.Address
        };
        
        await LoadViewBagData();
        ViewBag.Id = id;
        return View(createDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateEmployeeDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _employeeService.UpdateAsync(id, dto);
                TempData["SuccessMessage"] = "Employee updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating employee: " + ex.Message;
                ModelState.AddModelError("", ex.Message);
            }
        }
        else
        {
             TempData["ErrorMessage"] = "Validation failed. Please check the form.";
        }
        await LoadViewBagData();
        ViewBag.Id = id;
        return View(dto);
    }
    
    private async Task LoadViewBagData()
    {
        ViewBag.Departments = await _unitOfWork.Departments.GetAllAsync();
        ViewBag.Positions = await _unitOfWork.Positions.GetAllAsync();
        ViewBag.EducationLevels = await _unitOfWork.EducationLevels.GetAllAsync();
    }

    public async Task<IActionResult> DownloadCv(int id)
    {
        var emp = await _employeeService.GetByIdAsync(id);
        if (emp == null) return NotFound();

        var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: 'Helvetica', 'Arial', sans-serif; line-height: 1.6; color: #333; }}
                .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
                .header h1 {{ margin: 0; font-size: 28px; text-transform: uppercase; letter-spacing: 2px; }}
                .header p {{ margin: 5px 0 0; font-size: 18px; opacity: 0.9; }}
                .section {{ margin-top: 25px; margin-bottom: 25px; }}
                .section-title {{ color: #2c3e50; font-size: 18px; border-bottom: 2px solid #ecf0f1; padding-bottom: 5px; margin-bottom: 15px; text-transform: uppercase; font-weight: bold; }}
                .grid {{ display: table; width: 100%; }}
                .row {{ display: table-row; }}
                .col {{ display: table-cell; width: 50%; padding: 5px; vertical-align: top; }}
                .label {{ font-weight: bold; color: #7f8c8d; font-size: 12px; display: block; margin-bottom: 2px; }}
                .value {{ font-size: 14px; color: #2c3e50; }}
                .profile-text {{ background-color: #f9f9f9; padding: 15px; border-left: 4px solid #3498db; font-style: italic; }}
                .footer {{ margin-top: 40px; text-align: center; font-size: 10px; color: #bdc3c7; border-top: 1px solid #ecf0f1; padding-top: 10px; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>{emp.FirstName} {emp.LastName}</h1>
                <p>{emp.Position}</p>
            </div>

            <div class='section'>
                <div class='section-title'>Información de Contacto</div>
                <div class='grid'>
                    <div class='row'>
                        <div class='col'>
                            <span class='label'>Email</span>
                            <span class='value'>{emp.Email}</span>
                        </div>
                        <div class='col'>
                            <span class='label'>Teléfono</span>
                            <span class='value'>{emp.ContactPhone ?? "N/A"}</span>
                        </div>
                    </div>
                    <div class='row'>
                         <div class='col'>
                            <span class='label'>Dirección</span>
                            <span class='value'>{emp.Address ?? "N/A"}</span>
                        </div>
                        <div class='col'>
                            <span class='label'>Documento ID</span>
                            <span class='value'>{emp.DocumentNumber}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class='section'>
                <div class='section-title'>Perfil Profesional</div>
                <div class='profile-text'>
                    {emp.ProfessionalProfile ?? "Sin descripción de perfil profesional disponible."}
                </div>
            </div>

            <div class='section'>
                <div class='section-title'>Detalles Laborales</div>
                <div class='grid'>
                    <div class='row'>
                        <div class='col'>
                            <span class='label'>Departamento</span>
                            <span class='value'>{emp.DepartmentName}</span>
                        </div>
                        <div class='col'>
                             <span class='label'>Fecha de Ingreso</span>
                             <span class='value'>{emp.JoinDate.ToString("dd/MM/yyyy")}</span>
                        </div>
                    </div>
                     <div class='row'>
                        <div class='col'>
                             <span class='label'>Nivel Educativo</span>
                             <span class='value'>{emp.EducationLevel ?? "N/A"}</span>
                        </div>
                        <div class='col'>
                            <span class='label'>Estado Actual</span>
                            <span class='value'>{emp.Status}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div class='footer'>
                Generado por TalentosPlus HR System el {DateTime.Now:dd/MM/yyyy HH:mm}
            </div>
        </body>
        </html>
        ";
        
        var pdfBytes = _pdfService.GeneratePdf(html);
        return File(pdfBytes, "application/pdf", $"CV_{emp.FirstName}_{emp.LastName}.pdf");
    }

    [HttpPost]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            try 
            {
                using var stream = file.OpenReadStream();
                var ext = Path.GetExtension(file.FileName).ToLower();
                
                ImportProcessResult result;
                if (ext == ".json")
                {
                    // Call the new JSON import
                    // Since IEmployeeService interface might update, wait. 
                    // Actually, I need to cast or update interface first. 
                    // Assuming Interface IS updated (I'll do that next if not done).
                    // For now, let's assume I will update the interface in next step.
                     result = await _employeeService.ImportFromJsonAsync(stream);
                }
                else 
                {
                     result = await _employeeService.ImportFromExcelAsync(stream);
                }

                if (result.ErrorCount == 0)
                {
                    TempData["SuccessMessage"] = $"Successfully imported {result.SuccessCount} employees.";
                }
                else 
                {
                    TempData["SuccessMessage"] = $"Imported {result.SuccessCount} employees.";
                    var errorMsg = string.Join(" | ", result.Errors.Take(5));
                    if (result.ErrorCount > 5) errorMsg += $" ...and {result.ErrorCount - 5} more errors.";
                    TempData["ErrorMessage"] = $"Passed with {result.ErrorCount} errors. Details: {errorMsg}";
                }
            }
            catch (Exception ex) 
            {
                TempData["ErrorMessage"] = "Critical error importing file: " + ex.Message;
            }
        }
        else 
        {
            TempData["ErrorMessage"] = "Please select a valid file.";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var stream = await _employeeService.ExportToExcelAsync();
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Employees_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
