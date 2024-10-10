using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Entities;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IVlsuApiService
{
    public Task<CurrentInfo?> GetGroupInfoAsync(long groupId);

    public Task<List<Group>?> GetGroupsAsync(long instituteId, int educationForm);

    public Task<List<Institute>?> GetInstitutesAsync();

    public Task<ScheduleForWeek?> GetGroupScheduleAsync(long groupId, int weekType = 0, string weekDays = "1,2,3,4,5,6");

    public Task<ScheduleForWeek?> GetTeacherScheduleAsync(long teacherId, int weekType = 0, string weekDays = "1,2,3,4,5,6");

    public Task<CurrentInfo?> GetTeacherInfoAsync(long teacherId);

    public Task<List<TeacherInfo>?> GetTeachersAsync(string FIO);

}
