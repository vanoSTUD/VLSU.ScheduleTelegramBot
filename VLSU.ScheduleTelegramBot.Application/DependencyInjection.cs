using Microsoft.Extensions.DependencyInjection;
using VLSU.ScheduleTelegramBot.Application.Commands;
using VLSU.ScheduleTelegramBot.Application.Commands.Group;
using VLSU.ScheduleTelegramBot.Application.Commands.Teacher;
using VLSU.ScheduleTelegramBot.Application.Mappings.CustomMappers;
using VLSU.ScheduleTelegramBot.Application.Services;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Mappings;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.Application;

public static class DependencyInjection
{
	public static void AddApplicationServices(this IServiceCollection services)
	{
		services.AddScoped<IAppUserService, AppUserService>();

		services.AddScoped<IScheduleMapper, SheduleMapper>();

		services.AddSingleton<BaseCommand, StartCommand>();
		services.AddSingleton<BaseCommand, ShowInstitutesCommand>();
		services.AddSingleton<BaseCommand, ShowGroupsCommand>();
		services.AddSingleton<BaseCommand, ShowCourcesCommand>();
		services.AddSingleton<BaseCommand, ShowScheduleCommand>();
		services.AddSingleton<BaseCommand, ShowWeeksCommand>();
		services.AddSingleton<BaseCommand, FormEducationCommand>();
		services.AddSingleton<BaseCommand, UndefindCommand>();
		services.AddSingleton<BaseCommand, FindTeacherCommand>();
		services.AddSingleton<BaseCommand, ShowTeachersCommand>();
		services.AddSingleton<BaseCommand, ShowTeachersCountCommand>();
		services.AddSingleton<BaseCommand, StopFindTeacherCommand>();
	}
}
