namespace Application.Common.Time;

public static class AppTimeZone
{
    private const string DateTimeDisplayFormat = "dd/MM HH:mm";
    private static readonly TimeZoneInfo BusinessTimeZone = ResolveBusinessTimeZone();

    public static DateTime ToBusinessTime(DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, BusinessTimeZone);
    }

    public static DateOnly ToBusinessDate(DateTime utcDateTime) =>
        DateOnly.FromDateTime(ToBusinessTime(utcDateTime));

    public static string FormatDateTime(DateTime utcDateTime) =>
        ToBusinessTime(utcDateTime).ToString(DateTimeDisplayFormat);

    public static string? FormatDateTime(DateTime? utcDateTime) =>
        utcDateTime.HasValue ? FormatDateTime(utcDateTime.Value) : null;

    private static TimeZoneInfo ResolveBusinessTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
