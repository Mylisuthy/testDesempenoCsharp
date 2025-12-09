using TalentosPlus.Domain.Entities;

namespace TalentosPlus.Application.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Position { get; set; } = null!;
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? ProfessionalProfile { get; set; }
    public string? EducationLevel { get; set; }
    public string? ContactPhone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;
}

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Position { get; set; } = null!;
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? ProfessionalProfile { get; set; }
    public string? EducationLevel { get; set; }
    public string? ContactPhone { get; set; }
    public int DepartmentId { get; set; }
}
