using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IVlsuApiService
{
    public Task<CurrentInfo?> GetCurrentInfoAsync(long groupId, Roles role, CancellationToken ct = default);

    public Task<List<Group>?> GetGroupsAsync(long instituteId, int educationForm, CancellationToken ct = default);

    public Task<List<Institute>?> GetInstitutesAsync(CancellationToken ct = default);

    public Task<ScheduleForWeek?> GetScheduleAsync(long id, Roles role, int weekType = 0, string weekDays = "1,2,3,4,5,6", CancellationToken ct = default);

    public Task<List<Teacher>?> GetTeachersAsync(string FIO, CancellationToken ct = default);
}
