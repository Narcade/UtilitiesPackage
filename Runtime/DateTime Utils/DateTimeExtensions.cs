using System;

public static class DateTimeExtensions
{
    public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long ToUnixTimeSeconds(this DateTime dt) =>
        DateTimeSerializer.ToUnixTimeSeconds(dt);

    public static int GetDayIndexWhenMondayIs0(this DateTime dt)
    {
        int dayOfWeekSunday0 = (int) dt.DayOfWeek;
        int dayOfWeekMonday0 = (dayOfWeekSunday0 + 6) % 7;

        return dayOfWeekMonday0;
    }

    public static bool IsApproximately(this DateTime @this, DateTime other, TimeSpan epsilon) =>
        Math.Abs((other - @this).TotalMilliseconds) <= epsilon.TotalMilliseconds;

    public static bool IsNotApproximately(this DateTime @this, DateTime other, TimeSpan epsilon) =>
        !IsApproximately(@this, other, epsilon);
}
