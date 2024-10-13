using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Application.Commands.Group;

namespace VLSU.ScheduleTelegramBot.Application.Commands.Teacher;

public class ShowTeachersCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<ShowGroupsCommand> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ShowTeachersCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory, ILogger<ShowGroupsCommand> logger)
    {
        _bot = bot;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override string Name => CommandNames.ShowTeachers;

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.CallbackQuery is not { } callback)
            return;

        if (callback.Message is not { } message)
            return;

        try
        {
            if (args?[0] == null || string.IsNullOrEmpty(args[0]))
            {
                _logger.LogWarning("Argument is null or empty. Args = {args}", args?.ToString());

                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();
            var userService = scope.ServiceProvider.GetRequiredService<IAppUserService>();

            var user = await userService.GetOrCreateAsync(message.Chat.Id, cancellationToken);

            var userText = string.Join(" ", args);
            var teachers = await vlsuApi.GetTeachersAsync(userText, cancellationToken);

            if (teachers == null)
            {
                await _bot.SendTextMessageAsync(message.Chat, "Не удалось получить данные :( \nПопробуйте позже", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                return;
            }

            if (teachers.Count == 0)
            {
                await _bot.SendTextMessageAsync(message.Chat, $"Препрдаватели с совпадением '{userText}' не найдены", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                return;
            }

            var inlineMarkup = new InlineKeyboardMarkup();

            for (int i = 0; i < teachers.Count; i++)
            {
                var teacherId = teachers[i].Id;
                var teacherName = teachers[i].GetShortName();
                var buttonArgs = $"{teacherId} {Roles.Teacher} {teacherName}";

                inlineMarkup.AddNewRow().AddButton(teachers[i].Fullname, $"{CommandNames.ShowWeeks} {buttonArgs}");
            }

            var responceMessage = "Выбери преподавателя: ";

            await _bot.SendTextMessageAsync(message.Chat, responceMessage, replyMarkup: inlineMarkup, parseMode: ParseMode.Html, cancellationToken: cancellationToken);

        }
        catch
        {
            await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить преподавателей :( \nПопробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

            throw;
        }
    }
}
