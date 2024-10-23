using VLSU.ScheduleTelegramBot.Application.Helpers;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Enums;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Mappings;
using VLSU.ScheduleTelegramBot.Domain.Responces;

namespace VLSU.ScheduleTelegramBot.Application.Mappings.CustomMappers;

public class SheduleMapper : IScheduleMapper
{
    public ScheduleForWeek? Map(List<ScheduleResponce>? schedulesResponce)
    {
        if (schedulesResponce == null)
            return null;

        var schedulesForDays = new List<ScheduleForDay>();
        var scheduleResponceProperties = typeof(ScheduleResponce).GetProperties();

        foreach (var schedule in schedulesResponce)
        {
            var nominatorWeekLessons = new List<Lesson>();
            var denominatorWeekLessons = new List<Lesson>();

            foreach (var property in scheduleResponceProperties)
            {
                if (property.Name == nameof(ScheduleResponce.name))
                    continue;

                var lessonDescription = (string?)schedule.GetType().GetProperty(property.Name)?.GetValue(schedule, null);
                var lessonNumber = int.Parse(property.Name[1].ToString());

                if (string.IsNullOrEmpty(lessonDescription))
                    continue;

                if (property.Name.StartsWith('n'))
                    nominatorWeekLessons.Add(new Lesson(lessonDescription, lessonNumber));
                else if (property.Name.StartsWith('z'))
                    denominatorWeekLessons.Add(new Lesson(lessonDescription, lessonNumber));
            }

            schedulesForDays.Add(new ScheduleForDay(schedule.name, WeekHelper.GetWeekType(schedule.name), nominatorWeekLessons, EducationWeekTypes.Nominator));
            schedulesForDays.Add(new ScheduleForDay(schedule.name, WeekHelper.GetWeekType(schedule.name), denominatorWeekLessons, EducationWeekTypes.Denominator));
        }

        var scheduleForWeek = new ScheduleForWeek(schedulesForDays);

        return scheduleForWeek;
    }
}

