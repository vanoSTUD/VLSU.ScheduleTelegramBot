using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Application.Commands.Group;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class ShowWeeksCommand : BaseCommand
{
	private readonly ITelegramBotClient _bot;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<ShowGroupsCommand> _logger;

	public ShowWeeksCommand(ITelegramBotClient bot, ILogger<ShowGroupsCommand> logger, IServiceScopeFactory scopeFactory)
	{
		_bot = bot;
		_logger = logger;
		_scopeFactory = scopeFactory;
	}

	public override string Name => CommandNames.ShowWeeks;

	public override async Task ExecuteAsync(Update update, string[]? args = default)
	{
		if (update.CallbackQuery is not { } callback)
			return;

		if (callback.Message is not { } message)
			return;

		try
		{
			if (!long.TryParse(args?[0], out long id)||
				!Enum.TryParse(typeof(Roles), args?[1], out var role))
			{
				_logger.LogWarning("Agguments are null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить учебные недели. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			using var scope = _scopeFactory.CreateScope();
			var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();
			CurrentInfo? currentInfo;


            currentInfo = await vlsuApi.GetCurrentInfoAsync(id, (Roles)role);


            if (currentInfo == null)
			{
				_logger.LogWarning("Vlsu Api returns null: {args}", args?.ToString());
				await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить учебные недели. Попробуйте позже</b>", parseMode: ParseMode.Html);

				return;
			}

			var currentEducationWeekType = (EducationWeekTypes)currentInfo.CurrentWeekType;
			var responceMessage = $"Выбери неделю для {currentInfo.Name}:";

            await ShowScheduleCommand.SendMessageWithButtonsAsync(_bot, message.Chat, responceMessage, id, (Roles)role, currentEducationWeekType);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception in {name}", nameof(ShowWeeksCommand.ExecuteAsync));

			await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить учебные недели. Попробуйте позже</b>", parseMode: ParseMode.Html);
		}
	}
}
