using System.ComponentModel.DataAnnotations;
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
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? ProfessionalProfile { get; set; }
    public string? EducationLevel { get; set; }
    public string? ContactPhone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;
}

public class CreateEmployeeDto
{
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    public string DocumentNumber { get; set; } = null!;
    [Required]
    // Relaxed validation as per user request: just check for @ and .
    // This allows accents (which will be sanitized by the backend)
    [RegularExpression(@"^[^@]+@[^@]+\.[^@]+$", ErrorMessage = "Email must contain @ and .")]
    public string Email { get; set; } = null!;
    [Required]
    public string Position { get; set; } = null!;
    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public EmployeeStatus Status { get; set; }
    public string? ProfessionalProfile { get; set; }
    public string? EducationLevel { get; set; }
    public string? ContactPhone { get; set; }
    public int DepartmentId { get; set; }
}
