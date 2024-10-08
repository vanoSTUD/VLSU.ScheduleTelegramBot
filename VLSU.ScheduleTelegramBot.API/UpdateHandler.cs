using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Application.Commands;

namespace VLSU.ScheduleTelegramBot.API;

public class UpdateHandler : IUpdateHandler
{
	private readonly ITelegramBotClient _bot;
	private readonly ILogger<UpdateHandler> _logger;
	private readonly IServiceProvider _serviceProvider;
	private readonly IServiceScopeFactory _scopeFactory;

	private readonly List<BaseCommand> _commands;

	public UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger, IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider)
	{
		_bot = bot;
		_logger = logger;
		_scopeFactory = scopeFactory;
		_serviceProvider = serviceProvider;

		_commands = serviceProvider.GetServices<BaseCommand>().ToList();
	}

	public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
	{
		_logger.LogInformation("HandleError: {Exception}", exception);

		if (exception is RequestException)
			await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
	}

	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		await HandleMessage(update);

		await HandleCallbackQuery(update);

	}

	private async Task HandleMessage(Update update)
	{
		if (update.Message is not { } message)
			return;

		if (message.Text is not { } messageText)
			return;

		if (messageText.StartsWith('/'))
		{
			string commandName = messageText.Split(' ')[0];

			await ExecuteCommand(commandName, update);
		}
		else
		{
			await _commands.First(command => command.Name == CommandNames.Start).ExecuteAsync(update);
		}
	}

	private async Task HandleCallbackQuery(Update update)
	{
		if (update.CallbackQuery is not { } callback)
			return;

		if (callback.Data is not { } data)
			return;

		await ExecuteCommand(data, update);
	}

	private async Task ExecuteCommand(string command, Update update)
	{
		var foundedCommand = _commands.FirstOrDefault(c => c.Name == command.Split(' ')[0]);

		if (foundedCommand == null)
			return;

		var args = command.Split(' ').Skip(1).ToArray();

		await foundedCommand.ExecuteAsync(update, args);
	}

	private async Task<Message> SendInlineKeyboard(Message msg)
	{
		var inlineMarkup = new InlineKeyboardMarkup()
			.AddNewRow("1.1", "1.2", "1.3")
			.AddNewRow()
				.AddButton("WithCallbackData", "CallbackData")
				.AddButton(InlineKeyboardButton.WithUrl("WithUrl", "https://github.com/TelegramBots/Telegram.Bot"));

		return await _bot.SendTextMessageAsync(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);
	}
}
