﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using System.Text;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Application.Commands.Group;

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
				args.Length < 4 ||
				!long.TryParse(args[0], out long id) ||
				!Enum.TryParse(typeof(EducationWeekTypes), args[1], out var educationWeekType) ||
                !Enum.TryParse(typeof(Roles), args[2], out var role) ||
				string.IsNullOrEmpty(args[3]))
			{
				_logger.LogWarning("Agguments are null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			using var scope = _scopeFactory.CreateScope();
			var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();

			var schedule = await vlsuApi.GetScheduleAsync(id, (Roles)role);
			var currentInfo = await vlsuApi.GetCurrentInfoAsync(id, (Roles)role);
			var name = string.Join(' ', args.Skip(3));

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

			var scheduleMessage = GetScheduleMessage((EducationWeekTypes)educationWeekType, currentInfo, schedule, name);

			await SendMessageWithButtonsAsync(_bot, message.Chat.Id, scheduleMessage, id, (Roles)role, (EducationWeekTypes)currentInfo!.CurrentWeekType, name);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception on ShowScheduleCommand.ExecuteAsync()");

			await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить расписание. Попробуйте позже</b>", parseMode: ParseMode.Html);
		}
	}

    public static async Task SendMessageWithButtonsAsync(ITelegramBotClient bot, ChatId chatId, string message, long id, Roles role, EducationWeekTypes currentEducationWeekType, string name)
	{
		var nextEducationWeekType = currentEducationWeekType == EducationWeekTypes.Denominator ? EducationWeekTypes.Nominator : EducationWeekTypes.Denominator;
		var inlineMarkup = new InlineKeyboardMarkup();
		inlineMarkup
			.AddNewRow().AddButton($"Текущая неделя", $"{CommandNames.ShowSchedule} {id} {(int)currentEducationWeekType} {(int)role} {name}")
			.AddNewRow().AddButton($"Следующая неделя", $"{CommandNames.ShowSchedule} {id} {(int)nextEducationWeekType} {(int)role} {name}");

		await bot.SendTextMessageAsync(chatId, message, replyMarkup: inlineMarkup, parseMode: ParseMode.Html);
	}

    private static string GetScheduleMessage(EducationWeekTypes educationWeekType, CurrentInfo currentInfo, ScheduleForWeek schedule, string name)
    {
        DateTime todayDate = DateTime.Today;
        DateTime weekBeginDate = todayDate.AddDays(-(int)todayDate.DayOfWeek).AddDays(1);

        if (todayDate.DayOfWeek == DayOfWeek.Sunday)
            weekBeginDate = weekBeginDate.AddDays(-7);

        DateTime weekEndDate = weekBeginDate.AddDays(6);
        string weekName = "Текущая неделя";

        if (educationWeekType != (EducationWeekTypes)currentInfo.CurrentWeekType)
        {
            weekBeginDate = weekBeginDate.AddDays(7);
            weekEndDate = weekEndDate.AddDays(7);
            weekName = "Следующая неделя";
        }

        string educationWeekTypeName = educationWeekType == EducationWeekTypes.Nominator ? "Числитель" : "Знаменатель";

        var scheduleMessage = new StringBuilder($"Расписание для <b>{name}</b> \n");
        scheduleMessage.AppendLine($"Тип недели: <b>{educationWeekTypeName}</b>.");
        scheduleMessage.AppendLine($"Дата: <b>{weekBeginDate.ToString("d")} - {weekEndDate.ToString("d")}</b>.");
        scheduleMessage.AppendLine($"({weekName})");

        var currentSchedules = schedule.GetSchedules(educationWeekType);

        foreach (var currentSchedule in currentSchedules)
        {
            scheduleMessage.AppendLine($"\n<blockquote expandable><u>{currentSchedule.DayOfWeek}</u>");
            var lessons = currentSchedule.Lessons;

            foreach (var lesson in lessons)
            {
                AppendLesson(scheduleMessage, lesson, lesson.Number);
            }

            scheduleMessage.Append("</blockquote>");
        }

		return scheduleMessage.ToString();
    }

	private static void AppendLesson(StringBuilder builder, Lesson lesson, int lessonNumber)
	{
		if (lesson == null)
			return;

		string[] sportLessonNames = { "Элективные дисциплины по физической культуре", "Физическая культура и спорт" };
		string defaultLessonType = "лк";
		string lessonType = defaultLessonType;

        if (lesson.Description.Contains(','))
		{
			var lessonDescriptionSplit = lesson.Description.Split(',');
			lessonType = lessonDescriptionSplit[0];
        }

		//🔬📝📌🛠🤸‍♀️
		string emoji = lessonType switch
		{
			"лк" => "📝",
			"пр" => "📌",
			"лб" => "🔬",
			_ => "📝"
		};

		foreach(var lessonName in sportLessonNames)
            if (lesson.Description.Contains(lessonName))
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
