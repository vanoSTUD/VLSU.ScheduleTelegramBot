﻿using AutoMapper;
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

    public async Task<List<Group>?> GetGroupsAsync(long instituteId, int educationForm, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestUrl = _options.Value.GetGroups.AbsoluteUri;
            var requestBody = JsonContent.Create(new { Institut = instituteId, WFormed = educationForm });

            var responseMessage = await client.PostAsync(requestUrl, requestBody, cancellationToken);
            var stringResponce = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var responceGroups = JsonConvert.DeserializeObject<List<GroupInfoResponce>>(stringResponce);

            return _mapper.Map<List<Group>>(responceGroups);
        }
        catch(Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex.Message);

            return null;
        }

    }

    public async Task<List<Institute>?> GetInstitutesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestUrl = _options.Value.GetInstitutes.AbsoluteUri;

            var responseMessage = await client.GetAsync(requestUrl, cancellationToken);
            var responceString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var responceInstitutes = JsonConvert.DeserializeObject<List<InstituteInfoResponce>>(responceString);

            return _mapper.Map<List<Institute>>(responceInstitutes);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex.Message);

            return null;
        }
    }

    public async Task<List<Teacher>?> GetTeachersAsync(string FIO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestUrl = _options.Value.GetTeachers.AbsoluteUri;
            string requestBody = FIO;

            var responseMessage = await client.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);
            var responceString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var responceTeachers = JsonConvert.DeserializeObject<List<TeacherInfoResponce>>(responceString);

            return _mapper.Map<List<Teacher>>(responceTeachers);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex.Message);

            return null;
        }
    }

    public async Task<CurrentInfo?> GetCurrentInfoAsync(long id, Roles role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return role switch
        {
            Roles.Group => await GetCurrentInfoAsync(id, _options.Value.GetGroupInfo, cancellationToken),
            Roles.Teacher => await GetCurrentInfoAsync(id, _options.Value.GetTeacherInfo, cancellationToken),
            _ => null
        };
    }

    public async Task<ScheduleForWeek?> GetScheduleAsync(long id, Roles role, int weekType = 0, string weekDays = "1,2,3,4,5,6", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return role switch
        {
            Roles.Group => await GetScheduleAsync(id, _options.Value.GetGroupSchedule, weekType, weekDays, cancellationToken),
            Roles.Teacher => await GetScheduleAsync(id, _options.Value.GetTeacherSchedule, weekType, weekDays, cancellationToken),
            _ => null
        };
    }

    private async Task<CurrentInfo?> GetCurrentInfoAsync(long id, Uri sourceUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            string requestBody = id.ToString();

            var responseMessage = await client.PostAsJsonAsync(sourceUri, requestBody, cancellationToken);
            var responceString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var currentInfo = JsonConvert.DeserializeObject<CurrentInfo>(responceString);

            return currentInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex.Message);

            return null;
        }
    }

    private async Task<ScheduleForWeek?> GetScheduleAsync(long id, Uri sourceUri, int weekType = 0, string weekDays = "1,2,3,4,5,6", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var client = _clientFactory.CreateClient();
            var requestBody = JsonContent.Create(new { Nrec = id, WeekDays = weekDays, WeekType = weekType });

            var responseMessage = await client.PostAsync(sourceUri, requestBody, cancellationToken);
            var stringResponce = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrEmpty(stringResponce))
                return null;

            var schedulesResponce = JsonConvert.DeserializeObject<List<ScheduleResponce>>(stringResponce);

            if (schedulesResponce == null)
                return null;

            var scheduleForWeek = _scheduleMapper.Map(schedulesResponce);

            return scheduleForWeek;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception: {ex}", ex.Message);

            return null;
        }
    }
}
