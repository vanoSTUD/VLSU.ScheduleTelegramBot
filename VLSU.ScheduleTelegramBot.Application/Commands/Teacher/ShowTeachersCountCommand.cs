﻿using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace VLSU.ScheduleTelegramBot.Application.Commands.Teacher;

public class ShowTeachersCountCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FindTeacherCommand> _logger;

    public ShowTeachersCountCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory, ILogger<FindTeacherCommand> logger)
    {
        _bot = bot;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override string Name => CommandNames.ShowTeachersCount;

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.Message is not { } message)
            return;

        if (message.Text is not { } userText)
            return;

        var stopButton = InlineKeyboardButton.WithCallbackData($"Остановить поиск преподавателя", CommandNames.StopFindTeachers);
        var stopMarkup = new InlineKeyboardMarkup().AddNewRow().AddButton(stopButton);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IAppUserService>();
            var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();

            var user = await userService.GetOrCreateAsync(message.Chat.Id, cancellationToken);

            if (user.FindsTeacherSchedule == false)
            {
                _logger.LogWarning("ShowTeachersCommand was called without the AppUser.LooksAtTeachers value. User: {user}", user.ChatId);
                return;
            }

            if (userText.Length < 3)
            {
                await _bot.SendTextMessageAsync(message.Chat, "Для поиска нужно минимум 3 буквы от ФИО преподавателя.", replyMarkup: stopMarkup, cancellationToken: cancellationToken);
                return;
            }

            var teachers = await vlsuApi.GetTeachersAsync(userText, cancellationToken);

            if (teachers == null)
            {
                await _bot.SendTextMessageAsync(message.Chat, "Не удалось получить данные :( \nПопробуйте позже", replyMarkup: stopMarkup, cancellationToken: cancellationToken);
                return;
            }

            if (teachers.Count == 0)
            {
                await _bot.SendTextMessageAsync(message.Chat, $"Препрдаватели с совпадением '{userText}' не найдены", replyMarkup: stopMarkup, cancellationToken: cancellationToken);
                return;
            }

            string responceMessage = $"Найдено преподавателей - {teachers.Count}. ";

            if (teachers.Count > 10)
                responceMessage += "Можешь их посмотреть или ввести более конкретные данные";
            else
                responceMessage += "Можешь их открыть, нажав на кнопку ниже";

            string buttonArgs = userText;
            var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow().AddButton($"Просмотр", $"{CommandNames.ShowTeachers} {buttonArgs}")
                .AddNewRow().AddButton(stopButton);

            await _bot.SendTextMessageAsync(message.Chat, responceMessage, replyMarkup: inlineMarkup, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
        }
        catch
        {
            await _bot.SendTextMessageAsync(message.Chat, "Не удалось получить данные от сервера :( \nПопробуйте позже", replyMarkup: stopMarkup, cancellationToken: cancellationToken);

            throw;
        }
    }
}
