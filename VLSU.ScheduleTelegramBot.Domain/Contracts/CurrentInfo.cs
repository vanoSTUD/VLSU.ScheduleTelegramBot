namespace VLSU.ScheduleTelegramBot.Domain.Contracts;

public class CurrentInfo
{
    public string CurrentLesson { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CurrentSemester { get; set; }
    public int CurrentWeekType { get; set; }

}
