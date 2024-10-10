using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VLSU.ScheduleTelegramBot.DAL.Repositories;
using VLSU.ScheduleTelegramBot.Domain.Entities;
using VLSU.ScheduleTelegramBot.Domain.Interfaces.Repositories;

namespace VLSU.ScheduleTelegramBot.DAL;

public static class DependencyInjection
{
    public static void AddMSSqlDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        AddRepositories(services);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IBaseRepository<AppUser>, BaseRepository<AppUser>>();
    }
}
