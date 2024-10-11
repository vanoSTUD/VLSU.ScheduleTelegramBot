using AutoMapper;
using VLSU.ScheduleTelegramBot.Domain.Contracts;
using VLSU.ScheduleTelegramBot.Domain.Responces;

namespace VLSU.ScheduleTelegramBot.Application.Mappings.Profiles;

public class TeacherProfile : Profile
{
    public TeacherProfile()
    {
        CreateMap<TeacherInfoResponce, Teacher>()
            .ForMember(destination => destination.Id, opt =>
                opt.MapFrom(source => source.Nrec))
            .ForMember(destination => destination.Fullname, opt =>
                opt.MapFrom(source => source.FIO));
    }
}
