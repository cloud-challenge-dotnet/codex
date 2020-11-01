namespace Codex.Core.Extensions
{
    public static partial class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (str.Length == 0)
            {
                return "";
            }
            else if (str.Length == 1)
            {
                return char.ToLowerInvariant(str[0]).ToString();
            }
            else
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
        }
    }
}
