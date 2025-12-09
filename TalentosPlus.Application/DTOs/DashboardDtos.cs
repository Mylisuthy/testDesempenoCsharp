namespace TalentosPlus.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalEmployees { get; set; }
    public int EmployeesOnVacation { get; set; }
    public int ActiveEmployees { get; set; }
    public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
}
