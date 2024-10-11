using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VLSU.ScheduleTelegramBot.Domain.Entities;

namespace VLSU.ScheduleTelegramBot.DAL.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(u => u.ChatId);
        builder.Property(u => u.ChatId).ValueGeneratedNever();
    }
}
