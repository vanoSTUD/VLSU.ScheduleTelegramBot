using Microsoft.Extensions.Logging;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IVlsuApiService _vsuApiService;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(IVlsuApiService vsuApiService, ILogger<ScheduleService> logger)
    {
        _logger = logger;
        _vsuApiService = vsuApiService;
    }

    public async Task<ScheduleForWeek?> GetScheduleAsync(long id, Roles role)
    {
        ScheduleForWeek? schedule = null;

        if (role == Roles.Group)
        {
            schedule = await _vsuApiService.GetGroupScheduleAsync(id);
        }
        else if (role == Roles.Teacher)
        {
            schedule = await _vsuApiService.GetTeacherScheduleAsync(id);
        }

        if (schedule == null)
            _logger.LogInformation("ScheduleService.GetSchedule returns null. Id = {id}, Role = {role}", id, role);

        return schedule;
    }
}
