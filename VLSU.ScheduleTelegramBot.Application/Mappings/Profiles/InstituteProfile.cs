using AutoMapper;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Responces;

namespace VLSU.ScheduleTelegramBot.Application.Mappings.Profiles;

public class InstituteProfile : Profile
{
    public InstituteProfile()
    {
        CreateMap<InstituteInfoResponce, Institute>()
            .ForMember(destination => destination.Id, opt =>
                opt.MapFrom(source => source.Value))
            .ForMember(destination => destination.Name, opt =>
                opt.MapFrom(source => source.Text));
    }
}
