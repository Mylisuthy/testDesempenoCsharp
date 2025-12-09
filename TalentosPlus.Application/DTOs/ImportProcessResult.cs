namespace TalentosPlus.Application.DTOs;

public class ImportProcessResult
{
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
