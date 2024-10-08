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
			if (!long.TryParse(args?[0], out long groupId) ||
				!int.TryParse(args?[1], out int weekType))
			{
				_logger.LogWarning("Agguments are null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			using var scope = _scopeFactory.CreateScope();
			var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();
			var schedule = await vlsuApi.GetScheduleAsync(groupId, weekType);

			if (schedule == null)
			{
				_logger.LogWarning("Vlsu Api returns null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			if (schedule.Count == 0)
			{
				await _bot.SendTextMessageAsync(message.Chat, "<b>Расписание на эту неделю не найдено</b>", parseMode: ParseMode.Html);

				return;
			}

			var result = new StringBuilder("Расписание на неделю: \n");

			foreach (var scheduleItem in schedule)
			{
				result.AppendLine($"\n<u>{scheduleItem.name}</u>");

				if (weekType == 1)
				{
					if (!string.IsNullOrEmpty(scheduleItem.n1))
					{
						result.AppendLine("<b>1 пара (8:30 - 10:00):</b>");
						result.AppendLine($"<i>{scheduleItem.n1}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.n2))
					{
						result.AppendLine("<b>2 пара (10:20 - 11:50):</b>");
						result.AppendLine($"<i>{scheduleItem.n2}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.n3))
					{
						result.AppendLine("<b>3 пара (12:10 - 13:40):</b>");
						result.AppendLine($"<i>{scheduleItem.n3}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.n4))
					{
						result.AppendLine("<b>4 пара (14:00 - 15:30):</b>");
						result.AppendLine($"<i>{scheduleItem.n4}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.n5))
					{
						result.AppendLine("<b>5 пара (15:50 - 17:20):</b>");
						result.AppendLine($"<i>{scheduleItem.n5}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.n6))
					{
						result.AppendLine("<b>6 пара (17:40 - 19:10):</b>");
						result.AppendLine($"<i>{scheduleItem.n6}</i>");
					}
				}
				if (weekType == 2)
				{
					if (!string.IsNullOrEmpty(scheduleItem.z1))
					{
						result.AppendLine("<b>1 пара (8:30 - 10:00):</b>");
						result.AppendLine($"<i>{scheduleItem.z1}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.z2))
					{
						result.AppendLine("<b>2 пара (10:20 - 11:50):</b>");
						result.AppendLine($"<i>{scheduleItem.z2}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.z3))
					{
						result.AppendLine("<b>3 пара (12:10 - 13:40):</b>");
						result.AppendLine($"<i>{scheduleItem.z3}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.z4))
					{
						result.AppendLine("<b>4 пара (14:00 - 15:30):</b>");
						result.AppendLine($"<i>{scheduleItem.z4}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.z5))
					{
						result.AppendLine("<b>5 пара (15:50 - 17:20):</b>");
						result.AppendLine($"<i>{scheduleItem.z5}</i>");
					}
					if (!string.IsNullOrEmpty(scheduleItem.z6))
					{
						result.AppendLine("<b>6 пара (17:40 - 19:10):</b>");
						result.AppendLine($"<i>{scheduleItem.z6}</i>");
					}
				}
			}

			var inlineMarkup = new InlineKeyboardMarkup();

			inlineMarkup
				.AddNewRow().AddButton($"Текущая неделя", $"{CommandNames.ShowSchedule} {groupId} {weekType}")
				.AddNewRow().AddButton($"Следующая неделя", $"{CommandNames.ShowSchedule} {groupId} {(weekType == 1 ? 2 : 1)}")
				.AddNewRow().AddButton($"Предыдущая неделя", $"{CommandNames.ShowSchedule} {groupId} {(weekType == 1 ? 2 : 1)}");


			await _bot.SendTextMessageAsync(message.Chat, result.ToString(), replyMarkup: inlineMarkup, parseMode: ParseMode.Html);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception on ShowScheduleCommand.ExecuteAsync()");

			await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);
		}
	}
}
