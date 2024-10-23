using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Mappings;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using VLSU.ScheduleTelegramBot.Domain.Options;
using VLSU.ScheduleTelegramBot.Domain.Responces;
using VLSU.ScheduleTelegramBot.Domain.ResultPattern;

namespace VLSU.ScheduleTelegramBot.VlsuApiService;

public class VlsuApiService : IVlsuApiService
{
    private readonly IMapper _mapper;
    private readonly ILogger<VlsuApiService> _logger;
    private readonly IScheduleMapper _scheduleMapper;
    private readonly IOptions<VlsuApiOptions> _options;
    private readonly IHttpClientFactory _clientFactory;

    public VlsuApiService(ILogger<VlsuApiService> logger, IScheduleMapper scheduleMapper, IOptions<VlsuApiOptions> options, IMapper mapper, IHttpClientFactory clientFactory)
    {
        _mapper = mapper;
        _logger = logger;
        _options = options;
        _clientFactory = clientFactory;
        _scheduleMapper = scheduleMapper;
    }

    public async Task<Result<List<Group>>> GetGroupsAsync(long instituteId, int educationForm, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestUrl = _options.Value.GetGroupsUrl.AbsoluteUri;
            var requestBody = JsonContent.Create(new { Institut = instituteId, WFormed = educationForm });

            var responseMessage = await client.PostAsync(requestUrl, requestBody, cancellationToken);
            var stringResponce = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var responceGroups = JsonConvert.DeserializeObject<List<GroupInfoResponce>>(stringResponce);

            if (responceGroups == null)
                return "Не удалось получить группы";

            if (responceGroups.Count == 0)
                return "Группы не найдены";

            return _mapper.Map<List<Group>>(responceGroups);
        }
        catch(Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex);

            return "Не удалось получить информацию о группах";
        }

    }

    public async Task<Result<List<Institute>>> GetInstitutesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestUrl = _options.Value.GetInstitutesUrl.AbsoluteUri;

            var responseMessage = await client.GetAsync(requestUrl, cancellationToken);
            var responceString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var responceInstitutes = JsonConvert.DeserializeObject<List<InstituteInfoResponce>>(responceString);

            if (responceInstitutes == null)
                return "Не удалось получить информацию о институтах";

            if (responceInstitutes.Count == 0)
                return "Институты не найдены";

            return _mapper.Map<List<Institute>>(responceInstitutes);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex);

            return "Не удалось получить информацию о институтах";
        }
    }

    public async Task<Result<List<Teacher>>> GetTeachersAsync(string FIO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestUrl = _options.Value.GetTeachersUrl.AbsoluteUri;
            string requestBody = FIO;

            var responseMessage = await client.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);
            var responceString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var responceTeachers = JsonConvert.DeserializeObject<List<TeacherInfoResponce>>(responceString);

            if (responceTeachers == null)
                return "Ошибка в поиске преподавателя";

            if (responceTeachers.Count == 0)
                return $"Преподавателей с совпадением \"{FIO}\" не найдено";

            return _mapper.Map<List<Teacher>>(responceTeachers);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex);

            return "Не удалось получить информацию о преподавателях";
        }
    }

    public async Task<Result<CurrentInfo>> GetCurrentInfoAsync(long id, Roles role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return role switch
        {
            Roles.Group => await GetCurrentInfoAsync(id, _options.Value.GetGroupInfoUrl, cancellationToken),
            Roles.Teacher => await GetCurrentInfoAsync(id, _options.Value.GetTeacherInfoUrl, cancellationToken),
            _ => "Ошибка в запросе получения актуальной инфомации"
        };
    }

    public async Task<Result<ScheduleForWeek?>> GetScheduleAsync(long id, Roles role, int weekType = 0, string weekDays = "1,2,3,4,5,6", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return role switch
        {
            Roles.Group => await GetScheduleAsync(id, _options.Value.GetGroupScheduleUrl, weekType, weekDays, cancellationToken),
            Roles.Teacher => await GetScheduleAsync(id, _options.Value.GetTeacherScheduleUrl, weekType, weekDays, cancellationToken),
            _ => "Ошибка в запросе получения расписания занятий"
        };
    }

    private async Task<Result<CurrentInfo>> GetCurrentInfoAsync(long id, Uri sourceUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestBody = id.ToString();

            var responseMessage = await client.PostAsJsonAsync(sourceUri, requestBody, cancellationToken);
            var responceString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var currentInfo = JsonConvert.DeserializeObject<CurrentInfo>(responceString);

            return currentInfo!;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex);

            return "Не удалось получить актуальную информацию";
        }
    }

    private async Task<Result<ScheduleForWeek?>> GetScheduleAsync(long id, Uri sourceUri, int weekType = 0, string weekDays = "1,2,3,4,5,6", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string errorMessage = "Не удалось получить расписание занятий";

        try
        {
            var client = _clientFactory.CreateClient();
            var requestBody = JsonContent.Create(new { Nrec = id, WeekDays = weekDays, WeekType = weekType });

            var responseMessage = await client.PostAsync(sourceUri, requestBody, cancellationToken);
            var stringResponce = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(stringResponce))
                return errorMessage;

            var schedulesResponce = JsonConvert.DeserializeObject<List<ScheduleResponce>>(stringResponce);

            if (schedulesResponce == null)
                return "Расписание не найдено";

            var scheduleForWeek = _scheduleMapper.Map(schedulesResponce);
            return scheduleForWeek;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex);

            return errorMessage;
        }
    }
}
