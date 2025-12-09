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
    Task<ImportProcessResult> ImportFromExcelAsync(Stream excelStream);
    Task<ImportProcessResult> ImportFromJsonAsync(Stream jsonStream);
    Task<Stream> ExportToExcelAsync();
}
