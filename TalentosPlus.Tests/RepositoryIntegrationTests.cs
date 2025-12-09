using Microsoft.EntityFrameworkCore;
using TalentosPlus.Domain.Entities;
using TalentosPlus.Infrastructure.Persistence;
using TalentosPlus.Infrastructure.Repositories;

namespace TalentosPlus.Tests;

public class RepositoryIntegrationTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddEmployee_ShouldPersistToDatabase()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = new GenericRepository<Employee>(context);
        var employee = new Employee 
        { 
            FirstName = "Integration", 
            LastName = "Test", 
            Email = "int@test.com", 
            DocumentNumber = "999",

            Position = new Position { Name = "QA" } // Navigation property
        };

        // Act
        repository.Add(employee);
        await context.SaveChangesAsync();

        // Assert
        var savedEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Email == "int@test.com");
        Assert.NotNull(savedEmployee);
        Assert.Equal("Integration", savedEmployee.FirstName);
    }

    [Fact]
    public async Task GetAllDepartments_ShouldReturnAllAddedDepartments()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var repository = new GenericRepository<Department>(context);
        
        repository.Add(new Department { Name = "IT" });
        repository.Add(new Department { Name = "Sales" });
        await context.SaveChangesAsync();

        // Act
        var departments = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, departments.Count());
    }
}
