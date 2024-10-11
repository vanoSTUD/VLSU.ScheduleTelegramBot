namespace VLSU.ScheduleTelegramBot.Domain.Entities;

public class AppUser
{
    public long ChatId { get; set; }
    public bool LooksAtTeachers { get; set; } = false;
}
