namespace VLSU.ScheduleTelegramBot.Domain.Options;

public class VlsuApiOptions
{
    public const string Section = nameof(VlsuApiOptions);

    public Uri GetGroupInfo { get; set; } = default!;
    public Uri GetTeacherInfo { get; set; } = default!;
    public Uri GetTeacherSchedule { get; set; } = default!;
    public Uri GetGroupSchedule { get; set; } = default!;
    public Uri GetTeachers { get; set; } = default!;
    public Uri GetGroups { get; set; } = default!;
    public Uri GetInstitutes { get; set; } = default!;
}
