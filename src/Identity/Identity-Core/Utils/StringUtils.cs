using Hesabdari_Core.Constants;

namespace Hesabdari_Core.Utils
{
    public static class StringUtils
    {
        public static string TruncateWithEllipsis(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength) + TextSeparators.Ellipsis;
        }
    }
}
