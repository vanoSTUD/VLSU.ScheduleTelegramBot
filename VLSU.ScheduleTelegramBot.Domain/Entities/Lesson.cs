namespace VLSU.ScheduleTelegramBot.Domain.Entities;

public class Lesson
{
    public Lesson(string name, int number)
    {
        Description = name;
        Number = number;
    }

    public string Description { get; }
    public int Number {  get; }
}
