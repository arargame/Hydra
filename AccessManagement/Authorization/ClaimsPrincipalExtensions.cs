using System;
using System.Linq;
using System.Security.Claims;

namespace Hydra.AccessManagement.Authorization
{
    /// <summary>
    /// Client-side reading of the permission claims the API put into the JWT at login.
    ///
    /// IMPORTANT — what this is and is not: these helpers decide what to SHOW. They are a
    /// convenience so the UI does not offer a button that will bounce back as 403. They are not
    /// security: the token is in the user's own browser, and the only decision that counts is the
    /// one <c>PermissionEvaluator</c> makes on the server for every single request. Hiding a menu
    /// item never replaces the server check; it just stops the user walking into a wall.
    ///
    /// The matching rules mirror PermissionEvaluator exactly (null/empty/"*"/"All" mean "any"), so
    /// the menu and the API cannot disagree about what is reachable.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        private const string Wildcard = "*";
        private const string WildcardAlias = "All";

        public static bool IsSignedIn(this ClaimsPrincipal? principal)
            => principal?.Identity?.IsAuthenticated == true;

        /// <summary>True when the user holds the global "*" permission (the seeded admin).</summary>
        public static bool HasGlobalAccess(this ClaimsPrincipal? principal)
            => principal?.FindAll(HydraClaimTypes.Permission).Any(c => c.Value == Wildcard) == true;

        /// <summary>
        /// Can this user call <paramref name="controller"/>/<paramref name="action"/>?
        /// Pass a null action to ask "any action on this controller".
        /// </summary>
        public static bool CanAccessEndpoint(this ClaimsPrincipal? principal, string? controller, string? action = null)
        {
            if (!principal.IsSignedIn())
                return false;

            if (principal.HasGlobalAccess())
                return true;

            foreach (var claim in principal!.FindAll(HydraClaimTypes.Endpoint))
            {
                var parts = claim.Value.Split('/');

                var claimController = parts.Length > 0 ? parts[0] : null;
                var claimAction = parts.Length > 1 ? parts[1] : null;

                if (!SegmentMatches(claimController, controller))
                    continue;

                // A null requested action means "any action here is good enough".
                if (action is null || SegmentMatches(claimAction, action))
                    return true;
            }

            return false;
        }

        /// <summary>Shorthand for "should this controller appear in the menu at all".</summary>
        public static bool CanAccessController(this ClaimsPrincipal? principal, string? controller)
            => principal.CanAccessEndpoint(controller, action: null);

        /// <summary>True when the user holds a permission with this exact name.</summary>
        public static bool HasPermissionNamed(this ClaimsPrincipal? principal, string permissionName)
            => principal?.FindAll(HydraClaimTypes.Permission)
                         .Any(c => c.Value == Wildcard
                                   || string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase)) == true;

        /// <summary>Display name for the signed-in user: their name, falling back to their email.</summary>
        public static string? DisplayName(this ClaimsPrincipal? principal)
        {
            if (!principal.IsSignedIn())
                return null;

            var name = principal!.FindFirst(ClaimTypes.Name)?.Value;

            return string.IsNullOrWhiteSpace(name)
                ? principal.FindFirst(ClaimTypes.Email)?.Value
                : name;
        }

        private static bool SegmentMatches(string? claimValue, string? requestValue)
        {
            if (string.IsNullOrWhiteSpace(claimValue)
                || claimValue == Wildcard
                || string.Equals(claimValue, WildcardAlias, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.IsNullOrWhiteSpace(requestValue))
                return false;

            return string.Equals(claimValue, requestValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
