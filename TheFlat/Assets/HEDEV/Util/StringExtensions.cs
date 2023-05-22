using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proto.Util
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        private static string[] pluralExceptions = { "s", "sh", "ch", "x", "z" };

        public static string ToPlural(this string text)
        {
            if (pluralExceptions.Any(p => text.EndsWith(p)))
                return text + "es";
            return text + "s";
        }

        public static string ToUppers(this string text)
        {
            var builder = new StringBuilder(text);
            for (var i = 0; i < builder.Length; ++i)
                builder[i] = char.ToUpper(builder[i]);
            return builder.ToString();
        }

        public static string ToLowers(this string text)
        {
            var builder = new StringBuilder(text);
            for (var i = 0; i < builder.Length; ++i)
                builder[i] = char.ToLower(builder[i]);
            return builder.ToString();
        }

        public static IEnumerable<int> AllIndexesOf(this string source, char character, int startIndex = 0)
        {
            for (var index = startIndex;; ++index)
            {
                index = source.IndexOf(character, index);
                if (index == -1)
                    yield break;
                yield return index;
            }
        }
    }
}