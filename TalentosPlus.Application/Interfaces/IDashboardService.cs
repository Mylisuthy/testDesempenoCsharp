using TalentosPlus.Application.DTOs;

namespace TalentosPlus.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
