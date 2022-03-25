using Logic.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Helpers
{
    /// <summary>
    /// Helper class used to perform common data conversions.
    /// </summary>
    public static class ConversionHelper
    {
        /// <summary>
        /// Converts all new line special characters into line breaks for HTML usage.
        /// </summary>
        /// <param name="text">Text containing new line characters to be converted.</param>
        /// <returns>Text with new line characters converted into HTML line breaks.</returns>
        public static string ConvertNewLinesIntoHTML(this string text)
        {
            return text?.Replace(Environment.NewLine, "<br />");
        }

        /// <summary>
        /// Capitalizes the first letter of each of the given words.
        /// </summary>
        /// <param name="words">Words whose first letters are being capitalized.</param>
        /// <returns>Words with capitalized first letters.</returns>
        public static string Capitalize(this string words)
        {
            if (string.IsNullOrEmpty(words) || words.Length == 1)
            {
                return words?.ToUpper();
            }
            return string.Join(" ", words?.Split(' ').Select(word =>
            {
                if (string.IsNullOrEmpty(word) || word.Length == 1)
                {
                    return words?.ToUpper();
                }
                return word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
            }));
        }

        /// <summary>
        /// Converts IEnumerable to comma-separated string.
        /// </summary>
        /// <param name="enumerable">Value to convert.</param>
        /// <returns>String containing comma-separated list of values.</returns>
        public static string ToCommaSeparatedString(this IEnumerable enumerable)
        {
            return ToSeparatedString(enumerable, ",", false);
        }

        /// <summary>
        /// Converts IEnumerable to comma-separated string formatting items with itemFormat.
        /// </summary>
        /// <param name="enumerable">Value to convert.</param>
        /// <param name="itemFormat">Format string to use for items.</param>
        /// <returns>String containing comma-separated list of values.</returns>
        public static string ToCommaSeparatedString(this IEnumerable enumerable, string itemFormat)
        {
            return ToSeparatedString(enumerable, ",", itemFormat, false);
        }

        /// <summary>
        /// Converts IEnumerable to delimited string using, using supplied delimiter.
        /// </summary>
        /// <param name="enumerable">Value to convert.</param>
        /// <param name="delimiter">Delimiter to separate values.</param>
        /// <param name="ignoreEmptyValues">Indicates that if string representation of a value is empty,
        /// do not include it into result.</param>
        /// <returns>String containing delimited list of values.</returns>
        public static string ToSeparatedString(this IEnumerable enumerable, string delimiter, bool ignoreEmptyValues)
        {
            return ToSeparatedString(enumerable, delimiter, null, ignoreEmptyValues);
        }

        /// <summary>
        /// Converts IEnumerable to delimited string using, using supplied delimiter and item format
        /// </summary>
        /// <param name="enumerable">Value to convert</param>
        /// <param name="delimiter">Delimiter to separate values</param>
        /// <param name="itemFormat">Format string to use for items</param>
        /// <param name="ignoreEmptyValues">Indicates that if string representation of a value is empty,
        /// do not include it into result</param>
        /// <returns>String containing delimited list of values</returns>
        public static string ToSeparatedString(this IEnumerable enumerable, string delimiter, string itemFormat, bool ignoreEmptyValues)
        {
            if (enumerable == null)
                return string.Empty;

            if (delimiter == null)
                delimiter = string.Empty;

            StringBuilder sb = new StringBuilder();
            IEnumerator enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (ignoreEmptyValues && (enumerator.Current == null || enumerator.Current.ToString().Length == 0))
                    continue;

                string value;

                if (string.IsNullOrEmpty(itemFormat))
                {
                    value = enumerator.Current != null ? enumerator.Current.ToString() : "[null]";
                }
                else
                {
                    value = string.Format(itemFormat, enumerator.Current);
                }

                sb.Append(delimiter + value);
            }

            return sb.Length > 0 ? sb.ToString(delimiter.Length, sb.Length - delimiter.Length) : string.Empty;
        }
    }
}
