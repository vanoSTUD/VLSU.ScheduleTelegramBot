using Microsoft.Extensions.Logging;
using VLSU.ScheduleTelegramBot.Domain.Dto;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Repositories;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application.Services;

public class AppUserService : IAppUserService
{
    private readonly IBaseRepository<AppUser> _userRepository;
    private readonly ILogger<AppUserService> _logger;

    public AppUserService(IBaseRepository<AppUser> userRepository, ILogger<AppUserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AppUser> GetOrCreateAsync(long chatId)
    {
        try
        {
            var foundedUser = _userRepository.GetAll().FirstOrDefault(u => u.ChatId == chatId);

            if (foundedUser == null)
                foundedUser = await _userRepository.CreateAsync(new AppUser() { ChatId = chatId });

            return foundedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {name}, Message: {message}",nameof(AppUserService.GetOrCreateAsync), ex.Message);
            throw;
        }
    }

    public async Task UpdateAsync(UpdateAppUser update)
    {
        try
        {
            var foundedUser = _userRepository.GetAll().FirstOrDefault(u => u.ChatId == update.ChatId);

            if (foundedUser == null)
                foundedUser = await _userRepository.CreateAsync(new AppUser() { ChatId = update.ChatId});

            foundedUser.LooksAtTeachers = update.LooksAtTeachers;
            await _userRepository.UpdateAsync(foundedUser);            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {Class}.{Method}, Message: {Message}", nameof(AppUserService), nameof(UpdateAsync), ex.Message);
        }
    }
}
