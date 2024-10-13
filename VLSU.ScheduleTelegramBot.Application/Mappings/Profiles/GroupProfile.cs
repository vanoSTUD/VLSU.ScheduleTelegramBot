using AutoMapper;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Responces;

namespace VLSU.ScheduleTelegramBot.Application.Mappings.Profiles;

public class GroupProfile : Profile
{
    public GroupProfile()
    {
        CreateMap<GroupInfoResponce, Group>()
            .ForMember(destination => destination.Id, opt =>
                opt.MapFrom(source => source.Nrec));
    }
}
