using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace VLSU.ScheduleTelegramBot.Application.Commands.Group;

public class ShowCourcesCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ShowGroupsCommand> _logger;

    public ShowCourcesCommand(ITelegramBotClient bot, ILogger<ShowGroupsCommand> logger, IServiceScopeFactory scopeFactory)
    {
        _bot = bot;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override string Name => CommandNames.ShowCourses;

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.CallbackQuery is not { } callback)
            return;

        if (callback.Message is not { } message)
            return;

        try
        {
            if (!int.TryParse(args?[0], out int educationForm) ||
                !long.TryParse(args?[1], out long instituteId))
            {
                _logger.LogWarning("Agguments are null: {args}", args?.ToString());
                await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить курсы. Попробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();
            var groups = await vlsuApi.GetGroupsAsync(instituteId, educationForm, cancellationToken);

            if (groups == null)
            {
                _logger.LogWarning("Groups are null: {args}", args?.ToString());
                await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить курсы. Попробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                return;
            }

            if (groups.Count == 0)
            {
                await _bot.SendTextMessageAsync(message.Chat, "<b>Группы не найдены</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                return;
            }

            if (!int.TryParse(groups.Max(g => g.Course.Split(' ')[0]), out int maxCourse))
            {
                _logger.LogWarning("The course number could not be converted: {args}", args?.ToString());
                await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить курсы. Попробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                return;
            }

            var inlineMarkup = new InlineKeyboardMarkup();

            for (int i = 0; i < maxCourse; i++)
            {
                if (i == 0 || i % 2 == 0)
                    inlineMarkup.AddNewRow();

                var cource = i + 1;
                var arguments = $"{educationForm} {instituteId} {cource}";

                inlineMarkup.AddButton($"{cource} курс", $"{CommandNames.ShowGroups} {arguments}");
            }

            var responceMessage = "Выбери желаемый курс:";

            await _bot.SendTextMessageAsync(message.Chat, responceMessage, replyMarkup: inlineMarkup, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
        }
        catch
        {
            await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить курсы. Попробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

            throw;
        }
    }
}
