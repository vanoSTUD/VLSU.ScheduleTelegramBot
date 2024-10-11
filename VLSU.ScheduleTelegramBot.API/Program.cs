using Telegram.Bot;
using VLSU.ScheduleTelegramBot.API;
using VLSU.ScheduleTelegramBot.Application;
using VLSU.ScheduleTelegramBot.Application.Mappings.Profiles;
using VLSU.ScheduleTelegramBot.DAL;
using VLSU.ScheduleTelegramBot.Domain.Options;
using VLSU.ScheduleTelegramBot.VlsuApiService;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("MSSql")!;
var botConfigSection = configuration.GetSection(BotOptions.Section);

builder.Services.Configure<BotOptions>(botConfigSection);
builder.Services.Configure<VlsuApiOptions>(configuration.GetSection(VlsuApiOptions.Section));

builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient<ITelegramBotClient>(
	httpClient => new TelegramBotClient(botConfigSection.Get<BotOptions>()!.BotToken, httpClient));

builder.Services.AddApplicationServices();
builder.Services.AddVlguApiService();
builder.Services.AddMSSqlDbContext(connectionString);

builder.Services.AddSingleton<UpdateHandler>();
builder.Services.ConfigureTelegramBotMvc();

builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(GroupProfile), typeof(InstituteProfile), typeof(TeacherProfile));

var app = builder.Build();

app.MapControllers();

var bot = app.Services.GetRequiredService<ITelegramBotClient>();
var webhookUrl = botConfigSection.Get<BotOptions>()!.BotWebhookUrl.AbsoluteUri;
var secretToken = botConfigSection.Get<BotOptions>()!.SecretToken;
var ct = new CancellationTokenSource().Token;

await bot.SetWebhookAsync(webhookUrl, allowedUpdates: [], secretToken: secretToken, cancellationToken: ct);
Console.WriteLine($"Webhook set to {webhookUrl}");

app.Run();