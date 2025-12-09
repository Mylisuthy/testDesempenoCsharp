namespace TalentosPlus.Domain.Interfaces;

public interface IPdfService
{
    byte[] GeneratePdf(string htmlContent);
}
