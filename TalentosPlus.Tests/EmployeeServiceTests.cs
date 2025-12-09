using Moq;
using TalentosPlus.Application.DTOs;
using TalentosPlus.Application.Services;
using TalentosPlus.Application.Interfaces;
using TalentosPlus.Domain.Entities;
using TalentosPlus.Domain.Interfaces;
using Xunit;

namespace TalentosPlus.Tests;

public class EmployeeServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCallRepositoryAdd()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockEmailService = new Mock<IEmailService>();
        
        mockUnitOfWork.Setup(u => u.Employees.Add(It.IsAny<Employee>()));
        
        // Mock Positions and EducationLevels repositories
        var mockPositionRepo = new Mock<IGenericRepository<Position>>();
        var mockEducationRepo = new Mock<IGenericRepository<EducationLevel>>();
        
        mockUnitOfWork.Setup(u => u.Positions).Returns(mockPositionRepo.Object);
        mockUnitOfWork.Setup(u => u.EducationLevels).Returns(mockEducationRepo.Object);

        // Setup FinAsync to return empty list (simulating new position/education)
        mockPositionRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()))
            .ReturnsAsync(new List<Position>());
        mockEducationRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<EducationLevel, bool>>>()))
            .ReturnsAsync(new List<EducationLevel>());

        mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var service = new EmployeeService(mockUnitOfWork.Object, mockEmailService.Object);
        var dto = new CreateEmployeeDto
        {
            FirstName = "Juan",
            LastName = "Perez",
            Email = "juan@test.com",
            DocumentNumber = "12345",
            Position = "Dev",
            Salary = 1000,
            DepartmentId = 1
        };

        // Act
        await service.CreateAsync(dto);

        // Assert
        mockUnitOfWork.Verify(u => u.Employees.Add(It.IsAny<Employee>()), Times.Once);
        mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce());
        mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenEmployeeExists()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockEmailService = new Mock<IEmailService>();

        var emp = new Employee { Id = 1, FirstName = "Test", DepartmentId = 1 };
        var dept = new Department { Id = 1, Name = "HR" };

        mockUnitOfWork.Setup(u => u.Employees.GetByIdAsync(1)).ReturnsAsync(emp);
        mockUnitOfWork.Setup(u => u.Departments.GetByIdAsync(1)).ReturnsAsync(dept);

        var service = new EmployeeService(mockUnitOfWork.Object, mockEmailService.Object);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("HR", result.DepartmentName);
    }
}
