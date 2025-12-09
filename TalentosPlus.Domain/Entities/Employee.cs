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
    
    // Foreign Key for Position
    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? DateOfBirth { get; set; } // New
    public string? Address { get; set; } // New
    
    public EmployeeStatus Status { get; set; }
    
    // Additional Profile Info
    public string? ProfessionalProfile { get; set; }

    // Foreign Key for EducationLevel
    public int? EducationLevelId { get; set; }
    public EducationLevel? EducationLevel { get; set; }

    public string? ContactPhone { get; set; }

    // Foreign Key
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}
