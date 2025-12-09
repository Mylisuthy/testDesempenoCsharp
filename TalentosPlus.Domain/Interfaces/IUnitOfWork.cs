namespace TalentosPlus.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Entities.Employee> Employees { get; }
    IGenericRepository<Entities.Department> Departments { get; }
    IGenericRepository<Entities.Position> Positions { get; }
    IGenericRepository<Entities.EducationLevel> EducationLevels { get; }
    Task<int> CompleteAsync();
}
