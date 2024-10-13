using System.Threading;
using Telegram.Bot.Types;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public abstract class BaseCommand
{
	public abstract string Name { get; }

	public abstract Task ExecuteAsync(Update update, CancellationToken cancellationToken = default, string[]? args = default);
}
