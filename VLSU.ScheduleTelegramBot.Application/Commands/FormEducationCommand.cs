using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Application.Commands;

public class FormEducationCommand : BaseCommand
{
    private readonly ITelegramBotClient _bot;

    public FormEducationCommand(ITelegramBotClient bot)
    {
        _bot = bot;
    }

    public override string Name => CommandNames.FormEducation;

    public override async Task ExecuteAsync(Update update, string[]? args = default)
    {
        if (update.CallbackQuery is not { } callback)
            return;

        if (callback.Message is not { } message)
            return;

        var responceMessage = "Выбери форму обучения:";

        var inlineMarkup = new InlineKeyboardMarkup()
            .AddNewRow().AddButton("Очная", $"{CommandNames.ShowInstitutes} {(int)EducationForms.FullTime}")
            .AddNewRow().AddButton("Заочная", $"{CommandNames.ShowInstitutes} {(int)EducationForms.ParteTime}")
            .AddNewRow().AddButton("Очно-Заочная", $"{CommandNames.ShowInstitutes} {(int)EducationForms.Mixed}");

        await _bot.SendTextMessageAsync(message.Chat, responceMessage, replyMarkup: inlineMarkup, parseMode: ParseMode.Html);
    }
}
