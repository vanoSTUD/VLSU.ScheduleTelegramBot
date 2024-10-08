using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class StartCommand : BaseCommand
{
	private readonly ITelegramBotClient _bot;

	public StartCommand(ITelegramBotClient bot)
	{
		_bot = bot;
	}

	public override string Name => CommandNames.Start;

	public override async Task ExecuteAsync(Update update, string[]? args = default)
	{
		if (update.Message is not { } message)
			return;

		var inlineMarkup = new InlineKeyboardMarkup()
			.AddNewRow().AddButton("Очная", $"{CommandNames.ShowInstitutes} 0")
			.AddNewRow().AddButton("Заочная", $"{CommandNames.ShowInstitutes} 1")
			.AddNewRow().AddButton("Очно-Заочная", $"{CommandNames.ShowInstitutes} 2");

		await _bot.SendTextMessageAsync(message.Chat, "<i>Выберите форму обучения:</i>", replyMarkup: inlineMarkup, parseMode: ParseMode.Html);
	}
}
