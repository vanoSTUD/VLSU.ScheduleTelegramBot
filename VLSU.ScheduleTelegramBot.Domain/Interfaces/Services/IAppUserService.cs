using VLSU.ScheduleTelegramBot.Domain.Dto;
using VLSU.ScheduleTelegramBot.Domain.Entities;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IAppUserService
{
    public Task<AppUser> GetOrCreateAsync(long chatId, CancellationToken ct = default);

    public Task UpdateAsync(UpdateAppUser updateAppUser, CancellationToken ct = default);
}
