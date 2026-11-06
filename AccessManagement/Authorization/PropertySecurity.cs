using Hydra.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hydra.AccessManagement.Authorization
{
    /// <summary>
    /// Layer 5 of the permission architecture (Academy 5.5): column- and field-level security.
    ///
    /// Hydra's screens are meta-data driven — the grid, the details card and the form all render
    /// whatever <see cref="TableDTO.MetaColumns"/> describes. That makes this layer unusually
    /// cheap: remove a column from the meta-data on the server and it disappears from every
    /// screen at once, with no per-screen `if` anywhere in the UI.
    ///
    /// Crucially the VALUE is removed too, not just the column description. Hiding a column in
    /// the UI while still shipping its value in the JSON is not security, it is a costume — the
    /// data is one browser devtools tab away.
    /// </summary>
    public static class PropertySecurity
    {
        /// <summary>
        /// Fields that must never leave the server, for anyone, regardless of permissions.
        ///
        /// A password hash has no legitimate reason to reach a browser: it is not displayed, not
        /// edited (the edit form posts a new plaintext password over the same field), and its only
        /// use to an attacker is offline cracking. The reset token is the same story — whoever
        /// holds it can take over the account.
        ///
        /// Keyed as "Entity.Property".
        /// </summary>
        private static readonly HashSet<string> AlwaysHidden = new(StringComparer.OrdinalIgnoreCase)
        {
            "SystemUser.PasswordHash",
            "SystemUser.PasswordResetValidationToken",
        };

        /// <summary>
        /// Strips denied columns from a table: the meta-data that describes them AND the values
        /// carried in every row.
        /// </summary>
        public static void Apply(TableDTO? table, string? entityName, IReadOnlyCollection<string>? deniedProperties)
        {
            if (table is null)
                return;

            var denied = BuildDeniedSet(entityName, deniedProperties);

            if (denied.Count == 0)
                return;

            table.MetaColumns?.RemoveAll(column => column?.Name is not null && denied.Contains(column.Name));

            if (table.Rows is null)
                return;

            foreach (var row in table.Rows)
                row?.Columns?.RemoveAll(column => column?.Name is not null && denied.Contains(column.Name));
        }

        /// <summary>
        /// Nulls out denied properties on an entity instance.
        ///
        /// Needed because some endpoints (Details, Get) return the entity object itself alongside
        /// the TableDTO. Cleaning only the table would leave the same value sitting in the other
        /// half of the same response.
        /// </summary>
        public static void Redact(object? entity, string? entityName, IReadOnlyCollection<string>? deniedProperties)
        {
            if (entity is null)
                return;

            var denied = BuildDeniedSet(entityName ?? entity.GetType().Name, deniedProperties);

            if (denied.Count == 0)
                return;

            foreach (var property in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!denied.Contains(property.Name) || !property.CanWrite)
                    continue;

                try
                {
                    // Value types cannot take null; their default is the closest thing to "absent".
                    var blank = property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null
                        ? Activator.CreateInstance(property.PropertyType)
                        : null;

                    property.SetValue(entity, blank);
                }
                catch
                {
                    // An indexer or a property with a quirky setter must not break the response.
                }
            }
        }

        private static HashSet<string> BuildDeniedSet(string? entityName, IReadOnlyCollection<string>? deniedProperties)
        {
            var denied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (deniedProperties is not null)
            {
                foreach (var property in deniedProperties)
                {
                    if (!string.IsNullOrWhiteSpace(property))
                        denied.Add(property);
                }
            }

            if (string.IsNullOrWhiteSpace(entityName))
                return denied;

            foreach (var entry in AlwaysHidden)
            {
                var separator = entry.IndexOf('.');

                if (separator <= 0 || separator == entry.Length - 1)
                    continue;

                var entity = entry[..separator];
                var property = entry[(separator + 1)..];

                if (string.Equals(entity, entityName, StringComparison.OrdinalIgnoreCase))
                    denied.Add(property);
            }

            return denied;
        }
    }
}
