using System.Text.RegularExpressions;

namespace CelHost.Server.Utils
{
    public static class RegexHelper
    {
        public static bool RouteMatch(this string input)
        {
            var pattern = @"^\/[^\/]+$";
            bool isMatch = Regex.IsMatch(input, pattern);
            return isMatch;
        }
        public static bool UrlMatch(this string input)
        {
            var pattern = @"(https?:\/\/)?([\w-]+\.)+\w+(\:\d{2,6})?";
            bool isMatch = Regex.IsMatch(input, pattern);
            return isMatch;
        }
    }
    public static class MaskHelper
    {
        public static string Mask(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            int visibleStart = 4;
            int visibleEnd = 2;

            if (value.Length <= visibleStart + visibleEnd)
                return new string('*', value.Length);

            var masked = new string('*', value.Length - visibleStart - visibleEnd);
            return $"{value.Substring(0, visibleStart)}{masked}{value.Substring(value.Length - visibleEnd)}";
        }
    }
}
