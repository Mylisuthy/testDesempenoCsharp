using TalentosPlus.Application.DTOs;

namespace TalentosPlus.Application.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto?> GetByEmailAsync(string email);
    Task CreateAsync(CreateEmployeeDto dto);
    Task UpdateAsync(int id, CreateEmployeeDto dto);
    Task DeleteAsync(int id);
    Task ImportFromExcelAsync(Stream excelStream);
}
