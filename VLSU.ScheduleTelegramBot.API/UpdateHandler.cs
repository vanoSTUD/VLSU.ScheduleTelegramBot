using Telegram.Bot.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using VLSU.ScheduleTelegramBot.Application.Commands;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;
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
		if (exception is OperationCanceledException or TaskCanceledException)
		{
			_logger.LogWarning("Handle {name}: {Message}", nameof(exception), exception.Message);
			return;
		}

		_logger.LogError("HandleError: {Exception}", exception);

		if (exception is RequestException)
			await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
	}

	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		await HandleMessage(update, cancellationToken);

		await HandleCallbackQuery(update, cancellationToken);
	}

	private async Task HandleMessage(Update update, CancellationToken cancellationToken)
	{
		if (update.Message is not { } message)
			return;

		if (message.Text is not { } messageText)
			return;

		using var scope = _scopeFactory.CreateScope();
		var userService = scope.ServiceProvider.GetRequiredService<IAppUserService>();
		var user = await userService.GetOrCreateAsync(message.Chat.Id);

		if (messageText.StartsWith('/'))
		{
			string commandName = messageText.Split(' ')[0];

			await ExecuteCommandAsync(commandName, update, cancellationToken);
		}
        else if (user.FindsTeacherSchedule)
        {
            await _commands.First(command => command.Name == CommandNames.ShowTeachersCount).ExecuteAsync(update, cancellationToken);
            return;
        }
        else
		{
			await _commands.First(command => command.Name == CommandNames.Undefind).ExecuteAsync(update, cancellationToken);
		}
	}

	private async Task HandleCallbackQuery(Update update, CancellationToken cancellationToken)
	{
		if (update.CallbackQuery is not { } callback)
			return;

		if (callback.Data is not { } data)
			return;

        await ExecuteCommandAsync(data, update, cancellationToken);

        await _bot.AnswerCallbackQueryAsync(callback.Id, cancellationToken: cancellationToken);
	}

	private async Task ExecuteCommandAsync(string command, Update update, CancellationToken cancellationToken)
	{
		var foundedCommand = _commands.FirstOrDefault(c => c.Name == command.Split(' ')[0]);

		if (foundedCommand == null)
		{
            await _commands.First(command => command.Name == CommandNames.Undefind).ExecuteAsync(update, cancellationToken);
			return;
		}

        var args = command.Split(' ').Skip(1).ToArray();

		await foundedCommand.ExecuteAsync(update, cancellationToken, args);
	}
}
