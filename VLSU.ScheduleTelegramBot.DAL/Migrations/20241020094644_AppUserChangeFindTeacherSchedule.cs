using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VLSU.ScheduleTelegramBot.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AppUserChangeFindTeacherSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LooksAtTeachers",
                table: "AppUser",
                newName: "FindsTeacherSchedule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FindsTeacherSchedule",
                table: "AppUser",
                newName: "LooksAtTeachers");
        }
    }
}
