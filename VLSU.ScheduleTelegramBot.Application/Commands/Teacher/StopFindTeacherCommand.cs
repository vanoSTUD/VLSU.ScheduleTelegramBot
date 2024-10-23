using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Dto;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application.Commands.Teacher;

public class StopFindTeacherCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;

    public StopFindTeacherCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
    }

    public override string Name => CommandNames.StopFindTeachers;

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.CallbackQuery is { } callback)
        {
            if (callback.Message is not { } message)
                return;

            using var scope = _scopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IAppUserService>();

            await userService.UpdateAsync(new UpdateAppUser()
            {
                ChatId = message.Chat.Id,
                FindsTeacherSchedule = false
            });

            await _bot.SendTextMessageAsync(message.Chat, "Поиск преподавателя остановлен", cancellationToken: cancellationToken);
        }
    }
}
