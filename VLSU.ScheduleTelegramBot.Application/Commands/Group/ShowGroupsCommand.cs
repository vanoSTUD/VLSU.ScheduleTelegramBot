using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application.Commands.Group;

public class ShowGroupsCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<ShowGroupsCommand> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ShowGroupsCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory, ILogger<ShowGroupsCommand> logger)
    {
        _bot = bot;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override string Name => CommandNames.ShowGroups;

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.CallbackQuery is not { } callback)
            return;

        if (callback.Message is not { } message)
            return;

        try
        {
            if (args == null ||
                args.Length < 3 ||                
                !int.TryParse(args[0], out int educationForm) ||
                !long.TryParse(args[1], out long instituteId) ||
                args[2] is not { } course)
            {
                _logger.LogWarning("Agguments are null: {args}", args?.ToString());
                await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить группы. Попробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();
            var groupsResult = await vlsuApi.GetGroupsAsync(instituteId, educationForm, cancellationToken);

            if (groupsResult.IsFailure)
            {
                _logger.LogWarning("VlsuApi returns failure Groups. Args: {args}", args?.ToString());
                await _bot.SendTextMessageAsync(message.Chat, $"<b>{groupsResult.ErrorMessage}</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                return;
            }

            var inlineMarkup = new InlineKeyboardMarkup();
            var groups = groupsResult.Value!.Where(g => g.Course.StartsWith(course)).ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                if (i == 0 || i % 3 == 0)
                    inlineMarkup.AddNewRow();

                var groupId = groups[i].Id;
                var groupName = groups[i].Name;
                var arguments = $"{groupId} {Roles.Group} {groupName}";

                inlineMarkup.AddButton(groups[i].Name, $"{CommandNames.ShowWeeks} {arguments}");
            }

            var responceMessage = "Выбери желаемую группу: ";

            await _bot.SendTextMessageAsync(message.Chat, responceMessage, replyMarkup: inlineMarkup, parseMode: ParseMode.Html, cancellationToken: cancellationToken);

        }
        catch
        {
            await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить группы. Попробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

            throw;
        }
    }
}
