using System;

namespace Hydra.Utils
{
    /// <summary>
    /// Translates raw SQL and EF Core exceptions into:
    /// 1. Safe, sanitized, human-readable messages for end users (no internal schema/table/constraint leakage - CWE-209).
    /// 2. Detailed technical diagnostic messages for developer logging.
    /// </summary>
    public static class SqlExceptionHelper
    {
        /// <summary>
        /// Produces a safe, sanitized message for the end user and API response.
        /// NEVER leaks table names, column names, or constraint names to prevent information disclosure.
        /// </summary>
        public static string ToSafeUserMessage(Exception ex)
        {
            var baseException = ex.GetBaseException();
            var message = baseException.Message;

            if (message.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase))
            {
                if (message.Contains("DELETE statement", StringComparison.OrdinalIgnoreCase))
                    return "This record cannot be deleted because it is currently referenced by other records.";

                return "A selected related record does not exist or has been removed.";
            }

            if (message.Contains("UNIQUE KEY constraint", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("PRIMARY KEY constraint", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                return "A record with this information already exists.";
            }

            if (message.Contains("deadlock victim", StringComparison.OrdinalIgnoreCase))
            {
                return "The system is currently busy. Please try again in a few moments.";
            }

            return "An unexpected error occurred while saving changes. Please review the details and try again.";
        }

        /// <summary>
        /// Backward-compatible alias for ToSafeUserMessage.
        /// </summary>
        public static string ToUserFriendlyMessage(Exception ex) => ToSafeUserMessage(ex);

        /// <summary>
        /// Produces detailed technical diagnostics (constraint name, table, column, full exception)
        /// meant STRICTLY for developer logging into the database Log table.
        /// </summary>
        public static string ToTechnicalDetails(Exception ex)
        {
            var baseException = ex.GetBaseException();
            var message = baseException.Message;

            if (message.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase))
            {
                var fkName = ExtractBetween(message, "constraint \"", "\"") ?? ExtractBetween(message, "constraint '", "'");
                var table = ExtractBetween(message, "table \"", "\"") ?? ExtractBetween(message, "table '", "'");
                var column = ExtractBetween(message, "column '", "'");

                return $"[SQL ForeignKey Violation] Constraint: '{fkName}', Table: '{table}', Column: '{column}'. Raw: {message}";
            }

            if (message.Contains("UNIQUE KEY constraint", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("PRIMARY KEY constraint", StringComparison.OrdinalIgnoreCase))
            {
                var constraint = ExtractBetween(message, "constraint '", "'");
                var table = ExtractBetween(message, "object '", "'");
                var value = ExtractBetween(message, "value is ", ".");

                return $"[SQL Unique Violation] Constraint: '{constraint}', Table: '{table}', Value: '{value}'. Raw: {message}";
            }

            return $"[SQL Error] {baseException.GetType().Name}: {message}";
        }

        private static string? ExtractBetween(string source, string start, string end)
        {
            var startIndex = source.IndexOf(start, StringComparison.OrdinalIgnoreCase);
            if (startIndex < 0) return null;

            startIndex += start.Length;
            var endIndex = source.IndexOf(end, startIndex, StringComparison.OrdinalIgnoreCase);
            if (endIndex < 0) return null;

            return source[startIndex..endIndex];
        }
    }
}
