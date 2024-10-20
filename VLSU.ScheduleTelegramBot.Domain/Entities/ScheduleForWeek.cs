using VLSU.ScheduleTelegramBot.Domain.Enums;

namespace VLSU.ScheduleTelegramBot.Domain.Entities;

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
public class ScheduleForDay
{
    public ScheduleForDay(string dayName, DayOfWeek dayOfWeek, List<Lesson> lessons, EducationWeekTypes educationWeekType)
    {
        DayName = dayName;
        Lessons = lessons;
        EducationWeekType = educationWeekType;
        DayOfWeek = dayOfWeek;
    }
    
    public DayOfWeek DayOfWeek { get; }
    public EducationWeekTypes EducationWeekType { get; }
    public string DayName { get; }
    public List<Lesson> Lessons { get; }
}