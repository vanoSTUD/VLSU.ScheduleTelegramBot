using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using VLSU.ScheduleTelegramBot.Application.Commands;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
using VLSU.ScheduleTelegramBot.Domain.Dto;
using Telegram.Bot.Polling;


namespace VLSU.ScheduleTelegramBot.API;

public class UpdateHandler : IUpdateHandler
{
	private readonly ITelegramBotClient _bot;
	private readonly ILogger<UpdateHandler> _logger;
	private readonly IServiceScopeFactory _scopeFactory;

	private readonly List<BaseCommand> _commands;

    public UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger, IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
    {
        _bot = bot;
        _logger = logger;
        _scopeFactory = scopeFactory;

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

		using var scope = _scopeFactory.CreateScope();
		var userService = scope.ServiceProvider.GetRequiredService<IAppUserService>();
		var user = await userService.GetOrCreateAsync(message.Chat.Id);

        if (user.LooksAtTeachers)
		{
            await _commands.First(command => command.Name == CommandNames.ShowTeachersCount).ExecuteAsync(update);
			return;
        }

		if (messageText.StartsWith('/'))
		{
			string commandName = messageText.Split(' ')[0];

			await ExecuteCommand(commandName, update);
		}
		else
		{
			await _commands.First(command => command.Name == CommandNames.Undefind).ExecuteAsync(update);
		}
	}

	private async Task HandleCallbackQuery(Update update)
	{
		if (update.CallbackQuery is not { } callback)
			return;

		if (callback.Data is not { } data)
			return;

        await ExecuteCommand(data, update);

        await _bot.AnswerCallbackQueryAsync(callback.Id);
	}

	private async Task ExecuteCommand(string command, Update update)
	{
		var foundedCommand = _commands.FirstOrDefault(c => c.Name == command.Split(' ')[0]);

		if (foundedCommand == null)
		{
            await _commands.First(command => command.Name == CommandNames.Undefind).ExecuteAsync(update);
			return;
		}

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
