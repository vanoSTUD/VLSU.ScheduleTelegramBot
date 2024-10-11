namespace VLSU.ScheduleTelegramBot.Domain.Contracts;

public class Teacher
{
    public long Id { get; set; }
    public string Fullname { get; set; } = default!;

    public string GetShortName()
    {
        var fullnameSplit = Fullname.Split(' ');
        var shortName = $"{fullnameSplit[0]} {fullnameSplit[1].First()}. {fullnameSplit[2].First()}.";

        return shortName;
    }
}
