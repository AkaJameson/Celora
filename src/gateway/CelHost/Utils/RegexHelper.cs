using System.Text.RegularExpressions;

namespace CelHost.Utils
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
}
