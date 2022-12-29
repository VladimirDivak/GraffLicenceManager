using System;
using System.Globalization;

namespace GraffLicenceManager
{
    public static class DateFormater
    {
        public static string GetCorrectDateFormat(string currentFormat)
        {
            if (string.IsNullOrEmpty(currentFormat)) return "";

            DateTime correctFormat;

            try
            {
                correctFormat = DateTime.Parse(currentFormat);
            }
            catch
            {
                correctFormat = DateTime.ParseExact(currentFormat,
                    "dd.MM.yyyy H:mm:ss",
                    CultureInfo.InvariantCulture);
            }

            return correctFormat
                .ToShortDateString()
                .Replace("/", ".");
        }

        public static DateTime GetParseDate(string dateString)
        {
            try
            {
                return DateTime.Parse(dateString);
            }
            catch
            {
                return DateTime.ParseExact(dateString,
                    "dd.MM.yyyy H:mm:ss",
                    CultureInfo.InvariantCulture);
            }
        }
    }
}
