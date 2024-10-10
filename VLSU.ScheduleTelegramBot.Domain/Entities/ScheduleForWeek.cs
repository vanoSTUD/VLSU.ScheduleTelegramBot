using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Domain.Entities;

public class ScheduleForDay
{
    public ScheduleForDay(string dayOfWeek, WeekTypes weekType, List<Lesson> lessons, EducationWeekTypes educationWeekType)
    {
        DayOfWeek = dayOfWeek;
        Lessons = lessons;
        EducationWeekType = educationWeekType;
        WeekType = weekType;
    }
    
    public WeekTypes WeekType { get; }
    public EducationWeekTypes EducationWeekType { get; }
    public string DayOfWeek { get; }
    public List<Lesson> Lessons { get; }
}

public class ScheduleForWeek
{
    private readonly List<ScheduleForDay> _schedules;

    public ScheduleForWeek(List<ScheduleForDay> schedules)
    {
        _schedules = schedules;
    }

    public List<ScheduleForDay> GetSchedules(EducationWeekTypes weekType)
    {
        return _schedules.Where(s => s.EducationWeekType == weekType).ToList();
    }
}