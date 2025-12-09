namespace TalentosPlus.Domain.Interfaces;

public interface IAiService
{
    Task<string> AskQuestionAsync(string question, string contextData);
}
