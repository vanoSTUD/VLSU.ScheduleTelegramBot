namespace VLSU.ScheduleTelegramBot.Domain.Contracts;

public class Group
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
}
