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
        /// <summary>
        /// Optional localization delegate hook. When set, format strings are resolved via this provider.
        /// Signature: (resourceKey, formatArgs) => localizedString
        /// </summary>
        public static Func<string, object[], string>? Localizer { get; set; }

        private static string FormatMessage(string key, string defaultTemplate, params object[] args)
        {
            if (Localizer != null)
            {
                try
                {
                    var localized = Localizer(key, args);
                    if (!string.IsNullOrWhiteSpace(localized))
                        return localized;
                }
                catch
                {
                    // Fall back to default template
                }
            }

            return string.Format(defaultTemplate, args);
        }

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

        public static ValidationResult CreateResult(string errorMessage, IEnumerable<string> memberNames)
        {
            return new ValidationResult(errorMessage, memberNames);
        }

        /// <summary>
        /// Universal required field validation (string, object, etc.).
        /// </summary>
        public static ValidationResult Required(string propertyName, string? displayName = null)
        {
            var name = displayName ?? propertyName;
            var message = FormatMessage("Validation_Required", "{0} is required.", name);
            return CreateResult(message, new[] { propertyName });
        }

        /// <summary>
        /// Universal selection validation (dropdowns, foreign key IDs, etc.).
        /// </summary>
        public static ValidationResult MustBeSelected(string propertyName, string? displayName = null)
        {
            var name = displayName ?? propertyName;
            var message = FormatMessage("Validation_MustBeSelected", "{0} must be selected.", name);
            return CreateResult(message, new[] { propertyName });
        }

        public static ValidationResult MustBeSelected(PropertyInfo propertyInfo)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            return MustBeSelected(propertyInfo.Name, displayName);
        }

        public static ValidationResult ValueCannotBeNull(string propertyName)
        {
            return Required(propertyName);
        }

        public static ValidationResult GreaterThan(PropertyInfo propertyInfo, int limitValue)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            var message = FormatMessage("Validation_GreaterThan", "'{0}' must be greater than {1}.", displayName, limitValue);
            return CreateResult(message, new[] { propertyInfo.Name });
        }

        public static ValidationResult LessThan(PropertyInfo propertyInfo, int limitValue)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            var message = FormatMessage("Validation_LessThan", "'{0}' must be less than {1}.", displayName, limitValue);
            return CreateResult(message, new[] { propertyInfo.Name });
        }

        public static ValidationResult Range(PropertyInfo propertyInfo, int minValue, int maxValue)
        {
            var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
            var message = FormatMessage("Validation_Range", "'{0}' must be between {1} and {2}.", displayName, minValue, maxValue);
            return CreateResult(message, new[] { propertyInfo.Name });
        }

        public static ValidationResult IsDateInFuture(PropertyInfo propertyInfo, DateTime date)
        {
            if (date <= DateTime.Now)
            {
                var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
                var message = FormatMessage("Validation_DateInFuture", "'{0}' must be a future date.", displayName);
                return CreateResult(message, new[] { propertyInfo.Name });
            }

            return ValidationResult.Success!;
        }

        public static ValidationResult IsDateInPast(PropertyInfo propertyInfo, DateTime date)
        {
            if (date >= DateTime.Now)
            {
                var displayName = ReflectionHelper.GetDisplayNameOrPropertyName(propertyInfo);
                var message = FormatMessage("Validation_DateInPast", "'{0}' must be a past date.", displayName);
                return CreateResult(message, new[] { propertyInfo.Name });
            }

            return ValidationResult.Success!;
        }

        // General Regex Validation
        public static ValidationResult RegexMatch(string value, string pattern, string propertyName)
        {
            if (RegexHelper.IsMatch(value, pattern))
            {
                return ValidationResult.Success!;
            }

            var message = FormatMessage("Validation_RegexMatch", "'{0}' format is invalid.", propertyName);
            return CreateResult(message, new[] { propertyName });
        }

        // Email Validation
        public static ValidationResult IsValidEmail(string email, string propertyName)
        {
            const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (RegexHelper.IsMatch(email, emailPattern))
            {
                return ValidationResult.Success!;
            }

            var message = FormatMessage("Validation_Email", "'{0}' is not a valid email address.", propertyName);
            return CreateResult(message, new[] { propertyName });
        }

        // Phone Number Validation
        public static ValidationResult IsValidPhoneNumber(string phoneNumber, string propertyName)
        {
            const string phonePattern = @"^(\+?[1-9]{1,4}[-.●]?)?((\(\d{1,3}\))|\d{1,4})[-.●]?\d{1,4}[-.●]?\d{1,9}$";
            if (RegexHelper.IsMatch(phoneNumber, phonePattern))
            {
                return ValidationResult.Success!;
            }

            var message = FormatMessage("Validation_Phone", "'{0}' is not a valid phone number.", propertyName);
            return CreateResult(message, new[] { propertyName });
        }
    }
}
