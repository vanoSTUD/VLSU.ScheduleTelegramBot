namespace VLSU.ScheduleTelegramBot.API.Settings;

public class BotOptions
{
	public const string Section = nameof(BotOptions);

	public string BotToken { get; init; } = default!;
	public Uri BotWebhookUrl { get; init; } = default!;
	public string SecretToken { get; init; } = default!;
}
