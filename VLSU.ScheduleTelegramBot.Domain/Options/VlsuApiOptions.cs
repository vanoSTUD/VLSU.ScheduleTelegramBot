namespace VLSU.ScheduleTelegramBot.Domain.Options;

public class VlsuApiOptions
{
    public const string Section = nameof(VlsuApiOptions);

    public Uri GetGroupInfoUrl { get; set; } = default!;
    public Uri GetTeacherInfoUrl { get; set; } = default!;
    public Uri GetTeacherScheduleUrl { get; set; } = default!;
    public Uri GetGroupScheduleUrl { get; set; } = default!;
    public Uri GetTeachersUrl { get; set; } = default!;
    public Uri GetGroupsUrl { get; set; } = default!;
    public Uri GetInstitutesUrl { get; set; } = default!;
}
