using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.ResultPattern;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IVlsuApiService
{
    public Task<Result<CurrentInfo>> GetCurrentInfoAsync(long groupId, Roles role, CancellationToken ct = default);

    public Task<Result<List<Group>>> GetGroupsAsync(long instituteId, int educationForm, CancellationToken ct = default);

    public Task<Result<List<Institute>>> GetInstitutesAsync(CancellationToken ct = default);

    public Task<Result<ScheduleForWeek?>> GetScheduleAsync(long id, Roles role, int weekType = 0, string weekDays = "1,2,3,4,5,6", CancellationToken ct = default);

    public Task<Result<List<Teacher>>> GetTeachersAsync(string FIO, CancellationToken ct = default);
}
