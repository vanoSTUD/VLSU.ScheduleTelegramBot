using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using VLSU.ScheduleTelegramBot.Domain.Responces;

namespace VLSU.ScheduleTelegramBot.VlsuApiService;

public class VlsuApiService : IVlsuApiService
{
	private readonly ILogger<VlsuApiService> _logger;

	public VlsuApiService(ILogger<VlsuApiService> logger)
	{
		_logger = logger;
	}

	public async Task<List<Group>?> GetGroupsAsync(long instituteId, int educationForm)
	{
		try
		{
			using HttpClient client = new();
			string requestUrl = "https://abiturient-api.vlsu.ru/api/student/GetStudGroups";
			var requestBody = JsonContent.Create(new { Institut = instituteId, WFormed = educationForm });

			// Institut: \"{instituteId}\", WFormed: {educationForm}}}
			var responseMessage = await client.PostAsync(requestUrl, requestBody);
			var stringResponce = await responseMessage.Content.ReadAsStringAsync();
			var groups = JsonConvert.DeserializeObject<List<Group>>(stringResponce);

			return groups;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error from VlguApiService.GetGroups(): {Message}", ex.Message);

			return null;
		}
	}

	public async Task<List<Institute>?> GetInstitutesAsync()
	{
		try
		{
			using HttpClient client = new();
			string requestUrl = "https://abiturient-api.vlsu.ru/api/catalogs/GetInstitutes";

			var responseMessage = await client.GetAsync(requestUrl);
			var stringResponce = await responseMessage.Content.ReadAsStringAsync();
			var institutes = JsonConvert.DeserializeObject<List<Institute>>(stringResponce);

			return institutes;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error from VlguApiService.GetInstitutes(): {Message}", ex.Message);

			return null;
		}
	}

	public async Task<CurrentInfo?> GetCurrentInfo(long groupId)
	{
		try
		{
			using HttpClient client = new();
			string requestUrl = "https://abiturient-api.vlsu.ru/api/student/GetGroupCurrentInfo";
			string requestBody = groupId.ToString();

			var responseMessage = await client.PostAsJsonAsync(requestUrl, requestBody);
			var stringResponce = await responseMessage.Content.ReadAsStringAsync();
			var schedule = JsonConvert.DeserializeObject<CurrentInfo>(stringResponce);

			return schedule;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error from VlguApiService.GetCurrentInfo(): {Message}", ex.Message);

			return null;
		}
	}

	public async Task<List<ScheduleResponce>?> GetScheduleAsync(long groupId, int weekType = 0, string weekDays = "1,2,3,4,5,6")
	{
		try
		{
			using HttpClient client = new();
			string requestUrl = "https://abiturient-api.vlsu.ru/api/student/GetGroupSchedule";
			var requestBody = JsonContent.Create(new { Nrec = groupId, WeekDays = weekDays, WeekType = weekType });

			var responseMessage = await client.PostAsync(requestUrl, requestBody);
			var stringResponce = await responseMessage.Content.ReadAsStringAsync();

			if (stringResponce == "")
			{
				return new List<ScheduleResponce>();
			}

			var schedule = JsonConvert.DeserializeObject<List<ScheduleResponce>>(stringResponce);

			return schedule;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error from VlguApiService.GetScheduleAsync(): {Message}", ex.Message);

			return null;
		}
	}
}
