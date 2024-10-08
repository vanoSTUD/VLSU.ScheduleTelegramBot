using Microsoft.Extensions.DependencyInjection;
using VLSU.ScheduleTelegramBot.Application.Commands;

namespace VLSU.ScheduleTelegramBot.Application;

public static class DependencyInjection
{
	public static void AddApplicationServices(this IServiceCollection services)
	{
		services.AddSingleton<BaseCommand, StartCommand>();
		services.AddSingleton<BaseCommand, ShowInstitutesCommand>();
		services.AddSingleton<BaseCommand, ShowGroupsCommand>();
		services.AddSingleton<BaseCommand, ShowCourcesCommand>();
		services.AddSingleton<BaseCommand, ShowScheduleCommand>();
		services.AddSingleton<BaseCommand, ShowWeeksCommand>();
	}
}
