using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IScheduleService
{
    public Task<ScheduleForWeek?> GetScheduleAsync(long id, Roles role);
}
