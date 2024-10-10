﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Dto;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class StopFindTeacherCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FindTeacherCommand> _logger;

    public StopFindTeacherCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory, ILogger<FindTeacherCommand> logger)
    {
        _bot = bot;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override string Name => CommandNames.StopFindTeachers;

    public override async Task ExecuteAsync(Update update, string[]? args = default)
    {
        if (update.CallbackQuery is { } callback)
        {
            if (callback.Message is not { } message)
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IAppUserService>();

                var user = await userService.GetOrCreateAsync(message.Chat.Id);
                await userService.UpdateAsync(new UpdateAppUser()
                {
                    ChatId = message.Chat.Id,
                    LooksAtTeachers = false
                });

                await _bot.SendTextMessageAsync(message.Chat, "Поиск преподавателя остановлен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in {Class}.{Method}, Message: {Message}", nameof(StopFindTeacherCommand), nameof(ExecuteAsync), ex.Message);

                return;
            }
        }

    }
}