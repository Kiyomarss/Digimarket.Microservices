using System.Globalization;

namespace Hesabdari_Core.Utils
{
    public static class DateTimeUtils
    {
        private static readonly PersianCalendar PersianCalendar = new PersianCalendar();

        private const string DateFormat = "yyyy/MM/dd";
        private const string TimeFormat = "HH:mm:ss";
        private const string DateTimeFormat = DateFormat + " " + TimeFormat;

        public static string ToPersianDate(this DateTime? dateTime)
            => dateTime.HasValue ? FormatPersianDate(dateTime.Value) : string.Empty;

        public static string ToPersianDateTime(this DateTime? dateTime)
            => dateTime.HasValue
                   ? $"{FormatPersianDate(dateTime.Value)} {dateTime.Value:HH:mm:ss}"
                   : string.Empty;
        public static DateTime? TryParsePersianDate(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return null;

            var parts = persianDate.Split('/', StringSplitOptions.TrimEntries);
            if (parts.Length != 3
                || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var y)
                || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var m)
                || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var d))
            {
                return null;
            }

            try
            {
                return PersianCalendar.ToDateTime(y, m, d, 0, 0, 0, 0);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime? TryParsePersianDateTime(string persianDateTime)
        {
            if (string.IsNullOrWhiteSpace(persianDateTime))
                return null;

            var parts = persianDateTime.Split(' ', StringSplitOptions.TrimEntries);

            if (parts.Length != 2)
                return null;

            var date = TryParsePersianDate(parts[0]);

            if (date == null)
                return null;

            var timeParts = parts[1].Split(':', StringSplitOptions.TrimEntries);
            if (timeParts.Length < 2
                || !int.TryParse(timeParts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var hh)
                || !int.TryParse(timeParts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var mm))
            {
                return null;
            }

            var ss = 0;
            if (timeParts.Length > 2)
                int.TryParse(timeParts[2], NumberStyles.None, CultureInfo.InvariantCulture, out ss);

            try
            {
                return PersianCalendar.ToDateTime(
                                                  PersianCalendar.GetYear(date.Value),
                                                  PersianCalendar.GetMonth(date.Value),
                                                  PersianCalendar.GetDayOfMonth(date.Value),
                                                  hh, mm, ss, 0);
            }
            catch
            {
                return null;
            }
        }

        private static string FormatPersianDate(DateTime dateTime)
        {
            var pc = new PersianCalendar();
            var year = pc.GetYear(dateTime);
            var month = pc.GetMonth(dateTime);
            var day = pc.GetDayOfMonth(dateTime);

            return $"{year:0000}/{month:00}/{day:00}";
        }
    }
}