using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using System.Text;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Responces;
using VLSU.ScheduleTelegramBot.Domain.Entities;

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
			if (args == null ||
				args.Length < 3 ||
				!long.TryParse(args[0], out long id) ||
				!Enum.TryParse(typeof(EducationWeekTypes), args[1], out var educationWeekType) ||
                !Enum.TryParse(typeof(Roles), args[2], out var role))
			{
				_logger.LogWarning("Agguments are null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			using var scope = _scopeFactory.CreateScope();
			var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();
			var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();

			var schedule = await scheduleService.GetScheduleAsync(id, (Roles)role);
			CurrentInfo? currentInfo = null;

			if ((Roles)role == Roles.Teacher)
				currentInfo = await vlsuApi.GetTeacherInfoAsync(id);
			else if ((Roles)role == Roles.Group)
				currentInfo = await vlsuApi.GetGroupInfoAsync(id);

            if (schedule == null)
			{
				_logger.LogWarning("Vlsu Api returns null: Id = {id}, Role = {role}", id, role);
				await _bot.SendTextMessageAsync(message.Chat, $"<b>Расписание на эту неделю не найдено</b>", parseMode: ParseMode.Html);

				return;
			}

			if (currentInfo == null)
			{
                _logger.LogError("Vlsu Api returns null: Id = {id}", id);
                await _bot.SendTextMessageAsync(message.Chat, $"<b>Ошибка в получении расписания :( \nПопробуйте позже</b>", parseMode: ParseMode.Html);

                return;
            }

			DateTime today = DateTime.Today;
			DateTime weekBegin = today.AddDays(-(int)today.DayOfWeek).AddDays(1);
			DateTime weekEnd = weekBegin.AddDays(6);
			string weekName = "Текущая неделя";

            if ((int)educationWeekType != currentInfo.CurrentWeekType)
			{
				weekBegin = weekBegin.AddDays(7);
				weekEnd = weekEnd.AddDays(7);
				weekName = "Следующая неделя";
			}

			string educationWeekTypeName = (EducationWeekTypes)educationWeekType == EducationWeekTypes.Nominator ? "Числитель" : "Знаменатель";

            var responceMessage = new StringBuilder($"Расписание для <b>{currentInfo?.Name}.</b> \n");
			responceMessage.AppendLine($"Тип недели: <b>{educationWeekTypeName}</b>.");
			responceMessage.AppendLine($"Дата: <b>{weekBegin.ToString("d")} - {weekEnd.ToString("d")}</b>.");
			responceMessage.AppendLine($"({weekName})");

			var currentSchedules = schedule.GetSchedules((EducationWeekTypes)educationWeekType);

			foreach ( var currentSchedule in currentSchedules)
			{
                responceMessage.AppendLine($"\n<blockquote expandable><u>{currentSchedule.DayOfWeek}</u>");
				var lessons = currentSchedule.Lessons;

				foreach ( var lesson in lessons )
				{
                    AppendLesson(responceMessage, lesson, lesson.Number);
                }

                responceMessage.Append("</blockquote>");
            }

			await SendMessageWithButtonsAsync(_bot, message.Chat.Id, responceMessage.ToString(), id, (Roles)role, (EducationWeekTypes)currentInfo!.CurrentWeekType);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception on ShowScheduleCommand.ExecuteAsync()");

			await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);
		}
	}

	public static async Task SendMessageWithButtonsAsync(ITelegramBotClient bot, ChatId chatId, string message, long id, Roles role, EducationWeekTypes currentEducationWeekType)
	{
		var nextEducationWeekType = currentEducationWeekType == EducationWeekTypes.Denominator ? EducationWeekTypes.Nominator : EducationWeekTypes.Denominator;
		var inlineMarkup = new InlineKeyboardMarkup();
		inlineMarkup
			.AddNewRow().AddButton($"Текущая неделя", $"{CommandNames.ShowSchedule} {id} {currentEducationWeekType} {role}")
			.AddNewRow().AddButton($"Следующая неделя", $"{CommandNames.ShowSchedule} {id} {nextEducationWeekType} {role}");

		await bot.SendTextMessageAsync(chatId, message, replyMarkup: inlineMarkup, parseMode: ParseMode.Html);
	}

	private static void AppendLesson(StringBuilder builder, Lesson lesson, int lessonNumber)
	{
		if (lesson == null)
			return;

		string sportLessonName = "Элективные дисциплины по физической культуре и спорту";

        string lessonType = lesson.Description.Substring(0, lesson.Description.IndexOf(','));
		string emoji = "📝";

        //🔬📝📌🛠🤸‍♀️
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

		if (lesson.Description.Contains(sportLessonName))
            emoji = "🤸‍♀️";

        var firstLessonStartTime = new TimeOnly(8, 30);
		int lessonDuration = 90;
		int restDuration = 20;
		var lessonStartTime = firstLessonStartTime.AddMinutes((lessonDuration + restDuration) * (lessonNumber - 1));
		var lessonEndTime = lessonStartTime.AddMinutes(lessonDuration);

		builder.AppendLine($"➖<b>{lessonNumber} пара ({lessonStartTime} - {lessonEndTime}): {emoji}</b>");
		builder.AppendLine($"<i>{lesson.Description}</i>");
	}
}
