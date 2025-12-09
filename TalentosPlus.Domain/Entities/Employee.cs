namespace TalentosPlus.Domain.Entities;

public enum EmployeeStatus
{
    Active,
    Inactive,
    OnVacation
}

public class Employee : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Position { get; set; } = null!;
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    
    public EmployeeStatus Status { get; set; }
    
    // Additional Profile Info
    public string? ProfessionalProfile { get; set; }
    public string? EducationLevel { get; set; } // Could be an Enum, keeping as string for flexibility per requirements hint
    public string? ContactPhone { get; set; }

    // Foreign Key
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}
