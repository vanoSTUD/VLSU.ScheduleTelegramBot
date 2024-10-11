using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Application.Helpers;

public static class WeekHelper
{
    public static WeekTypes GetWeekType(string weekName)
    {
        return weekName switch
        {
            "Понедельник" => WeekTypes.Monday,
            "Вторник" => WeekTypes.Tuesday,
            "Среда" => WeekTypes.Wednesday,
            "Четверг" => WeekTypes.Thursday,
            "Пятница" => WeekTypes.Friday,
            "Суббота" => WeekTypes.Saturday,
            _ => WeekTypes.Undefined,
        };
    }
}
