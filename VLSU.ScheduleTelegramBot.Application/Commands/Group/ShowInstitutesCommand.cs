using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application.Commands.Group;

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

    public override async Task ExecuteAsync(Update update, CancellationToken cancellationToken, string[]? args = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (update.CallbackQuery is not { } callback)
            return;

        if (callback.Message is not { } message)
            return;

        try
        {
            if (args == null ||
                args.Length < 1 ||
                !Enum.TryParse(typeof(EducationForms), args[0], out var educationForm))
            {
                _logger.LogWarning("Argument is not {type}. Args = {args}", nameof(EducationForms), args?.ToString());

                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var vlsuApi = scope.ServiceProvider.GetRequiredService<IVlsuApiService>();

            var institutesResponce = await vlsuApi.GetInstitutesAsync(cancellationToken);

            if (institutesResponce.IsFailure)
            {
                _logger.LogWarning("Institutes returns {output}. Args = {args}", institutesResponce.ErrorMessage, args?.ToString());
                await _bot.SendTextMessageAsync(message.Chat, $"<b>{institutesResponce.ErrorMessage} \nПопробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

                return;
            }

            var inlineMarkup = new InlineKeyboardMarkup();
            var institutes = institutesResponce.Value!;

            for (int i = 0; i < institutes.Count; i++)
            {
                if (i == 0 || i % 4 == 0)
                    inlineMarkup.AddNewRow();

                var instituteId = institutes[i].Id;
                var arguments = $"{(int)educationForm} {instituteId}";

                inlineMarkup.AddButton(institutes[i].GetShortName(), $"{CommandNames.ShowCourses} {arguments}");
            }

            var responceMessage = "Выбери желаемый институт: ";

            await _bot.SendTextMessageAsync(message.Chat, responceMessage, replyMarkup: inlineMarkup, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
        }
        catch
        {
            await _bot.SendTextMessageAsync(message.Chat, "<b>Не удалось отобразить институты. Попробуйте позже</b>", parseMode: ParseMode.Html, cancellationToken: cancellationToken);

            throw;
        }
    }
}
