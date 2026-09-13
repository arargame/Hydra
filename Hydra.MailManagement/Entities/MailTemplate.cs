using Hydra.Core;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Hydra.MailManagement.Entities
{
    public class MailTemplate : BaseObject<MailTemplate>
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? Parameters { get; set; }

        public virtual ICollection<Mail> Mails { get; set; } = new List<Mail>();

        public List<string> GetParameters
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Parameters))
                    return new List<string>();

                return Parameters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }
        }

        public MailTemplate()
        {
        }

        public MailTemplate(string code, string subject, string body, string? parameters = null)
        {
            Code = code;
            Subject = subject;
            Body = body;
            Parameters = parameters;
            Name = code;
        }

        /// <summary>
        /// Renders body by replacing @ParameterName or {{ParameterName}} placeholders with model property values.
        /// Supports IDictionary&lt;string, object?&gt;, IDictionary&lt;string, string&gt;, and standard CLR objects.
        /// </summary>
        public string RenderBody(object? model)
        {
            return RenderText(Body, model);
        }

        /// <summary>
        /// Renders subject by replacing placeholders with model property values.
        /// </summary>
        public string RenderSubject(object? model)
        {
            return RenderText(Subject, model);
        }

        private static string RenderText(string templateText, object? model)
        {
            if (string.IsNullOrEmpty(templateText) || model == null)
                return templateText;

            var rendered = templateText;

            // 1. If model is a dictionary
            if (model is IDictionary<string, object?> dictObj)
            {
                foreach (var (key, val) in dictObj)
                {
                    var strVal = FormatValue(val);
                    rendered = ReplacePlaceholder(rendered, key, strVal);
                }
                return rendered;
            }

            if (model is IDictionary<string, string?> dictStr)
            {
                foreach (var (key, val) in dictStr)
                {
                    rendered = ReplacePlaceholder(rendered, key, val ?? string.Empty);
                }
                return rendered;
            }

            if (model is IDictionary nonGenericDict)
            {
                foreach (DictionaryEntry entry in nonGenericDict)
                {
                    var key = entry.Key?.ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        var val = FormatValue(entry.Value);
                        rendered = ReplacePlaceholder(rendered, key, val);
                    }
                }
                return rendered;
            }

            // 2. Reflection on CLR properties
            var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                try
                {
                    var rawVal = prop.GetValue(model);
                    var propValue = FormatValue(rawVal);
                    rendered = ReplacePlaceholder(rendered, prop.Name, propValue);
                }
                catch
                {
                    // Ignore property evaluation errors during replacement
                }
            }

            return rendered;
        }

        private static string FormatValue(object? val)
        {
            if (val == null)
                return string.Empty;

            if (val is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return val.ToString() ?? string.Empty;
        }

        private static string ReplacePlaceholder(string content, string key, string value)
        {
            return content
                .Replace($"@{key}", value, StringComparison.OrdinalIgnoreCase)
                .Replace($"{{{{{key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
