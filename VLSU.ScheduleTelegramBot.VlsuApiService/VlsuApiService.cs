using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;
using VLSU.ScheduleTelegramBot.Application.Helpers;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;
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

    public async Task<CurrentInfo?> GetGroupInfoAsync(long groupId)
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
            _logger.LogError(ex, "Error from VlguApiService.GetGroupCurrentInfo(): {Message}", ex.Message);

            return null;
        }
    }

    public async Task<CurrentInfo?> GetTeacherInfoAsync(long teacherId)
    {
        try
        {
            using HttpClient client = new();
            string requestUrl = "https://abiturient-api.vlsu.ru/api/employee/GetCurrentInfo";
            string requestBody = teacherId.ToString();

            var responseMessage = await client.PostAsJsonAsync(requestUrl, requestBody);
            var stringResponce = await responseMessage.Content.ReadAsStringAsync();
            var schedule = JsonConvert.DeserializeObject<CurrentInfo>(stringResponce);

            return schedule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error from VlguApiService.GetTeacherInfoAsync(): {Message}", ex.Message);

            return null;
        }
    }

    public async Task<ScheduleForWeek?> GetGroupScheduleAsync(long groupId, int weekType = 0, string weekDays = "1,2,3,4,5,6")
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
                return null;
            }

            var schedulesResponce = JsonConvert.DeserializeObject<List<ScheduleResponce>>(stringResponce);

            if (schedulesResponce == null)
                return null;

            
            var schedulesForDays = new List<ScheduleForDay>();

            var scheduleResponceProperties = typeof(ScheduleResponce).GetProperties();

            foreach(var schedule in schedulesResponce)
            {
                var nominatorWeekLessons = new List<Lesson>();
                var denominatorWeekLessons = new List<Lesson>();

                foreach (var property in scheduleResponceProperties)
                {
                    if (property.Name == nameof(ScheduleResponce.name))
                        continue;

                    var lessonDescription = (string?)schedule.GetType().GetProperty(property.Name)?.GetValue(schedule, null);
                    var lessonNumber = int.Parse(property.Name[1].ToString());

                    if (string.IsNullOrEmpty(lessonDescription))
                        continue;

                    if (property.Name.StartsWith('n'))
                        nominatorWeekLessons.Add(new Lesson(lessonDescription, lessonNumber));
                    else if (property.Name.StartsWith('z'))
                        denominatorWeekLessons.Add(new Lesson(lessonDescription, lessonNumber));
                }

                schedulesForDays.Add(new ScheduleForDay(schedule.name, WeekHelper.GetWeekType(schedule.name), nominatorWeekLessons, EducationWeekTypes.Nominator));
                schedulesForDays.Add(new ScheduleForDay(schedule.name, WeekHelper.GetWeekType(schedule.name), denominatorWeekLessons, EducationWeekTypes.Denominator));
            }

            var scheduleForWeek = new ScheduleForWeek(schedulesForDays);


            return scheduleForWeek;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error from VlguApiService.GetGroupScheduleAsync(): {Message}", ex.Message);

            return null;
        }
    }

    public async Task<ScheduleForWeek?> GetTeacherScheduleAsync(long teacherId, int weekType = 0, string weekDays = "1,2,3,4,5,6")
    {
        try
        {
            using HttpClient client = new();
            string requestUrl = "https://abiturient-api.vlsu.ru/api/employee/GetEmployeeSchedule";
            var requestBody = JsonContent.Create(new { Nrec = teacherId, WeekDays = weekDays, WeekType = weekType });

            var responseMessage = await client.PostAsync(requestUrl, requestBody);
            var stringResponce = await responseMessage.Content.ReadAsStringAsync();

            if (stringResponce == "")
            {
                return null;
            }

            var schedulesResponce = JsonConvert.DeserializeObject<List<ScheduleResponce>>(stringResponce);

            if (schedulesResponce == null)
                return null;

            var schedulesForDays = new List<ScheduleForDay>();
            var scheduleResponceProperties = typeof(ScheduleResponce).GetProperties();

            foreach (var schedule in schedulesResponce)
            {
                var nominatorWeekLessons = new List<Lesson>();
                var denominatorWeekLessons = new List<Lesson>();

                foreach (var property in scheduleResponceProperties)
                {
                    if (property.Name == nameof(ScheduleResponce.name))
                        continue;

                    var lessonDescription = (string?)schedule.GetType().GetProperty(property.Name)?.GetValue(schedule, null);
                    var lessonNumber = int.Parse(property.Name[1].ToString());

                    if (string.IsNullOrEmpty(lessonDescription))
                        continue;

                    if (property.Name.StartsWith('n'))
                        nominatorWeekLessons.Add(new Lesson(lessonDescription, lessonNumber));
                    else if (property.Name.StartsWith('z'))
                        denominatorWeekLessons.Add(new Lesson(lessonDescription, lessonNumber));
                }

                schedulesForDays.Add(new ScheduleForDay(schedule.name, WeekHelper.GetWeekType(schedule.name), nominatorWeekLessons, EducationWeekTypes.Nominator));
                schedulesForDays.Add(new ScheduleForDay(schedule.name, WeekHelper.GetWeekType(schedule.name), denominatorWeekLessons, EducationWeekTypes.Denominator));
            }

            var scheduleForWeek = new ScheduleForWeek(schedulesForDays);

            return scheduleForWeek;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error from VlguApiService.GetTeacherScheduleAsync(): {Message}", ex.Message);

            return null;
        }
    }

    public async Task<List<TeacherInfo>?> GetTeachersAsync(string FIO)
    {
        try
        {
            using HttpClient client = new();
            string requestUrl = "https://abiturient-api.vlsu.ru/api/employee/GetEmployeesCollection";
            string requestBody = FIO;

            var responseMessage = await client.PostAsJsonAsync(requestUrl, requestBody);
            var stringResponce = await responseMessage.Content.ReadAsStringAsync();
            var teachers = JsonConvert.DeserializeObject<List<TeacherInfo>>(stringResponce);

            return teachers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {Class}.{Method}, Message: {Message}", nameof(VlsuApiService), nameof(GetTeachersAsync), ex.Message);

            return null;
        }
    }

}
