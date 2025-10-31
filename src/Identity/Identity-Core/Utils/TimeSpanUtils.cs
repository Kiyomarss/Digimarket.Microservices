namespace Hesabdari_Core.Utils
{
    public static class TimeSpanUtils
    {
        /// <summary>
        /// تبدیل TimeSpan به رشته به فرمت mm:ss یا hh:mm:ss
        /// </summary>
        public static string ToDurationString(this TimeSpan duration)
        {
            if (duration.Hours > 0)
                return duration.ToString(@"hh\:mm\:ss"); // مثلا 01:25:30
            else
                return duration.ToString(@"mm\:ss");     // مثلا 04:54
        }

        /// <summary>
        /// اگر TimeSpan نال بود خروجی خالی برگردان
        /// </summary>
        public static string ToDurationString(this TimeSpan? duration)
        {
            return duration.HasValue ? duration.Value.ToDurationString() : string.Empty;
        }
    }
}