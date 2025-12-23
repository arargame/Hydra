using System;
using System.Text.RegularExpressions;

namespace Hydra.Utils
{
    /// <summary>
    /// Centralized regex utility with built-in timeout protection against ReDoS attacks.
    /// All regex operations use a default 1-second timeout to prevent infinite processing.
    /// </summary>
    public static class RegexHelper
    {
        /// <summary>
        /// Default timeout for regex operations (1 second)
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Indicates whether the input string matches the specified regular expression pattern.
        /// </summary>
        /// <param name="input">The string to search for a match</param>
        /// <param name="pattern">The regular expression pattern to match</param>
        /// <param name="options">Regex options (default: None)</param>
        /// <param name="timeout">Timeout duration (default: 1 second)</param>
        /// <returns>True if the regex pattern finds a match; otherwise, false</returns>
        public static bool IsMatch(string input, string pattern, RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
        {
            return Regex.IsMatch(input, pattern, options, timeout ?? DefaultTimeout);
        }

        /// <summary>
        /// Searches the input string for the first occurrence of the specified regular expression.
        /// </summary>
        /// <param name="input">The string to search for a match</param>
        /// <param name="pattern">The regular expression pattern to match</param>
        /// <param name="options">Regex options (default: None)</param>
        /// <param name="timeout">Timeout duration (default: 1 second)</param>
        /// <returns>Match object containing the first match</returns>
        public static Match Match(string input, string pattern, RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
        {
            return Regex.Match(input, pattern, options, timeout ?? DefaultTimeout);
        }

        /// <summary>
        /// Searches the input string for all occurrences of the specified regular expression.
        /// </summary>
        /// <param name="input">The string to search for matches</param>
        /// <param name="pattern">The regular expression pattern to match</param>
        /// <param name="options">Regex options (default: None)</param>
        /// <param name="timeout">Timeout duration (default: 1 second)</param>
        /// <returns>Collection of Match objects</returns>
        public static MatchCollection Matches(string input, string pattern, RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
        {
            return Regex.Matches(input, pattern, options, timeout ?? DefaultTimeout);
        }

        /// <summary>
        /// Replaces all strings that match a specified regular expression with a specified replacement string.
        /// </summary>
        /// <param name="input">The string to search for a match</param>
        /// <param name="pattern">The regular expression pattern to match</param>
        /// <param name="replacement">The replacement string</param>
        /// <param name="options">Regex options (default: None)</param>
        /// <param name="timeout">Timeout duration (default: 1 second)</param>
        /// <returns>A new string with replacements</returns>
        public static string Replace(string input, string pattern, string replacement, RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
        {
            return Regex.Replace(input, pattern, replacement, options, timeout ?? DefaultTimeout);
        }

        /// <summary>
        /// Splits an input string into an array of substrings at the positions defined by a regular expression pattern.
        /// </summary>
        /// <param name="input">The string to split</param>
        /// <param name="pattern">The regular expression pattern to match</param>
        /// <param name="options">Regex options (default: None)</param>
        /// <param name="timeout">Timeout duration (default: 1 second)</param>
        /// <returns>Array of strings</returns>
        public static string[] Split(string input, string pattern, RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
        {
            return Regex.Split(input, pattern, options, timeout ?? DefaultTimeout);
        }
    }
}
