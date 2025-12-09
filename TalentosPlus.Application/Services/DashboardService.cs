using TalentosPlus.Application.DTOs;
using TalentosPlus.Application.Interfaces;
using TalentosPlus.Domain.Entities;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        var departments = await _unitOfWork.Departments.GetAllAsync();

        var stats = new DashboardStatsDto
        {
            TotalEmployees = employees.Count(),
            EmployeesOnVacation = employees.Count(e => e.Status == EmployeeStatus.OnVacation),
            ActiveEmployees = employees.Count(e => e.Status == EmployeeStatus.Active)
        };

        foreach(var dept in departments)
        {
            var count = employees.Count(e => e.DepartmentId == dept.Id);
            if (count > 0)
                stats.EmployeesByDepartment.Add(dept.Name, count);
        }

        return stats;
    }
}
