using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Responces;

namespace VLSU.ScheduleTelegramBot.Domain.Interfaces.Mappings;

public interface IScheduleMapper
{
    public ScheduleForWeek Map(List<ScheduleResponce> schedulesResponce);
}