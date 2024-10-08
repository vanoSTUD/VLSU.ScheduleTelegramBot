namespace VLSU.ScheduleTelegramBot.Domain.Entities;

public class Group
{
	public string Course { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public long Nrec { get; set; }
}
