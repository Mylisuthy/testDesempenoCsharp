using Microsoft.AspNetCore.Mvc;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepartmentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();
        return Ok(departments);
    }
}
