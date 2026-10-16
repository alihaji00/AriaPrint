#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Extensions;

public static class RandomExtensions
{
    /// <summary>
    /// Generates a random numeric code with the specified length.
    /// </summary>
    /// <returns>A numeric code as text.</returns>
    public static string GenerateRandomNumericCode(this int length)
    {
        if (length <= 0)
        {
            return "";
        }

        const string digits = "0123456789";
        char[] result = new char[length];

        for (int index = 0; index < length; index++)
        {
            result[index] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        }

        return new string(result);
    }

    /// <summary>
    /// Generates a random code using the selected character groups.
    /// </summary>
    /// <returns>A random code as text.</returns>
    public static string GenerateRandomCode(
        this int length,
        bool includeNumbers = true,
        bool includeLowercaseLetters = true,
        bool includeUppercaseLetters = true)
    {
        if (length <= 0)
        {
            return "";
        }

        string characters = "";

        if (includeNumbers)
        {
            characters += "0123456789";
        }

        if (includeLowercaseLetters)
        {
            characters += "abcdefghijklmnopqrstuvwxyz";
        }

        if (includeUppercaseLetters)
        {
            characters += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        }

        if (characters.Length == 0)
        {
            return "";
        }

        char[] result = new char[length];

        for (int index = 0; index < length; index++)
        {
            result[index] = characters[RandomNumberGenerator.GetInt32(characters.Length)];
        }

        return new string(result);
    }
}

public static class StringExtensions
{
    /// <summary>
    /// Returns the first characters of a text.
    /// </summary>
    /// <returns>The shortened text, or an empty string.</returns>
    public static string FirstCharacters(this string? value, int length)
    {
        if (string.IsNullOrEmpty(value) || length <= 0)
        {
            return "";
        }

        return value.Substring(0, Math.Min(length, value.Length));
    }

    /// <summary>
    /// Returns the left characters of a text.
    /// </summary>
    /// <returns>The left part of the text.</returns>
    public static string Left(this string? value, int length)
    {
        return value.FirstCharacters(length);
    }

    /// <summary>
    /// Returns the right characters of a text.
    /// </summary>
    /// <returns>The right part of the text.</returns>
    public static string Right(this string? value, int length)
    {
        if (string.IsNullOrEmpty(value) || length <= 0)
        {
            return "";
        }

        int startIndex = Math.Max(0, value.Length - length);

        return value.Substring(startIndex);
    }

    /// <summary>
    /// Shortens a text and adds a suffix when needed.
    /// </summary>
    /// <returns>The shortened text.</returns>
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || maxLength <= 0)
        {
            return "";
        }

        if (value.Length <= maxLength)
        {
            return value;
        }

        if (suffix.Length >= maxLength)
        {
            return value.Substring(0, maxLength);
        }

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Checks whether a text contains a non-whitespace value.
    /// </summary>
    /// <returns>True when the text has content.</returns>
    public static bool HasContent(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) == false;
    }

    /// <summary>
    /// Returns the text length safely.
    /// </summary>
    /// <returns>The text length, or zero for null.</returns>
    public static int SafeLength(this string? value)
    {
        return value?.Length ?? 0;
    }

    /// <summary>
    /// Converts the first character to uppercase.
    /// </summary>
    /// <returns>The text with an uppercase first character.</returns>
    public static string FirstCharacterToUpper(this string? value, string defaultValue = "")
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        if (value.Length == 1)
        {
            return value.ToUpperInvariant();
        }

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    /// <summary>
    /// Trims a text and converts it to uppercase.
    /// </summary>
    /// <returns>The normalized uppercase text.</returns>
    public static string TrimAndToUpper(this string? value, string defaultValue = "")
    {
        return value?.Trim().ToUpperInvariant() ?? defaultValue;
    }

    /// <summary>
    /// Trims a text and converts it to lowercase.
    /// </summary>
    /// <returns>The normalized lowercase text.</returns>
    public static string TrimAndToLower(this string? value, string defaultValue = "")
    {
        return value?.Trim().ToLowerInvariant() ?? defaultValue;
    }

    /// <summary>
    /// Removes extra spaces from a text.
    /// </summary>
    /// <returns>The text with single spaces.</returns>
    public static string RemoveExtraSpaces(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Converts Persian and Arabic digits to English digits.
    /// </summary>
    /// <returns>The text with English digits.</returns>
    public static string ToEnglishDigits(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        const string englishDigits = "0123456789";
        const string persianDigits = "۰۱۲۳۴۵۶۷۸۹";
        const string arabicDigits = "٠١٢٣٤٥٦٧٨٩";

        for (int index = 0; index < englishDigits.Length; index++)
        {
            value = value
                .Replace(persianDigits[index], englishDigits[index])
                .Replace(arabicDigits[index], englishDigits[index]);
        }

        return value;
    }

    /// <summary>
    /// Converts English and Arabic digits to Persian digits.
    /// </summary>
    /// <returns>The text with Persian digits.</returns>
    public static string ToPersianDigits(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        const string englishDigits = "0123456789";
        const string persianDigits = "۰۱۲۳۴۵۶۷۸۹";

        value = value.ToEnglishDigits();

        for (int index = 0; index < englishDigits.Length; index++)
        {
            value = value.Replace(englishDigits[index], persianDigits[index]);
        }

        return value;
    }

    /// <summary>
    /// Replaces common Arabic characters with Persian characters.
    /// </summary>
    /// <returns>The corrected Persian text.</returns>
    public static string FixPersianCharacters(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value
            .Replace("ﮎ", "ک")
            .Replace("ﮏ", "ک")
            .Replace("ﮐ", "ک")
            .Replace("ﮑ", "ک")
            .Replace("ك", "ک")
            .Replace("ي", "ی")
            .Replace("ى", "ی")
            .Replace("ۀ", "ه")
            .Replace("ة", "ه")
            .Replace('\u00A0', ' ')
            .Replace('\u200C', ' ');
    }

    /// <summary>
    /// Cleans Persian characters, digits, and extra spaces.
    /// </summary>
    /// <returns>The normalized text.</returns>
    public static string CleanText(this string? value)
    {
        return value
            .FixPersianCharacters()
            .ToEnglishDigits()
            .RemoveExtraSpaces();
    }

    /// <summary>
    /// Extracts only numeric digits from a text.
    /// </summary>
    /// <returns>The extracted digits.</returns>
    public static string ExtractNumbers(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return Regex.Replace(value.ToEnglishDigits(), @"[^0-9]", "");
    }

    /// <summary>
    /// Converts a title to a URL-friendly slug.
    /// </summary>
    /// <returns>The generated slug.</returns>
    public static string GenerateSlug(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        string result = value.CleanText().ToLowerInvariant();
        result = Regex.Replace(result, @"[^\p{L}\p{Nd}]+", "-");
        result = Regex.Replace(result, @"-+", "-");

        return result.Trim('-');
    }

    /// <summary>
    /// Checks whether a text is numeric.
    /// </summary>
    /// <returns>True when the text contains a valid number.</returns>
    public static bool IsNumeric(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalizedValue = value.ToEnglishDigits();

        return decimal.TryParse(
            normalizedValue,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out _);
    }

    /// <summary>
    /// Checks whether a text is a valid GUID.
    /// </summary>
    /// <returns>True when the GUID is valid.</returns>
    public static bool IsValidGuid(this string? value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Checks whether a text is a valid email address.
    /// </summary>
    /// <returns>True when the email address is valid.</returns>
    public static bool IsValidEmail(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return MailAddress.TryCreate(value.Trim(), out _);
    }

    /// <summary>
    /// Checks whether a text matches a regular expression.
    /// </summary>
    /// <returns>True when the text matches the pattern.</returns>
    public static bool MatchesRegex(
        this string? value,
        string pattern,
        RegexOptions options = RegexOptions.None)
    {
        if (value == null || string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(
                value,
                pattern,
                options,
                TimeSpan.FromSeconds(2));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts the text between two markers.
    /// </summary>
    /// <returns>The extracted text, or null when markers are not found.</returns>
    public static string? ExtractBetween(
        this string? value,
        string startText,
        string endText,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(value) ||
            string.IsNullOrEmpty(startText) ||
            string.IsNullOrEmpty(endText))
        {
            return null;
        }

        int startIndex = value.IndexOf(startText, comparison);

        if (startIndex < 0)
        {
            return null;
        }

        startIndex += startText.Length;

        int endIndex = value.IndexOf(endText, startIndex, comparison);

        if (endIndex < 0)
        {
            return null;
        }

        return value.Substring(startIndex, endIndex - startIndex);
    }

    /// <summary>
    /// Counts non-overlapping occurrences of a text.
    /// </summary>
    /// <returns>The occurrence count.</returns>
    public static int CountOccurrences(
        this string? value,
        string searchText,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(searchText))
        {
            return 0;
        }

        int count = 0;
        int searchIndex = 0;

        while (true)
        {
            int foundIndex = value.IndexOf(searchText, searchIndex, comparison);

            if (foundIndex < 0)
            {
                break;
            }

            count++;
            searchIndex = foundIndex + searchText.Length;
        }

        return count;
    }

    /// <summary>
    /// Hides the middle part of a mobile number.
    /// </summary>
    /// <returns>The masked mobile number.</returns>
    public static string MaskMobileNumber(this string? value)
    {
        string mobileNumber = value.ExtractNumbers();

        if (mobileNumber.Length < 8)
        {
            return mobileNumber;
        }

        return $"{mobileNumber.Substring(0, 4)}***{mobileNumber.Substring(mobileNumber.Length - 4)}";
    }

    /// <summary>
    /// Hides part of an email username.
    /// </summary>
    /// <returns>The masked email address.</returns>
    public static string MaskEmail(this string? value)
    {
        if (value.IsValidEmail() == false)
        {
            return "";
        }

        string[] parts = value!.Split('@');
        string username = parts[0];
        string domain = parts[1];

        if (username.Length <= 2)
        {
            return $"***@{domain}";
        }

        return $"{username.Substring(0, 2)}***@{domain}";
    }
}

public static class ConversionExtensions
{
    /// <summary>
    /// Converts a value to text safely.
    /// </summary>
    /// <returns>The converted text or the default value.</returns>
    public static string ToSafeString(this object? value, string defaultValue = "")
    {
        if (value == null || value == DBNull.Value)
        {
            return defaultValue;
        }

        return value.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Converts a value to an integer safely.
    /// </summary>
    /// <returns>The converted integer or the default value.</returns>
    public static int ToSafeInt(this object? value, int defaultValue = 0)
    {
        return value.ToNullableInt() ?? defaultValue;
    }

    /// <summary>
    /// Converts a value to a nullable integer.
    /// </summary>
    /// <returns>The converted integer, or null when invalid.</returns>
    public static int? ToNullableInt(this object? value)
    {
        if (value == null || value == DBNull.Value)
        {
            return null;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        string text = value.ToString().ToEnglishDigits().Trim();

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalResult))
        {
            return (int)decimalResult;
        }

        return null;
    }

    /// <summary>
    /// Converts a value to a long integer safely.
    /// </summary>
    /// <returns>The converted long integer or the default value.</returns>
    public static long ToSafeLong(this object? value, long defaultValue = 0)
    {
        return value.ToNullableLong() ?? defaultValue;
    }

    /// <summary>
    /// Converts a value to a nullable long integer.
    /// </summary>
    /// <returns>The converted long integer, or null when invalid.</returns>
    public static long? ToNullableLong(this object? value)
    {
        if (value == null || value == DBNull.Value)
        {
            return null;
        }

        if (value is long longValue)
        {
            return longValue;
        }

        string text = value.ToString().ToEnglishDigits().Trim();

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
        {
            return result;
        }

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalResult))
        {
            return (long)decimalResult;
        }

        return null;
    }

    /// <summary>
    /// Converts a value to a decimal safely.
    /// </summary>
    /// <returns>The converted decimal or the default value.</returns>
    public static decimal ToSafeDecimal(this object? value, decimal defaultValue = 0)
    {
        return value.ToNullableDecimal() ?? defaultValue;
    }

    /// <summary>
    /// Converts a value to a nullable decimal.
    /// </summary>
    /// <returns>The converted decimal, or null when invalid.</returns>
    public static decimal? ToNullableDecimal(this object? value)
    {
        if (value == null || value == DBNull.Value)
        {
            return null;
        }

        if (value is decimal decimalValue)
        {
            return decimalValue;
        }

        string text = value.ToString().ToEnglishDigits().Trim();

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Converts a value to a double safely.
    /// </summary>
    /// <returns>The converted double or the default value.</returns>
    public static double ToSafeDouble(this object? value, double defaultValue = 0)
    {
        if (value == null || value == DBNull.Value)
        {
            return defaultValue;
        }

        if (value is double doubleValue)
        {
            return doubleValue;
        }

        string text = value.ToString().ToEnglishDigits().Trim();

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>
    /// Converts a value to a byte safely.
    /// </summary>
    /// <returns>The converted byte or the default value.</returns>
    public static byte ToSafeByte(this object? value, byte defaultValue = 0)
    {
        if (value == null || value == DBNull.Value)
        {
            return defaultValue;
        }

        if (value is byte byteValue)
        {
            return byteValue;
        }

        if (value is bool boolValue)
        {
            return boolValue.ToTinyInt();
        }

        string text = value.ToString().ToEnglishDigits().Trim();

        if (byte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte result))
        {
            return result;
        }

        if (bool.TryParse(text, out bool boolResult))
        {
            return boolResult.ToTinyInt();
        }

        return defaultValue;
    }

    /// <summary>
    /// Converts a value to a boolean safely.
    /// </summary>
    /// <returns>The converted boolean or the default value.</returns>
    public static bool ToSafeBoolean(this object? value, bool defaultValue = false)
    {
        if (value == null || value == DBNull.Value)
        {
            return defaultValue;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is byte byteValue)
        {
            return byteValue == 1;
        }

        string text = value
            .ToString()
            .ToEnglishDigits()
            .Trim()
            .ToLowerInvariant();

        if (text is "1" or "true" or "yes" or "on")
        {
            return true;
        }

        if (text is "0" or "false" or "no" or "off")
        {
            return false;
        }

        return defaultValue;
    }

    /// <summary>
    /// Converts a boolean to tinyint-compatible byte.
    /// </summary>
    /// <returns>One for true and zero for false.</returns>
    public static byte ToTinyInt(this bool value)
    {
        return value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Converts a byte to boolean.
    /// </summary>
    /// <returns>True when the value is one.</returns>
    public static bool ToBoolean(this byte value)
    {
        return value == 1;
    }

    /// <summary>
    /// Formats a numeric value with separators.
    /// </summary>
    /// <returns>The formatted number, or an empty string.</returns>
    public static string FormatNumber(this object? value, int decimalPlaces = 0)
    {
        decimal? number = value.ToNullableDecimal();

        if (number == null)
        {
            return "";
        }

        decimalPlaces = Math.Max(0, decimalPlaces);

        return number.Value.ToString($"N{decimalPlaces}", CultureInfo.InvariantCulture);
    }
}

public sealed class PersianDateInfo
{
    public int Year { get; init; }

    public int Month { get; init; }

    public int Day { get; init; }

    public string MonthName { get; init; } = "";

    public string DayName { get; init; } = "";

    public string DateText { get; init; } = "";

    public string TimeText { get; init; } = "";

    public int DateNumber { get; init; }

    public long DateTimeNumber { get; init; }
}

public static class DateTimeExtensions
{
    private static readonly string[] PersianMonthNames =
    {
        "فروردین",
        "اردیبهشت",
        "خرداد",
        "تیر",
        "مرداد",
        "شهریور",
        "مهر",
        "آبان",
        "آذر",
        "دی",
        "بهمن",
        "اسفند"
    };

    private static readonly Dictionary<DayOfWeek, string> PersianDayNames = new()
    {
        [DayOfWeek.Saturday] = "شنبه",
        [DayOfWeek.Sunday] = "یکشنبه",
        [DayOfWeek.Monday] = "دوشنبه",
        [DayOfWeek.Tuesday] = "سه‌شنبه",
        [DayOfWeek.Wednesday] = "چهارشنبه",
        [DayOfWeek.Thursday] = "پنج‌شنبه",
        [DayOfWeek.Friday] = "جمعه"
    };

    /// <summary>
    /// Converts a date to Unix seconds.
    /// </summary>
    /// <returns>The Unix timestamp in seconds.</returns>
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts a date to Unix milliseconds.
    /// </summary>
    /// <returns>The Unix timestamp in milliseconds.</returns>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Converts Unix seconds to a UTC date.
    /// </summary>
    /// <returns>The converted UTC date.</returns>
    public static DateTime FromUnixTimeSeconds(this long unixTimeSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
    }

    /// <summary>
    /// Converts Unix milliseconds to a UTC date.
    /// </summary>
    /// <returns>The converted UTC date.</returns>
    public static DateTime FromUnixTimeMilliseconds(this long unixTimeMilliseconds)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).UtcDateTime;
    }

    /// <summary>
    /// Converts a date and time to yyyyMMddHHmmss.
    /// </summary>
    /// <returns>The numeric date and time.</returns>
    public static long ToDateTimeNumber(this DateTime dateTime)
    {
        return long.Parse(
            dateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a date to yyyyMMdd.
    /// </summary>
    /// <returns>The numeric date.</returns>
    public static int ToDateNumber(this DateTime dateTime)
    {
        return int.Parse(
            dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a time to HHmmss.
    /// </summary>
    /// <returns>The numeric time.</returns>
    public static int ToTimeNumber(this DateTime dateTime)
    {
        return int.Parse(
            dateTime.ToString("HHmmss", CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns complete Persian date information.
    /// </summary>
    /// <returns>A Persian date information model.</returns>
    public static PersianDateInfo ToPersianDateInfo(this DateTime dateTime)
    {
        PersianCalendar calendar = new();

        int year = calendar.GetYear(dateTime);
        int month = calendar.GetMonth(dateTime);
        int day = calendar.GetDayOfMonth(dateTime);

        string dateText = string.Format(
            CultureInfo.InvariantCulture,
            "{0:D4}/{1:D2}/{2:D2}",
            year,
            month,
            day);

        string dateNumberText = string.Format(
            CultureInfo.InvariantCulture,
            "{0:D4}{1:D2}{2:D2}",
            year,
            month,
            day);

        string dateTimeNumberText = string.Format(
            CultureInfo.InvariantCulture,
            "{0:D4}{1:D2}{2:D2}{3:HHmmss}",
            year,
            month,
            day,
            dateTime);

        return new PersianDateInfo
        {
            Year = year,
            Month = month,
            Day = day,
            MonthName = PersianMonthNames[month - 1],
            DayName = PersianDayNames[dateTime.DayOfWeek],
            DateText = dateText,
            TimeText = dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            DateNumber = int.Parse(dateNumberText, CultureInfo.InvariantCulture),
            DateTimeNumber = long.Parse(dateTimeNumberText, CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Converts a date to a short Persian date.
    /// </summary>
    /// <returns>The Persian date as text.</returns>
    public static string ToPersianDateString(
        this DateTime dateTime,
        bool includeTime = false,
        string separator = "/")
    {
        PersianDateInfo date = dateTime.ToPersianDateInfo();

        string result = string.Join(
            separator,
            date.Year.ToString("D4", CultureInfo.InvariantCulture),
            date.Month.ToString("D2", CultureInfo.InvariantCulture),
            date.Day.ToString("D2", CultureInfo.InvariantCulture));

        if (includeTime)
        {
            result += $" {date.TimeText}";
        }

        return result;
    }

    /// <summary>
    /// Converts a date to a readable Persian date.
    /// </summary>
    /// <returns>The long Persian date as text.</returns>
    public static string ToLongPersianDateString(this DateTime dateTime, bool includeTime = false)
    {
        PersianDateInfo date = dateTime.ToPersianDateInfo();

        string result = $"{date.DayName} {date.Day} {date.MonthName} {date.Year}";

        if (includeTime)
        {
            result += $" ساعت {date.TimeText}";
        }

        return result;
    }

    /// <summary>
    /// Converts a date to a Persian yyyyMMdd number.
    /// </summary>
    /// <returns>The numeric Persian date.</returns>
    public static int ToPersianDateNumber(this DateTime dateTime)
    {
        return dateTime.ToPersianDateInfo().DateNumber;
    }

    /// <summary>
    /// Converts a date to a Persian yyyyMMddHHmmss number.
    /// </summary>
    /// <returns>The numeric Persian date and time.</returns>
    public static long ToPersianDateTimeNumber(this DateTime dateTime)
    {
        return dateTime.ToPersianDateInfo().DateTimeNumber;
    }

    /// <summary>
    /// Converts a Persian yyyyMMdd number to DateTime.
    /// </summary>
    /// <returns>The converted datetime, or null when invalid.</returns>
    public static DateTime? FromPersianNumberToDateTime(this int persianDateNumber)
    {
        string value = persianDateNumber.ToString(CultureInfo.InvariantCulture);

        if (value.Length != 8)
        {
            return null;
        }

        int year = value.Substring(0, 4).ToSafeInt();
        int month = value.Substring(4, 2).ToSafeInt();
        int day = value.Substring(6, 2).ToSafeInt();

        try
        {
            return new PersianCalendar().ToDateTime(year, month, day, 0, 0, 0, 0);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a Persian yyyyMMdd number to Date.
    /// </summary>
    /// <returns>The converted date, or null when invalid.</returns>
    public static DateTime? FromPersianNumberToDate(this int persianDateNumber)
    {
        string value = persianDateNumber.FromPersianNumberToDateTime().ToString();

        DateOnly date = value.Substring(0, 4);
        int month = value.Substring(4, 2).ToSafeInt();
        int day = value.Substring(6, 2).ToSafeInt();

        try
        {
            return new PersianCalendar().ToDateTime(year, month, day);
        }
        catch
        {
            return null;
        }
    }
}

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions IgnoreNullOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a value to JSON.
    /// </summary>
    /// <returns>The JSON text.</returns>
    public static string ToJson(this object? value)
    {
        return JsonSerializer.Serialize(value, DefaultOptions);
    }

    /// <summary>
    /// Serializes a value to JSON and ignores null properties.
    /// </summary>
    /// <returns>The JSON text.</returns>
    public static string ToJsonIgnoreNull(this object? value)
    {
        return JsonSerializer.Serialize(value, IgnoreNullOptions);
    }

    /// <summary>
    /// Deserializes JSON to a model.
    /// </summary>
    /// <returns>The model, or the default value when invalid.</returns>
    public static T? FromJson<T>(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Converts an object to a dictionary.
    /// </summary>
    /// <returns>A dictionary containing object properties.</returns>
    public static Dictionary<string, object?> ToDictionary(this object? value)
    {
        if (value == null)
        {
            return new Dictionary<string, object?>();
        }

        return value
            .ToJson()
            .FromJson<Dictionary<string, object?>>()
            ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Converts JSON text to a dictionary.
    /// </summary>
    /// <returns>A dictionary containing JSON values.</returns>
    public static Dictionary<string, object?> JsonToDictionary(this string? json)
    {
        return json.FromJson<Dictionary<string, object?>>()
            ?? new Dictionary<string, object?>();
    }
}

public static class EnumExtensions
{
    /// <summary>
    /// Returns the Description attribute of an enum value.
    /// </summary>
    /// <returns>The description or enum name.</returns>
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }
}

public static class HttpRequestExtensions
{
    /// <summary>
    /// Reads a string value from the query string.
    /// </summary>
    /// <returns>The query value or the default value.</returns>
    public static string GetQueryString(this HttpRequest request, string key, string defaultValue = "")
    {
        string value = request.Query[key].FirstOrDefault() ?? "";

        return string.IsNullOrWhiteSpace(value)
            ? defaultValue
            : value.Trim();
    }

    /// <summary>
    /// Reads an integer from the query string.
    /// </summary>
    /// <returns>The query integer or the default value.</returns>
    public static int GetQueryInt(this HttpRequest request, string key, int defaultValue = 0)
    {
        return request.GetQueryString(key).ToSafeInt(defaultValue);
    }

    /// <summary>
    /// Reads a long integer from the query string.
    /// </summary>
    /// <returns>The query long integer or the default value.</returns>
    public static long GetQueryLong(this HttpRequest request, string key, long defaultValue = 0)
    {
        return request.GetQueryString(key).ToSafeLong(defaultValue);
    }

    /// <summary>
    /// Reads a decimal from the query string.
    /// </summary>
    /// <returns>The query decimal or the default value.</returns>
    public static decimal GetQueryDecimal(this HttpRequest request, string key, decimal defaultValue = 0)
    {
        return request.GetQueryString(key).ToSafeDecimal(defaultValue);
    }

    /// <summary>
    /// Reads a boolean from the query string.
    /// </summary>
    /// <returns>The query boolean or the default value.</returns>
    public static bool GetQueryBoolean(this HttpRequest request, string key, bool defaultValue = false)
    {
        return request.GetQueryString(key).ToSafeBoolean(defaultValue);
    }

    /// <summary>
    /// Reads a request header value.
    /// </summary>
    /// <returns>The header value or the default value.</returns>
    public static string GetHeaderValue(
        this HttpRequest request,
        string headerName,
        string defaultValue = "")
    {
        string value = request.Headers[headerName].FirstOrDefault() ?? "";

        return string.IsNullOrWhiteSpace(value)
            ? defaultValue
            : value.Trim();
    }

    /// <summary>
    /// Reads the Bearer token from the Authorization header.
    /// </summary>
    /// <returns>The token without the Bearer prefix.</returns>
    public static string GetBearerToken(this HttpRequest request)
    {
        string authorization = request.GetHeaderValue(HeaderNames.Authorization);
        const string prefix = "Bearer ";

        if (authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return "";
        }

        return authorization.Substring(prefix.Length).Trim();
    }

    /// <summary>
    /// Returns the client IP address.
    /// </summary>
    /// <returns>The client IP address or an empty string.</returns>
    public static string GetClientIpAddress(this HttpRequest request)
    {
        string cloudflareIp = request.Headers["CF-CONNECTING-IP"].FirstOrDefault() ?? "";

        if (string.IsNullOrWhiteSpace(cloudflareIp) == false)
        {
            return cloudflareIp.Trim();
        }

        string forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "";

        if (string.IsNullOrWhiteSpace(forwardedFor) == false)
        {
            string firstIp = forwardedFor
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? "";

            if (string.IsNullOrWhiteSpace(firstIp) == false)
            {
                return firstIp.Trim();
            }
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
    }

    /// <summary>
    /// Returns simple browser and device information.
    /// </summary>
    /// <returns>The browser and device names.</returns>
    public static string GetClientInfo(this HttpRequest request)
    {
        string userAgent = request.Headers[HeaderNames.UserAgent].ToString();

        return $"Browser: {userAgent.GetBrowserName()}, Device: {userAgent.GetDeviceType()}";
    }

    /// <summary>
    /// Converts submitted form values to a model.
    /// </summary>
    /// <returns>The created model, or null for non-form requests.</returns>
    public static async Task<T?> ToModelFromFormAsync<T>(
        this HttpRequest request,
        CancellationToken cancellationToken = default)
        where T : new()
    {
        if (request.HasFormContentType == false)
        {
            return default;
        }

        IFormCollection form = await request.ReadFormAsync(cancellationToken);
        T model = new();

        foreach (PropertyInfo property in typeof(T).GetProperties())
        {
            if (property.CanWrite == false)
            {
                continue;
            }

            string value = form[property.Name].FirstOrDefault() ?? "";

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            object? convertedValue = value.ConvertTo(property.PropertyType);

            if (convertedValue != null)
            {
                property.SetValue(model, convertedValue);
            }
        }

        return model;
    }

    /// <summary>
    /// Converts text to a requested runtime type.
    /// </summary>
    /// <returns>The converted value, or null when invalid.</returns>
    private static object? ConvertTo(this string value, Type targetType)
    {
        Type actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (actualType == typeof(string))
            {
                return value;
            }

            if (actualType == typeof(Guid))
            {
                return Guid.TryParse(value, out Guid result) ? result : null;
            }

            if (actualType == typeof(DateTime))
            {
                return DateTime.TryParse(value, out DateTime result) ? result : null;
            }

            if (actualType == typeof(bool))
            {
                return value.ToSafeBoolean();
            }

            if (actualType.IsEnum)
            {
                return Enum.TryParse(actualType, value, true, out object? result)
                    ? result
                    : null;
            }

            return Convert.ChangeType(
                value.ToEnglishDigits(),
                actualType,
                CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Detects a browser name from a user-agent value.
    /// </summary>
    /// <returns>The detected browser name.</returns>
    private static string GetBrowserName(this string userAgent)
    {
        if (userAgent.Contains("Edg", StringComparison.OrdinalIgnoreCase))
        {
            return "Microsoft Edge";
        }

        if (userAgent.Contains("OPR", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("Opera", StringComparison.OrdinalIgnoreCase))
        {
            return "Opera";
        }

        if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
        {
            return "Google Chrome";
        }

        if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
        {
            return "Mozilla Firefox";
        }

        if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase))
        {
            return "Safari";
        }

        if (userAgent.Contains("MSIE", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("Trident", StringComparison.OrdinalIgnoreCase))
        {
            return "Internet Explorer";
        }

        return "Unknown";
    }

    /// <summary>
    /// Detects a device type from a user-agent value.
    /// </summary>
    /// <returns>The detected device type.</returns>
    private static string GetDeviceType(this string userAgent)
    {
        if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
        {
            return "Tablet";
        }

        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
        {
            return "Mobile";
        }

        return "Desktop";
    }
}

public static class UrlExtensions
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    /// <summary>
    /// Checks whether a URL responds successfully.
    /// </summary>
    /// <returns>True when the URL is available.</returns>
    public static async Task<bool> UrlExistsAsync(
        this string? url,
        CancellationToken cancellationToken = default)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) == false)
        {
            return false;
        }

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Head, uri);
            using HttpResponseMessage response = await HttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public static class QueryableExtensions
{
    /// <summary>
    /// Applies simple page-based pagination to a query.
    /// </summary>
    /// <returns>The paginated query.</returns>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        int maximumPageSize = 200)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);
        maximumPageSize = Math.Max(1, maximumPageSize);
        pageSize = Math.Min(pageSize, maximumPageSize);

        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}

public static class DataTableExtensions
{
    /// <summary>
    /// Converts the first DataTable row to a dictionary.
    /// </summary>
    /// <returns>The first row dictionary, or an empty dictionary.</returns>
    public static Dictionary<string, object?> ToFirstRowDictionary(this DataTable? dataTable)
    {
        Dictionary<string, object?> result = new();

        if (dataTable == null || dataTable.Rows.Count == 0)
        {
            return result;
        }

        foreach (DataColumn column in dataTable.Columns)
        {
            object value = dataTable.Rows[0][column];

            result[column.ColumnName] = value == DBNull.Value
                ? null
                : value;
        }

        return result;
    }

    /// <summary>
    /// Converts all DataTable rows to dictionaries.
    /// </summary>
    /// <returns>A list containing all rows.</returns>
    public static List<Dictionary<string, object?>> ToDictionaryList(this DataTable? dataTable)
    {
        List<Dictionary<string, object?>> result = new();

        if (dataTable == null)
        {
            return result;
        }

        foreach (DataRow dataRow in dataTable.Rows)
        {
            Dictionary<string, object?> row = new();

            foreach (DataColumn column in dataTable.Columns)
            {
                object value = dataRow[column];

                row[column.ColumnName] = value == DBNull.Value
                    ? null
                    : value;
            }

            result.Add(row);
        }

        return result;
    }
}
