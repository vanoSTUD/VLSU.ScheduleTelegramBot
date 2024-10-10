using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Application.Helpers;

public static class WeekHelper
{
    public static WeekTypes GetWeekType(string weekName)
    {
        switch (weekName)
        {
            case "Понедельник":
                return WeekTypes.Monday;
            case "Вторник":
                return WeekTypes.Tuesday;
            case "Среда":
                return WeekTypes.Wednesday;
            case "Четверг":
                return WeekTypes.Thursday;
            case "Пятница":
                return WeekTypes.Friday;
            case "Суббота":
                return WeekTypes.Saturday;
        }

        return WeekTypes.Undefined;
    }
}
