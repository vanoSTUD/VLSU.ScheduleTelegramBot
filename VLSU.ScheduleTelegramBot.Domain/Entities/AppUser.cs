namespace VLSU.ScheduleTelegramBot.Domain.Entities;

public class AppUser
{
    public long ChatId { get; set; }
    public bool FindsTeacherSchedule { get; set; } = false;
}
