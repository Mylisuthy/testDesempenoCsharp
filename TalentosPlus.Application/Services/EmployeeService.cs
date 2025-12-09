using ClosedXML.Excel;
using TalentosPlus.Application.DTOs;
using TalentosPlus.Application.Interfaces;
using TalentosPlus.Domain.Entities;
using TalentosPlus.Domain.Interfaces;

namespace TalentosPlus.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public EmployeeService(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        // Manual mapping for now, can perform joins if needed via specific repo methods, 
        // but lazy loading or include is better handled in repo. 
        // For Proof of Concept, we assume rudimentary mapping.
        // In real world, AutoMapper or Mapster.
        
        var dtos = new List<EmployeeDto>();
        foreach(var e in employees)
        {
            var dept = await _unitOfWork.Departments.GetByIdAsync(e.DepartmentId);
            dtos.Add(MapToDto(e, dept?.Name ?? "Unknown"));
        }
        return dtos;
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null) return null;
        var dept = await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId);
        return MapToDto(employee, dept?.Name ?? "Unknown");
    }

    public async Task<EmployeeDto?> GetByEmailAsync(string email)
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.Email == email);
        var employee = employees.FirstOrDefault();
        if (employee == null) return null;
         var dept = await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId);
        return MapToDto(employee, dept?.Name ?? "Unknown");
    }

    public async Task CreateAsync(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DocumentNumber = dto.DocumentNumber,
            Email = dto.Email,
            Position = dto.Position,
            Salary = dto.Salary,
            JoinDate = dto.JoinDate,
            Status = dto.Status,
            DepartmentId = dto.DepartmentId,
            ProfessionalProfile = dto.ProfessionalProfile,
            EducationLevel = dto.EducationLevel,
            ContactPhone = dto.ContactPhone
        };

        _unitOfWork.Employees.Add(employee);
        await _unitOfWork.CompleteAsync();

        // Send Welcome Email if it's a self-registration or admin creation? 
        // Requirement says "Autoregistro... system must send real email".
        // Use IEmailService here.
        try 
        {
            await _emailService.SendEmailAsync(dto.Email, "Bienvenido a TalentosPlus", $"Hola {dto.FirstName}, bienvenido a TalentosPlus. Tu registro fue exitoso.");
        }
        catch 
        {
            // Email failure shouldn't rollback DB in this context usually, but depends on strictness.
            // Ignoring for now to keep flow active.
        }
    }

    public async Task UpdateAsync(int id, CreateEmployeeDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null) throw new Exception("Employee not found");

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.DocumentNumber = dto.DocumentNumber;
        employee.Email = dto.Email;
        employee.Position = dto.Position;
        employee.Salary = dto.Salary;
        employee.Status = dto.Status;
        employee.DepartmentId = dto.DepartmentId;
        employee.ProfessionalProfile = dto.ProfessionalProfile;
        employee.EducationLevel = dto.EducationLevel;
        employee.ContactPhone = dto.ContactPhone;
        // JoinDate usually doesn't change easily, but let's allow it.
        employee.JoinDate = dto.JoinDate;

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee != null)
        {
            _unitOfWork.Employees.Remove(employee);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task ImportFromExcelAsync(Stream excelStream)
    {
        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

        foreach (var row in rows)
        {
            // Assuming simplified columns order based on typical Excel test files:
            // Name, Surname, Doc, Email, Position, Salary, JoinDate, Dept, etc.
            // We need to match columns by name or index. 
            // For robustness, usually we search headers. For this test, I will implementation robust header search.
            
            // Let's implement robust finding later if column names are unknown, 
            // but for now let's assume valid columns exist.
            
            // Minimal check
            try 
            {
                var departmentName = row.Cell(8).GetValue<string>(); // e.g. Col 8
                
                // Find or Create Department
                var depts = await _unitOfWork.Departments.FindAsync(d => d.Name == departmentName);
                var dept = depts.FirstOrDefault();
                if (dept == null)
                {
                    dept = new Department { Name = departmentName };
                    _unitOfWork.Departments.Add(dept);
                    await _unitOfWork.CompleteAsync(); // Save to get ID
                }

                var emp = new Employee
                {
                    FirstName = row.Cell(1).GetValue<string>(),
                    LastName = row.Cell(2).GetValue<string>(),
                    DocumentNumber = row.Cell(3).GetValue<string>(),
                    Email = row.Cell(4).GetValue<string>(),
                    Position = row.Cell(5).GetValue<string>(),
                    Salary = row.Cell(6).GetValue<decimal>(),
                    JoinDate = row.Cell(7).GetDateTime(),
                    DepartmentId = dept.Id,
                    Status = EmployeeStatus.Active // Default
                };
                
                // Add more fields if present in Excel
                
                // Check dupes
                var existing = await _unitOfWork.Employees.FindAsync(e => e.DocumentNumber == emp.DocumentNumber || e.Email == emp.Email);
                if (!existing.Any())
                {
                    _unitOfWork.Employees.Add(emp);
                }
            } 
            catch
            {
                // Skip invalid rows
                continue;
            }
        }
        await _unitOfWork.CompleteAsync();
    }

    private static EmployeeDto MapToDto(Employee e, string deptName)
    {
        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            DocumentNumber = e.DocumentNumber,
            Email = e.Email,
            Position = e.Position,
            Salary = e.Salary,
            JoinDate = e.JoinDate,
            Status = e.Status,
            DepartmentId = e.DepartmentId,
            DepartmentName = deptName,
            ProfessionalProfile = e.ProfessionalProfile,
            EducationLevel = e.EducationLevel,
            ContactPhone = e.ContactPhone
        };
    }
}
