namespace VLSU.ScheduleTelegramBot.Application.Helpers;

public static class WeekHelper
{
    public static DayOfWeek GetWeekType(string weekTypeName)
    {
        return weekTypeName switch
        {
            "Понедельник" => DayOfWeek.Monday,
            "Вторник" => DayOfWeek.Tuesday,
            "Среда" => DayOfWeek.Wednesday,
            "Четверг" => DayOfWeek.Thursday,
            "Пятница" => DayOfWeek.Friday,
            "Суббота" => DayOfWeek.Saturday,
            _ => DayOfWeek.Sunday
        };
    }
}
