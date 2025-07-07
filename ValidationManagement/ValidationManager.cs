using Hydra.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hydra.ValidationManagement
{
    public static class ValidationManager
    {
        public static bool IsValid(this object entity, out List<ValidationResult> results)
        {
            results = ValidateEntity(entity);
            return results == null || results.Count == 0;
        }

        public static List<ValidationResult> ValidateEntity(object entity)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(entity);
            Validator.TryValidateObject(entity, context, results, true);

            if (entity is IValidatableObject validatable)
                results.AddRange(validatable.Validate(context));

            return results;
        }

        public static ValidationResult ValueCannotBeNull(string propertyName)
        {
            return CreateResult($"The value of '{propertyName}' cannot be null or empty",
                new[] { propertyName });
        }

        public static ValidationResult CreateResult(string errorMessage, IEnumerable<string> memberNames)
        {
            return new ValidationResult(errorMessage, memberNames);
        }

        public static ValidationResult MustBeSelected(PropertyInfo propertyInfo)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            return CreateResult($"'{displayName}' must be selected", new[] { displayName });
        }

        public static ValidationResult GreaterThan(PropertyInfo propertyInfo, int limitValue)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            return CreateResult($"'{displayName}' must be greater than {limitValue}", new[] { displayName });
        }

        public static ValidationResult LessThan(PropertyInfo propertyInfo, int limitValue)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            return CreateResult($"'{displayName}' must be less than {limitValue}", new[] { displayName });
        }

        public static ValidationResult Range(PropertyInfo propertyInfo, int minValue, int maxValue)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            return CreateResult($"'{displayName}' must be between {minValue} and {maxValue}", new[] { displayName });
        }

        public static ValidationResult IsDateInFuture(PropertyInfo propertyInfo, DateTime date)
        {
            if (date <= DateTime.Now)
                return CreateResult($"'{propertyInfo.Name}' must be a future date", new[] { propertyInfo.Name });

            return ValidationResult.Success!;
        }

        public static ValidationResult IsDateInPast(PropertyInfo propertyInfo, DateTime date)
        {
            if (date >= DateTime.Now)
                return CreateResult($"'{propertyInfo.Name}' must be a past date", new[] { propertyInfo.Name });

            return ValidationResult.Success!;
        }

        // General Regex Validation
        public static ValidationResult RegexMatch(string value, string pattern, string propertyName)
        {
            if (Regex.IsMatch(value, pattern))
            {
                return ValidationResult.Success!;
            }

            return CreateResult($"'{propertyName}' format is invalid", new[] { propertyName });
        }

        // Email Validation
        public static ValidationResult IsValidEmail(string email, string propertyName)
        {
            const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (Regex.IsMatch(email, emailPattern))
            {
                return ValidationResult.Success!;
            }

            return CreateResult($"'{propertyName}' is not a valid email address", new[] { propertyName });
        }

        // Phone Number Validation
        public static ValidationResult IsValidPhoneNumber(string phoneNumber, string propertyName)
        {
            // Example: Matches formats like (123) 456-7890, 123-456-7890, 123.456.7890, +1234567890
            const string phonePattern = @"^(\+?[1-9]{1,4}[-.●]?)?((\(\d{1,3}\))|\d{1,4})[-.●]?\d{1,4}[-.●]?\d{1,9}$";
            if (Regex.IsMatch(phoneNumber, phonePattern))
            {
                return ValidationResult.Success!;
            }

            return CreateResult($"'{propertyName}' is not a valid phone number", new[] { propertyName });
        }
    }

}