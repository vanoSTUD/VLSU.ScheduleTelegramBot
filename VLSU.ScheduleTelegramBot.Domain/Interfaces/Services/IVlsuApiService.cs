using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IVlsuApiService
{
    public Task<CurrentInfo?> GetCurrentInfoAsync(long groupId, Roles role);

    public Task<List<Group>?> GetGroupsAsync(long instituteId, int educationForm);

    public Task<List<Institute>?> GetInstitutesAsync();

    public Task<ScheduleForWeek?> GetScheduleAsync(long id, Roles role, int weekType = 0, string weekDays = "1,2,3,4,5,6");

    public Task<List<Teacher>?> GetTeachersAsync(string FIO);
}
