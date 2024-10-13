using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using VLSU.ScheduleTelegramBot.Domain.Dto;
using Microsoft.Extensions.Logging;

namespace VLSU.ScheduleTelegramBot.Application.Commands.Teacher;

public class FindTeacherCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FindTeacherCommand> _logger;

    public FindTeacherCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory, ILogger<FindTeacherCommand> logger)
    {
        _bot = bot;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override string Name => CommandNames.FindTeacher;

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.CallbackQuery?.Message is not { } message)
            return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IAppUserService>();

            var user = await userService.GetOrCreateAsync(message.Chat.Id, cancellationToken);
            await userService.UpdateAsync(new UpdateAppUser()
            {
                ChatId = message.Chat.Id,
                LooksAtTeachers = true
            }, cancellationToken);

            var responceMessage = $"""
            Напиши ФИО преподавателя или его часть (минимум 3 символа)
            """;

            await _bot.SendTextMessageAsync(message.Chat, responceMessage, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
        }
        catch 
        {
            await _bot.SendTextMessageAsync(message.Chat, "Не удалось получить данные от сервера :( \nПопробуйте позже", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

            throw;
        }
    }
}
