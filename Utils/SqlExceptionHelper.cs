using System;

namespace Hydra.Utils
{
    public static class SqlExceptionHelper
    {
        public static string ToUserFriendlyMessage(Exception ex)
        {
            var baseException = ex.GetBaseException();
            var message = baseException.Message;

            if (message.Contains("FOREIGN KEY constraint"))
            {
                return ParseForeignKeyError(message);
            }

            if (message.Contains("UNIQUE KEY constraint") || message.Contains("PRIMARY KEY constraint"))
            {
                return ParseUniqueConstraintError(message);
            }

            if (message.Contains("deadlock victim"))
            {
                return "The system is currently busy. Please try again in a few moments. (Deadlock)";
            }

            return ex.GetFullMessage();
        }

        private static string ParseUniqueConstraintError(string message)
        {
            // "Violation of UNIQUE KEY constraint 'UQ_Employee_Email'. Cannot insert duplicate key in object 'dbo.Employee'. The duplicate key value is (test@test.com)."
            
            var constraint = ExtractBetween(message, "constraint '", "'");
            var table = ExtractBetween(message, "object '", "'");
            var value = ExtractBetween(message, "value is ", ".");

            return $"A record with the value {value} already exists in '{table}'. (Constraint: {constraint})";
        }

        private static string ParseForeignKeyError(string message)
        {
            // Expected format often contains:
            // "The INSERT statement conflicted with the FOREIGN KEY constraint "FK_Request_Employee_CreatedByEmployeeId". The conflict occurred in database "...", table "dbo.Employee", column 'Id'."
            
            // FK_Request_Employee_CreatedByEmployeeId
            var fkName = ExtractBetween(message, "constraint \"", "\"");

            // dbo.Employee
            var table = ExtractBetween(message, "table \"", "\"");

            // Id
            var column = ExtractBetween(message, "column '", "'");

            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(column))
            {
                // Fallback if parsing fails
                return "Invalid reference. A related record does not exist.";
            }

            return $"Invalid reference. The related '{table}.{column}' record does not exist. (Constraint: {fkName})";
        }

        private static string? ExtractBetween(string source, string start, string end)
        {
            var startIndex = source.IndexOf(start);
            if (startIndex < 0) return null;

            startIndex += start.Length;
            var endIndex = source.IndexOf(end, startIndex);
            if (endIndex < 0) return null;

            return source[startIndex..endIndex];
        }
    }
}
