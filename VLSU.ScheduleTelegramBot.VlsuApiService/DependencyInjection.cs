using Microsoft.Extensions.DependencyInjection;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Services;

namespace VLSU.ScheduleTelegramBot.VlsuApiService;

public static class DependencyInjection
{
	public static void AddVlguApiService(this IServiceCollection services)
	{
		services.AddScoped<IVlsuApiService, VlsuApiService>();
	}
}
