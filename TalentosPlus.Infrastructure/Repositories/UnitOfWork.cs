using TalentosPlus.Domain.Entities;
using TalentosPlus.Domain.Interfaces;
using TalentosPlus.Infrastructure.Persistence;

namespace TalentosPlus.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Employees = new GenericRepository<Employee>(_context);
        Departments = new GenericRepository<Department>(_context);
        Positions = new GenericRepository<Position>(_context);
        EducationLevels = new GenericRepository<EducationLevel>(_context);
    }

    public IGenericRepository<Employee> Employees { get; private set; }
    public IGenericRepository<Department> Departments { get; private set; }
    public IGenericRepository<Position> Positions { get; private set; }
    public IGenericRepository<EducationLevel> EducationLevels { get; private set; }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
