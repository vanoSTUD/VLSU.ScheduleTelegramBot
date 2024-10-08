using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class ShowInstitutesCommand : BaseCommand
{
	private readonly ITelegramBotClient _bot;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<ShowInstitutesCommand> _logger;

	public ShowInstitutesCommand(ITelegramBotClient bot, IServiceScopeFactory scopeFactory, ILogger<ShowInstitutesCommand> logger)
	{
		_bot = bot;
		_logger = logger;
		_scopeFactory = scopeFactory;
	}

	public override string Name => CommandNames.ShowInstitutes;

	public override async Task ExecuteAsync(Update update, string[]? args = default)
	{
		if (update.CallbackQuery is not { } callback)
			return;

		if (callback.Message is not { } message)
			return;

		try
		{
			using var scope = _scopeFactory.CreateScope();
			var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();

			var institutes = await vlsuApi.GetInstitutesAsync();

			if (institutes == null)
			{
				_logger.LogWarning("institutes are null. Args = {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить институты. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			foreach (var institute in institutes)
			{
				var instituteFullName = institute.Text;
				var splitName = instituteFullName.Split(' ');
				var resultName = new StringBuilder();

				foreach (var word in splitName)
				{
					if (word.Length > 1)
						resultName.Append(word[0].ToString().ToUpper());
					else
						resultName.Append(word);
				}

				institute.Text = resultName.ToString();
			}

			var inlineMarkup = new InlineKeyboardMarkup();

			for (int i = 0; i < institutes.Count; i++)
			{
				if (i == 0 || i % 4 == 0)
					inlineMarkup.AddNewRow();

				var educationForm = args?[0];
				var instituteId = institutes[i].Value;
				var arguments = $"{educationForm} {instituteId}";

				inlineMarkup.AddButton(institutes[i].Text, $"{CommandNames.ShowCourses} {arguments}");
			}

			await _bot.SendTextMessageAsync(message.Chat, "<i>Выберите институт:</i>", replyMarkup: inlineMarkup, parseMode: ParseMode.Html);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception on ShowInstitutesCommand.ExecuteAsync()");

			await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить институты. Попробуйте позже</b>", parseMode: ParseMode.Html);
		}
	}
}
