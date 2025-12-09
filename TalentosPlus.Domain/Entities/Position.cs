namespace TalentosPlus.Domain.Entities;

public class Position : BaseEntity
{
    public string Name { get; set; } = null!;
    
    // Navigation
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
