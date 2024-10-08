using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using System.Text;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class ShowScheduleCommand : BaseCommand
{
	private readonly ITelegramBotClient _bot;
	private readonly ILogger<ShowGroupsCommand> _logger;
	private readonly IServiceScopeFactory _scopeFactory;

	public ShowScheduleCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory, ILogger<ShowGroupsCommand> logger)
	{
		_bot = bot;
		_logger = logger;
		_scopeFactory = scopeFactory;
	}

	public override string Name => CommandNames.ShowSchedule;

	public override async Task ExecuteAsync(Update update, string[]? args = default)
	{
		if (update.CallbackQuery is not { } callback)
			return;

		if (callback.Message is not { } message)
			return;

		try
		{
			await _bot.AnswerCallbackQueryAsync(callback.Id);

			if (!long.TryParse(args?[0], out long groupId) ||
				!int.TryParse(args?[1], out int weekIncrement))
			{
				_logger.LogWarning("Agguments are null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			using var scope = _scopeFactory.CreateScope();
			var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();
			var groupInfo = await vlsuApi.GetCurrentInfo(groupId);

			if (groupInfo == null)
			{
				_logger.LogWarning("Vlsu Api returns null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось получить информацию о группе</b>", parseMode: ParseMode.Html);

				return;
			}

			var weekType = groupInfo.CurrentWeekType;
			var schedule = await vlsuApi.GetScheduleAsync(groupId, weekType);

			if (schedule == null)
			{
				_logger.LogWarning("Vlsu Api returns null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, $"<b>Не удалось отобразить расписание {groupInfo.Name}. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			if (schedule.Count == 0)
			{
				await _bot.SendTextMessageAsync(message.Chat, $"<b>Расписание {groupInfo.Name} на эту неделю не найдено</b>", parseMode: ParseMode.Html);

				return;
			}

			DateTime today = DateTime.Today;
			DateTime weekBegin = today.AddDays(-(int)today.DayOfWeek).AddDays(1);
			DateTime weekEnd = weekBegin.AddDays(6);
			string weekName = "Текущая неделя";

			if (weekIncrement != 0)
			{
				weekBegin = weekBegin.AddDays(7 * weekIncrement);
				weekEnd = weekEnd.AddDays(7 * weekIncrement);
				weekName = weekIncrement == 1 ? "Следующая неделя" : "Предыдущая неделя";

				weekType = weekType == 1 ? 2 : 1;
			}

			var result = new StringBuilder($"Расписание для <b>{groupInfo?.Name}.</b> \n");
			result.AppendLine($"Тип недели: <b>{(weekType == 1 ? "Числитель" : "Знаменатель")}</b>.");
			result.AppendLine($"Дата: <b>{weekBegin.ToString("d")} - {weekEnd.ToString("d")}</b>.");
			result.AppendLine($"({weekName})");

			foreach (var scheduleItem in schedule)
			{
				result.AppendLine($"\n<blockquote expandable><u>{scheduleItem.name}</u>");

				if (weekType == 1)
				{
					AppendLessonIfNotNull(result, scheduleItem.n1, 1);
					AppendLessonIfNotNull(result, scheduleItem.n2, 2);
					AppendLessonIfNotNull(result, scheduleItem.n3, 3);
					AppendLessonIfNotNull(result, scheduleItem.n4, 4);
					AppendLessonIfNotNull(result, scheduleItem.n5, 5);
					AppendLessonIfNotNull(result, scheduleItem.n6, 6);
				}
				if (weekType == 2)
				{
					AppendLessonIfNotNull(result, scheduleItem.z1, 1);
					AppendLessonIfNotNull(result, scheduleItem.z2, 2);
					AppendLessonIfNotNull(result, scheduleItem.z3, 3);
					AppendLessonIfNotNull(result, scheduleItem.z4, 4);
					AppendLessonIfNotNull(result, scheduleItem.z5, 5);
					AppendLessonIfNotNull(result, scheduleItem.z6, 6);
				}

				result.Append("</blockquote>");
			}

			await SendMessageWithButtonsAsync(_bot, message.Chat.Id, result.ToString(), groupId, weekType);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception on ShowScheduleCommand.ExecuteAsync()");

			await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);
		}
	}

	public static async Task SendMessageWithButtonsAsync(ITelegramBotClient bot, ChatId chatId, string message, long groupId, int weekType)
	{
		var inlineMarkup = new InlineKeyboardMarkup()
			.AddNewRow().AddButton($"Текущая неделя", $"{CommandNames.ShowSchedule} {groupId} 0")
			.AddNewRow().AddButton($"Предыдущая неделя", $"{CommandNames.ShowSchedule} {groupId} -1")
						.AddButton($"Следующая неделя", $"{CommandNames.ShowSchedule} {groupId} 1");

		await bot.SendTextMessageAsync(chatId, message, replyMarkup: inlineMarkup, parseMode: ParseMode.Html);
	}

	private void AppendLessonIfNotNull(StringBuilder builder, string lessonDescription, int lessonNumber)
	{
		if (string.IsNullOrEmpty(lessonDescription))
			return;

		//🔬📝📌🛠
		//str.Substring(0, str.IndexOf('^'));
		string lessonType = lessonDescription.Substring(0, lessonDescription.IndexOf(','));
		string emoji = string.Empty;

		switch (lessonType)
		{
			case "лк":
				emoji = "📝";
				break;
			case "пр":
				emoji = "📌";
				break;
			case "лб":
				emoji = "🔬";
				break;
		}

		var firstLessonStartTime = new TimeOnly(8, 30);
		int lessonDuration = 90;
		int restDuration = 20;
		var lessonStartTime = firstLessonStartTime.AddMinutes((lessonDuration + restDuration) * (lessonNumber - 1));
		var lessonEndTime = lessonStartTime.AddMinutes(lessonDuration);

		builder.AppendLine($"➖<b>{lessonNumber} пара ({lessonStartTime} - {lessonEndTime}): {emoji}</b>");
		builder.AppendLine($"<i>{lessonDescription}</i>");
	}
}
