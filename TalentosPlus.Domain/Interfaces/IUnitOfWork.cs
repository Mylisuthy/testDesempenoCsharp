namespace TalentosPlus.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Entities.Employee> Employees { get; }
    IGenericRepository<Entities.Department> Departments { get; }
    Task<int> CompleteAsync();
}
