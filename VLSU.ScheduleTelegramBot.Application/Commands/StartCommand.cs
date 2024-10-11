using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VLSU.ScheduleTelegramBot.Domain.Enums;

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
		Message? message = null;

		if (update.Message is not null)
			message = update.Message;
		else if (update?.CallbackQuery?.Message is not null)
			message = update.CallbackQuery.Message;

		if (message == null)
			return;

		var userName = message.From?.FirstName;
		string responceMessage = $"Привет{(userName is null ? "" : $", <b>{userName}</b>")}!\n";
        responceMessage += "Чье расписание смотрим?";

		var inlineMarkup = new InlineKeyboardMarkup()
			.AddNewRow().AddButton("Студента", $"{CommandNames.FormEducation}")
			.AddNewRow().AddButton("Преподавателя", $"{CommandNames.FindTeacher}");

		await _bot.SendTextMessageAsync(message.Chat, responceMessage, replyMarkup: inlineMarkup, parseMode: ParseMode.Html);
	}
}
