using VLSU.ScheduleTelegramBot.Domain.Dto;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.ResultPattern;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

public interface IAppUserService
{
    public Task<Result<AppUser>> GetOrCreateAsync(long chatId, CancellationToken ct = default);

    public Task<Result<AppUser>> UpdateAsync(UpdateAppUser updateAppUser, CancellationToken ct = default);
}
