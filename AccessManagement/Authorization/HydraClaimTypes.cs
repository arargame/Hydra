namespace Hydra.AccessManagement.Authorization
{
    /// <summary>
    /// Hydra-specific claim types put into the JWT at login, alongside the standard
    /// <see cref="System.Security.Claims.ClaimTypes"/> ones.
    ///
    /// Why both a role NAME and a role ID: the standard ClaimTypes.Role claim carries the role's
    /// NAME so that ordinary ASP.NET/Blazor checks work as written — [Authorize(Roles = "Admin")]
    /// and &lt;AuthorizeView Roles="Admin"&gt;. Until now only the role's Guid was written there,
    /// which made every such check silently impossible (Academy 5.4). The Guid is still useful for
    /// server-side lookups, so it moved to its own claim instead of being dropped.
    /// </summary>
    public static class HydraClaimTypes
    {
        /// <summary>Role's Guid. The role's NAME goes into the standard ClaimTypes.Role claim.</summary>
        public const string RoleId = "hydra:roleId";

        /// <summary>
        /// Name of one permission the user effectively holds (role-inherited or direct). Lets the
        /// UI hide what the user cannot do without an extra round trip — it is a convenience for
        /// rendering, never the authority: the server re-checks on every request.
        /// </summary>
        public const string Permission = "hydra:permission";

        /// <summary>
        /// One endpoint the user may call, as "Controller/Action" (an empty segment is written as
        /// "*"). Emitted only for ControllerActionBased permissions — the same permissions the
        /// server's PermissionEvaluator considers for endpoint decisions, so the menu and the API
        /// cannot disagree about what is reachable.
        ///
        /// Separate from <see cref="Permission"/> because that one carries the permission's NAME,
        /// which says nothing about which endpoint it unlocks.
        /// </summary>
        public const string Endpoint = "hydra:endpoint";

        /// <summary>Value meaning "everything" wherever a Hydra claim can hold a wildcard.</summary>
        public const string Wildcard = "*";
    }
}
