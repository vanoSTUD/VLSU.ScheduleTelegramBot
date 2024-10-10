using VLSU.ScheduleTelegramBot.Domain.Dto;
using VLSU.ScheduleTelegramBot.Domain.Entities;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IAppUserService
{
    public Task<AppUser> GetOrCreateAsync(long chatId);

    public Task UpdateAsync(UpdateAppUser update);
}
