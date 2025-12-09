namespace TalentosPlus.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = null!;
    // Navigation property
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
