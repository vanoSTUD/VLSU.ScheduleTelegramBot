using Microsoft.AspNetCore.Http;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Responces;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IVlsuApiService
{
	public Task<List<Institute>?> GetInstitutesAsync();
	public Task<List<Group>?> GetGroupsAsync(long instituteId, int educationForm);
	public Task<List<ScheduleResponce>?> GetScheduleAsync(long groupId, int weekType = 0, string weekDays = "1,2,3,4,5,6");
	public Task<CurrentInfo?> GetCurrentInfo(long groupId);
	
}
