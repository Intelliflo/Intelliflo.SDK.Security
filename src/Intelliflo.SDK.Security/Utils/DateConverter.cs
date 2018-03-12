using System;
using System.Globalization;

namespace Intelliflo.SDK.Security.Utils
{
    internal static class DateConverter
    {
        private const string Iso8601Format = "yyyyMMddTHHmmssZ";

        public static string ToIso8601Format(this DateTime value)
        {
            return value.ToString(Iso8601Format, CultureInfo.InvariantCulture);
        }

        public static DateTime FromIso8601Format(this string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            return DateTime.ParseExact(
                value,
                new[] { Iso8601Format },
                CultureInfo.InvariantCulture,
                DateTimeStyles.None)
                .ToUniversalTime();
        }
    }
}