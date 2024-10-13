using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class UndefindCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;

    public UndefindCommand(ITelegramBotClient bot)
    {
        _bot = bot;
    }

    public override string Name => CommandNames.Undefind;

    public override async Task ExecuteAsync(Update update, string[]? args = default)
    {
        if (update.Message is not { } message)
            return;

        string responceMessage = $"Не распознал команду! \nПопробуй {CommandNames.Start}";

        await _bot.SendTextMessageAsync(message.Chat, responceMessage, parseMode: ParseMode.Html);
    }
}
