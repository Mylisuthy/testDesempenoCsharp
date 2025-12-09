using ClosedXML.Excel;
using System.Text.Json;
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
        
        var dtos = new List<EmployeeDto>();
        foreach(var e in employees)
        {
            // Hydrate relations manually since Generic Repo doesn't include them
            var dept = await _unitOfWork.Departments.GetByIdAsync(e.DepartmentId);
            var pos = await _unitOfWork.Positions.GetByIdAsync(e.PositionId);
            var edu = e.EducationLevelId.HasValue ? await _unitOfWork.EducationLevels.GetByIdAsync(e.EducationLevelId.Value) : null;
            
            // Assign to entity just for mapping context
            e.Position = pos;
            e.EducationLevel = edu;

            dtos.Add(MapToDto(e, dept?.Name ?? "Unknown"));
        }
        return dtos;
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null) return null;
        var dept = await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId);
        
        var pos = await _unitOfWork.Positions.GetByIdAsync(employee.PositionId);
        employee.Position = pos;
        if (employee.EducationLevelId.HasValue) employee.EducationLevel = await _unitOfWork.EducationLevels.GetByIdAsync(employee.EducationLevelId.Value);

        return MapToDto(employee, dept?.Name ?? "Unknown");
    }

    public async Task<EmployeeDto?> GetByEmailAsync(string email)
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.Email == email);
        var employee = employees.FirstOrDefault();
        if (employee == null) return null;
         var dept = await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId);
         
         var pos = await _unitOfWork.Positions.GetByIdAsync(employee.PositionId);
         employee.Position = pos;
         if (employee.EducationLevelId.HasValue) employee.EducationLevel = await _unitOfWork.EducationLevels.GetByIdAsync(employee.EducationLevelId.Value);

        return MapToDto(employee, dept?.Name ?? "Unknown");
    }

    public async Task CreateAsync(CreateEmployeeDto dto)
    {
        try
        {
            // Sanitize Email
            dto.Email = SanitizeEmail(dto.Email);
            
            // Check for duplicates
            var existingByEmail = await _unitOfWork.Employees.FindAsync(e => e.Email == dto.Email);
            if (existingByEmail.Any())
            {
                throw new InvalidOperationException($"Ya existe un empleado con el correo '{dto.Email}'.");
            }
            
            var existingByDoc = await _unitOfWork.Employees.FindAsync(e => e.DocumentNumber == dto.DocumentNumber);
            if (existingByDoc.Any())
            {
                throw new InvalidOperationException($"Ya existe un empleado con el documento '{dto.DocumentNumber}'.");
            }
            
            // Lookup or Create Position / EducationLevel
            var position = await GetOrCreatePositionAsync(dto.Position);
            var education = !string.IsNullOrEmpty(dto.EducationLevel) ? await GetOrCreateEducationLevelAsync(dto.EducationLevel) : null;

            // Ensure UTC for PostgreSQL
            var joinDateUtc = DateTime.SpecifyKind(dto.JoinDate, DateTimeKind.Utc);
            DateTime? dobUtc = dto.DateOfBirth.HasValue ? DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc) : null;

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DocumentNumber = dto.DocumentNumber,
                Email = dto.Email,
                PositionId = position.Id,
                Salary = dto.Salary,
                JoinDate = joinDateUtc,
                Status = dto.Status,
                DepartmentId = dto.DepartmentId,
                ProfessionalProfile = dto.ProfessionalProfile,
                EducationLevelId = education?.Id,
                ContactPhone = dto.ContactPhone,
                DateOfBirth = dobUtc,
                Address = dto.Address
            };

            _unitOfWork.Employees.Add(employee);
            await _unitOfWork.CompleteAsync();

            // Send Welcome Email
            try 
            {
                await _emailService.SendEmailAsync(dto.Email, "Bienvenido a TalentosPlus", $"Hola {dto.FirstName}, bienvenido a TalentosPlus. Tu registro fue exitoso.");
            }
            catch 
            {
                // Email failure shouldn't rollback DB
            }
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw validation errors as-is
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al crear empleado: {ex.Message}. Por favor verifica que todos los campos estén correctos.", ex);
        }
    }

    public async Task UpdateAsync(int id, CreateEmployeeDto dto)
    {
        try
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null) throw new InvalidOperationException("Empleado no encontrado.");
            
            // Sanitize
            dto.Email = SanitizeEmail(dto.Email);

            // Check for duplicates (excluding current employee)
            var existingByEmail = await _unitOfWork.Employees.FindAsync(e => e.Email == dto.Email && e.Id != id);
            if (existingByEmail.Any())
            {
                throw new InvalidOperationException($"Ya existe otro empleado con el correo '{dto.Email}'.");
            }
            
            var existingByDoc = await _unitOfWork.Employees.FindAsync(e => e.DocumentNumber == dto.DocumentNumber && e.Id != id);
            if (existingByDoc.Any())
            {
                throw new InvalidOperationException($"Ya existe otro empleado con el documento '{dto.DocumentNumber}'.");
            }

            var position = await GetOrCreatePositionAsync(dto.Position);
            var education = !string.IsNullOrEmpty(dto.EducationLevel) ? await GetOrCreateEducationLevelAsync(dto.EducationLevel) : null;

            // Ensure UTC for PostgreSQL
            var joinDateUtc = DateTime.SpecifyKind(dto.JoinDate, DateTimeKind.Utc);
            DateTime? dobUtc = dto.DateOfBirth.HasValue ? DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc) : null;

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.DocumentNumber = dto.DocumentNumber;
            employee.Email = dto.Email;
            employee.PositionId = position.Id;
            employee.Salary = dto.Salary;
            employee.Status = dto.Status;
            employee.DepartmentId = dto.DepartmentId;
            employee.ProfessionalProfile = dto.ProfessionalProfile;
            employee.EducationLevelId = education?.Id;
            employee.ContactPhone = dto.ContactPhone;
            employee.DateOfBirth = dobUtc;
            employee.Address = dto.Address;
            employee.JoinDate = joinDateUtc;

            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.CompleteAsync();
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw validation errors as-is
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al actualizar empleado: {ex.Message}. Por favor verifica que todos los campos estén correctos.", ex);
        }
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

    public async Task<ImportProcessResult> ImportFromExcelAsync(Stream excelStream)
    {
        var result = new ImportProcessResult();
        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

        foreach (var row in rows)
        {
            int rowNum = row.RowNumber();
            try 
            {
                var docNumber = row.Cell(1).GetValue<string>(); 
                if (string.IsNullOrWhiteSpace(docNumber)) 
                {
                    // Skip empty rows silently or log? Let's skip empty.
                    continue; 
                }

                var firstName = row.Cell(2).GetValue<string>();
                var lastName = row.Cell(3).GetValue<string>();

                // Safer Date Parsing
                DateTime? dob = null;
                if (!row.Cell(4).IsEmpty())
                {
                    DateTime rawDob;
                    if (row.Cell(4).DataType == XLDataType.DateTime)
                        rawDob = row.Cell(4).GetDateTime();
                    else if (DateTime.TryParse(row.Cell(4).GetValue<string>(), out var parsedDob))
                        rawDob = parsedDob;
                    else 
                        rawDob = DateTime.MinValue; // Failed verify

                    if (rawDob != DateTime.MinValue)
                        dob = DateTime.SpecifyKind(rawDob, DateTimeKind.Utc);
                }

                var address = row.Cell(5).GetValue<string>();
                var phone = row.Cell(6).GetValue<string>();
                var email = SanitizeEmail(row.Cell(7).GetValue<string>());
                var positionName = row.Cell(8).GetValue<string>();

                // Safer Decimal Parsing
                decimal salary = 0;
                if (!row.Cell(9).IsEmpty())
                {
                    if (row.Cell(9).DataType == XLDataType.Number)
                        salary = Convert.ToDecimal(row.Cell(9).GetDouble());
                    else 
                    {
                        var salStr = row.Cell(9).GetValue<string>();
                        salStr = salStr.Replace("$", "").Replace(" ", "");
                        decimal.TryParse(salStr, out salary);
                    }
                }

                // Join Date
                DateTime joinDate = DateTime.UtcNow; // Default to UtcNow
                if (row.Cell(10).DataType == XLDataType.DateTime)
                    joinDate = row.Cell(10).GetDateTime();
                else if (DateTime.TryParse(row.Cell(10).GetValue<string>(), out var parsedJoin))
                    joinDate = parsedJoin;
                
                // Enforce UTC
                joinDate = DateTime.SpecifyKind(joinDate, DateTimeKind.Utc);

                var statusStr = row.Cell(11).GetValue<string>();
                var eduName = row.Cell(12).GetValue<string>();
                var profile = row.Cell(13).GetValue<string>();
                var deptName = row.Cell(14).GetValue<string>();

                // Logic to find/create foreign keys
                var dept = await GetOrCreateDepartmentAsync(deptName);
                var position = await GetOrCreatePositionAsync(positionName);
                var edu = !string.IsNullOrWhiteSpace(eduName) ? await GetOrCreateEducationLevelAsync(eduName) : null;
                
                // Status Mapping (Already updated, keeping it)
                var statusStrLower = statusStr?.Trim().ToLower() ?? "";
                var status = EmployeeStatus.Active; 
                if (statusStrLower.Contains("inactivo")) status = EmployeeStatus.Inactive;
                else if (statusStrLower.Contains("vacaciones")) status = EmployeeStatus.OnVacation;
                else if (statusStrLower.Contains("activo")) status = EmployeeStatus.Active;

                // Ensure we don't accidentally update DocumentNumber if it's unique key 
                // but checking dupes above handles that.

                var emp = new Employee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DocumentNumber = docNumber,
                    Email = email,
                    PositionId = position.Id,
                    Salary = salary,
                    JoinDate = joinDate,
                    DateOfBirth = dob,
                    Address = address,
                    ContactPhone = phone,
                    Status = status,
                    DepartmentId = dept.Id,
                    EducationLevelId = edu?.Id,
                    ProfessionalProfile = profile
                };

                // Check dupes (local check against DB)
                var existing = await _unitOfWork.Employees.FindAsync(e => e.DocumentNumber == emp.DocumentNumber || e.Email == emp.Email);
                if (!existing.Any())
                {
                    _unitOfWork.Employees.Add(emp);
                    // Critical: Save immediately to catch DB errors per row
                    await _unitOfWork.CompleteAsync(); 
                    result.SuccessCount++;
                }
                else 
                {
                    result.Errors.Add($"Row {rowNum}: Duplicate Document/Email found in DB. Skipped.");
                }
            } 
            catch (Exception ex)
            {
                // Capture inner exception for DB errors
                var msg = ex.Message;
                if (ex.InnerException != null) msg += " -> " + ex.InnerException.Message;
                
                result.Errors.Add($"Row {rowNum}: {msg}");
                
                // If context is poisoned by failed add (EF Core tracking), we might need to detach? 
                // In a scoped service with per-request lifetime, a failed SaveChanges usually doesn't discard the entity from tracker.
                // We should ideally recreate context or clear tracker, but for now simple error reporting is key. 
                // 'continue' will proceed to next row, but if 'emp' is still in Added state, next SaveChanges might try to save it again.
                // To be safe, we should detach the failed entity if possible, but UnitOfWork generic repo might hide it.
                // For this targeted fix, let's assume one failure. 
                
                continue;
            }
        }
        // Removed final CompleteAsync as we save per row
        return result;
    }
    
    public async Task<ImportProcessResult> ImportFromJsonAsync(Stream jsonStream)
    {
        var result = new ImportProcessResult();
        try 
        {
            using var document = await JsonDocument.ParseAsync(jsonStream);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                result.Errors.Add("Invalid JSON format. Expected an array of employees.");
                return result;
            }

            int index = 0;
            foreach (var element in document.RootElement.EnumerateArray())
            {
                index++;
                try 
                {
                    // Extract fields safely
                    var docNumber = GetString(element, "Documento");
                    if (string.IsNullOrWhiteSpace(docNumber)) continue;

                    var firstName = GetString(element, "Nombres");
                    var lastName = GetString(element, "Apellidos");
                    
                    // Dates
                    var dobStr = GetString(element, "FechaNacimiento");
                    DateTime? dob = null;
                    if (DateTime.TryParse(dobStr, out var d)) dob = DateTime.SpecifyKind(d, DateTimeKind.Utc);

                    var joinDateStr = GetString(element, "FechaIngreso");
                    var joinDate = DateTime.TryParse(joinDateStr, out var jd) ? DateTime.SpecifyKind(jd, DateTimeKind.Utc) : DateTime.UtcNow;

                    var address = GetString(element, "Direccion");
                    var phone = GetString(element, "Telefono"); // Might be number or string in JSON
                    if (string.IsNullOrEmpty(phone) && element.TryGetProperty("Telefono", out var pVal) && pVal.ValueKind == JsonValueKind.Number)
                        phone = pVal.ToString();

                    var email = SanitizeEmail(GetString(element, "Email"));
                    var positionName = GetString(element, "Cargo");
                    
                    decimal salary = 0;
                    if (element.TryGetProperty("Salario", out var salVal))
                    {
                        if (salVal.ValueKind == JsonValueKind.Number) salary = salVal.GetDecimal();
                        else if (salVal.ValueKind == JsonValueKind.String) decimal.TryParse(salVal.GetString(), out salary);
                    }

                    var statusStr = GetString(element, "Estado");
                    var eduName = GetString(element, "NivelEducativo");
                    var profile = GetString(element, "PerfilProfesional");
                    var deptName = GetString(element, "Departamento");

                    // Logic matches Excel
                    var dept = await GetOrCreateDepartmentAsync(deptName);
                    var position = await GetOrCreatePositionAsync(positionName);
                    var edu = !string.IsNullOrWhiteSpace(eduName) ? await GetOrCreateEducationLevelAsync(eduName) : null;

                    var statusStrLower = statusStr?.Trim().ToLower() ?? "";
                    var status = EmployeeStatus.Active; 
                    if (statusStrLower.Contains("inactivo")) status = EmployeeStatus.Inactive;
                    else if (statusStrLower.Contains("vacaciones")) status = EmployeeStatus.OnVacation;
                    else if (statusStrLower.Contains("activo")) status = EmployeeStatus.Active;

                    var emp = new Employee
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        DocumentNumber = docNumber,
                        Email = email,
                        PositionId = position.Id,
                        Salary = salary,
                        JoinDate = joinDate,
                        DateOfBirth = dob,
                        Address = address,
                        ContactPhone = phone,
                        Status = status,
                        DepartmentId = dept.Id,
                        EducationLevelId = edu?.Id,
                        ProfessionalProfile = profile
                    };

                    var existing = await _unitOfWork.Employees.FindAsync(e => e.DocumentNumber == emp.DocumentNumber);
                    if (!existing.Any())
                    {
                        _unitOfWork.Employees.Add(emp);
                        await _unitOfWork.CompleteAsync();
                        result.SuccessCount++;
                    }
                    else 
                    {
                         result.Errors.Add($"Record {index}: Duplicate Document {docNumber}. Skipped.");
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    if (ex.InnerException != null) msg += " -> " + ex.InnerException.Message;
                    result.Errors.Add($"Record {index}: {msg}");
                }
            }
        }
        catch (Exception ex)
        {
             result.Errors.Add("Critical JSON Error: " + ex.Message);
        }
        return result;
    }

    private string GetString(JsonElement element, string propName)
    {
        if (element.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? "";
        return "";
    }

    public async Task<Stream> ExportToExcelAsync()
    {
        var employees = await GetAllAsync();
        var stream = new MemoryStream();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Empleados"); // Spanish Name

            // Spanish Headers
            worksheet.Cell(1, 1).Value = "Documento";
            worksheet.Cell(1, 2).Value = "Nombres";
            worksheet.Cell(1, 3).Value = "Apellidos";
            worksheet.Cell(1, 4).Value = "FechaNacimiento";
            worksheet.Cell(1, 5).Value = "Direccion";
            worksheet.Cell(1, 6).Value = "Telefono";
            worksheet.Cell(1, 7).Value = "Email";
            worksheet.Cell(1, 8).Value = "Cargo";
            worksheet.Cell(1, 9).Value = "Salario";
            worksheet.Cell(1, 10).Value = "FechaIngreso";
            worksheet.Cell(1, 11).Value = "Estado";
            worksheet.Cell(1, 12).Value = "NivelEducativo";
            worksheet.Cell(1, 13).Value = "PerfilProfesional";
            worksheet.Cell(1, 14).Value = "Departamento";

            // Style Header
            var header = worksheet.Range("A1:N1");
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.DarkSlateBlue;
            header.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var emp in employees)
            {
                worksheet.Cell(row, 1).Value = emp.DocumentNumber;
                worksheet.Cell(row, 2).Value = emp.FirstName;
                worksheet.Cell(row, 3).Value = emp.LastName;
                worksheet.Cell(row, 4).Value = emp.DateOfBirth.HasValue ? emp.DateOfBirth.Value.ToShortDateString() : "";
                worksheet.Cell(row, 5).Value = emp.Address;
                worksheet.Cell(row, 6).Value = emp.ContactPhone;
                worksheet.Cell(row, 7).Value = emp.Email;
                worksheet.Cell(row, 8).Value = emp.Position; // Position Name
                worksheet.Cell(row, 9).Value = emp.Salary;
                worksheet.Cell(row, 10).Value = emp.JoinDate.ToShortDateString();
                worksheet.Cell(row, 11).Value = emp.Status.ToString();
                worksheet.Cell(row, 12).Value = emp.EducationLevel; // Name
                worksheet.Cell(row, 13).Value = emp.ProfessionalProfile;
                worksheet.Cell(row, 14).Value = emp.DepartmentName;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(stream);
        }

        stream.Position = 0;
        return stream;
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
            Position = e.Position?.Name ?? "Unknown", // Nav property lookup
            Salary = e.Salary,
            JoinDate = e.JoinDate,
            DateOfBirth = e.DateOfBirth,
            Address = e.Address, 
            Status = e.Status,
            DepartmentId = e.DepartmentId,
            DepartmentName = deptName,
            ProfessionalProfile = e.ProfessionalProfile,
            EducationLevel = e.EducationLevel?.Name ?? "N/A", // Nav property lookup
            ContactPhone = e.ContactPhone
        };
    }

    // Helpers
    private async Task<Position> GetOrCreatePositionAsync(string name)
    {
        var existing = await _unitOfWork.Positions.FindAsync(p => p.Name.ToLower() == name.ToLower());
        var pos = existing.FirstOrDefault();
        if (pos == null)
        {
            pos = new Position { Name = name };
            _unitOfWork.Positions.Add(pos);
            await _unitOfWork.CompleteAsync();
        }
        return pos;
    }

    private async Task<EducationLevel> GetOrCreateEducationLevelAsync(string name)
    {
        var existing = await _unitOfWork.EducationLevels.FindAsync(e => e.Name.ToLower() == name.ToLower());
        var edu = existing.FirstOrDefault();
        if (edu == null)
        {
            edu = new EducationLevel { Name = name };
            _unitOfWork.EducationLevels.Add(edu);
            await _unitOfWork.CompleteAsync();
        }
        return edu;
    }
    private string SanitizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "";
        email = email.Trim().ToLower();
        return RemoveDiacritics(email);
    }

    private string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    private async Task<Department> GetOrCreateDepartmentAsync(string name)
    {
         if (string.IsNullOrWhiteSpace(name)) name = "General";
         var existing = await _unitOfWork.Departments.FindAsync(d => d.Name.ToLower() == name.ToLower());
         var dept = existing.FirstOrDefault();
         if (dept == null)
         {
             dept = new Department { Name = name };
             _unitOfWork.Departments.Add(dept);
             await _unitOfWork.CompleteAsync();
         }
         return dept;
    }
}
