using System;
using System.Globalization;

public static class DateTimeSerializer
{
    // ReSharper disable once ConvertToConstant.Local...
    private static readonly string DefaultDateFormat = "yyyy-MM-dd";

    private static readonly string[] Formats = {
        "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss",
        "dd/MM/yyyy HH:mm", "dd-MM-yyyy HH:mm", "dd.MM.yyyy HH:mm",
        "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy.MM.dd HH:mm:ss",
        "yyyy/MM/dd HH:mm", "yyyy-MM-dd HH:mm", "yyyy.MM.dd HH:mm",
    };

    public static bool TryParseDate(string input, out DateTime result) =>
        DateTime.TryParseExact(input, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result);

    public static DateTime ParseDate(string input) =>
        DateTime.ParseExact(input, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

    public static string ToString(DateTime dateTime) => dateTime.ToString(Formats[0], CultureInfo.InvariantCulture);

    public static string ToDateString(DateTime dateTime) => dateTime.ToString(DefaultDateFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Use this method for dates without time.
    /// </summary>
    /// <param name="dateTimeString"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static DateTime FromDateString(string dateTimeString, DateTime defaultValue)
    {
        return TryParseFromDateString(dateTimeString, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Use this method for dates without time.
    /// </summary>
    /// <param name="dateTimeString"></param>
    /// <param name="defaultValue"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static DateTime FromDateString(string dateTimeString, DateTime defaultValue, string format)
    {
        var isParseSuccessful = TryParseFromDateString(dateTimeString, out var result);
        if (isParseSuccessful)
            return result;

        isParseSuccessful = DateTime.TryParseExact(
            dateTimeString,
            format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out result);

        if (isParseSuccessful)
            return result.Date;

        return defaultValue;
    }

    private static bool TryParseFromDateString(string dateTimeString, out DateTime result)
    {
        bool isParseSuccessful;

        // date string with default date format
        isParseSuccessful = DateTime.TryParseExact(
            dateTimeString,
            DefaultDateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out result);

        if (isParseSuccessful)
            return true;

        // date and time with default device format
        if (DateTime.TryParse(dateTimeString, out result))
        {
            result = result.Date;
            return true;
        }

        // date and time string with many possible formats
        isParseSuccessful = DateTime.TryParseExact(
            dateTimeString,
            Formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out result);

        if (isParseSuccessful)
        {
            result = result.Date;
            return true;
        }

        // unix timestamp
        if (long.TryParse(dateTimeString, out var timestamp))
        {
            result = FromUnixTimeSeconds(timestamp).LocalDateTime.Date;
            return true;
        }

        return false;
    }

    public static long ToUnixTimeSeconds(DateTime dateTime)
    {
        return ((DateTimeOffset) dateTime).ToUnixTimeSeconds();
    }

    public static DateTimeOffset FromUnixTimeSeconds(long unixSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

    public static DateTime ParseTimestampOrDateTime(string dateTimeString, DateTime defaultValue, bool assumeLocal = false)
    {
        return TryParseTimestampOrDateTime(dateTimeString, out var parsedDateTime, assumeLocal) ? parsedDateTime : defaultValue;
    }

    public static bool TryParseTimestampOrDateTime(string dateTimeString, out DateTime result, bool assumeLocal = false)
    {
        if (long.TryParse(dateTimeString, out var timestamp))
        {
            var dto = FromUnixTimeSeconds(timestamp);
            result = assumeLocal ? dto.LocalDateTime : dto.UtcDateTime;

            return true;
        }

        if (DateTime.TryParse(dateTimeString, out var dateTime))
        {
            result = dateTime;
            return true;
        }

        result = DateTime.MinValue;
        return false;
    }

    public static DateTime ParseTimestampOrDateTime(string dateTimeString, DateTime defaultValue, string format, bool assumeLocal = false) =>
        ParseTimestampOrDateTime(dateTimeString, defaultValue, format, CultureInfo.InvariantCulture, assumeLocal);

    public static DateTime ParseTimestampOrDateTime(string dateTimeString, DateTime defaultValue, string format, IFormatProvider formatProvider, bool assumeLocal = false)
    {
        if (long.TryParse(dateTimeString, out var timestamp))
        {
            var dto = FromUnixTimeSeconds(timestamp);
            return assumeLocal ? dto.LocalDateTime : dto.UtcDateTime;
        }

        if (string.IsNullOrEmpty(format))
        {
            if (DateTime.TryParse(dateTimeString, out var dateTime))
                return dateTime;
        }
        else
        {
            var dateTimeStyle = assumeLocal ? DateTimeStyles.AssumeLocal : DateTimeStyles.None;
            if (DateTime.TryParseExact(dateTimeString, format, formatProvider, dateTimeStyle, out var dateTime))
                return dateTime;
        }

        return defaultValue;
    }
}
