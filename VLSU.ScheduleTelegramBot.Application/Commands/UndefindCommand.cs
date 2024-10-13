using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Logging;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class UndefindCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<UndefindCommand> _logger;

    public UndefindCommand(ITelegramBotClient bot, ILogger<UndefindCommand> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    public override string Name => CommandNames.Undefind;

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken = default, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.Message is not { } message)
            return;

        string responceMessage = $"Не распознал команду! \nПопробуй {CommandNames.Start}";

        await _bot.SendTextMessageAsync(message.Chat, responceMessage, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
    }
}
