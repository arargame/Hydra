using Hydra.DI;
using Hydra.IdentityAndAccess;
using Hydra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hydra.AccessManagement.Authorization
{
    /// <summary>
    /// The outcome of one authorization question, kept as a value so the caller can both act on
    /// it (allow/deny) and log/return WHY — a bare bool would throw the reason away, and "why was
    /// I refused" is the single most common support question an access system produces.
    /// </summary>
    public sealed class PermissionDecision
    {
        public bool Allowed { get; }

        /// <summary>Name of the permission that granted access, when one did.</summary>
        public string? MatchedPermissionName { get; }

        /// <summary>Short, log-friendly explanation. Never shown verbatim to the end user.</summary>
        public string Reason { get; }

        private PermissionDecision(bool allowed, string reason, string? matchedPermissionName = null)
        {
            Allowed = allowed;
            Reason = reason;
            MatchedPermissionName = matchedPermissionName;
        }

        public static PermissionDecision Allow(string reason, string? matchedPermissionName = null)
            => new PermissionDecision(true, reason, matchedPermissionName);

        public static PermissionDecision Deny(string reason)
            => new PermissionDecision(false, reason);
    }

    public interface IPermissionEvaluator
    {
        /// <summary>
        /// Decides whether the given user (or an anonymous caller, when <paramref name="userId"/>
        /// is null) may call <paramref name="controller"/>/<paramref name="action"/>.
        /// </summary>
        Task<PermissionDecision> EvaluateEndpointAsync(Guid? userId, string? controller, string? action);

        /// <summary>
        /// Every permission the user effectively holds: those granted through their roles plus
        /// those granted directly. Used for the endpoint check and for building login claims.
        /// </summary>
        Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId);
    }

    /// <summary>
    /// Turns the Permission/RolePermission/SystemUserPermission rows — which until now were pure
    /// data with nothing reading them (see Hydra Academy 5.5) — into actual allow/deny decisions.
    ///
    /// Matching rules, deliberately narrow:
    ///
    ///  1. A permission must be Enabled to count at all.
    ///
    ///  2. Only permissions whose Type is <see cref="PermissionType.ControllerActionBased"/>, or
    ///     whose Name is exactly "*", take part in ENDPOINT authorization. This matters: an
    ///     EntityPropertyBased row (Entity="Request", Property="Price") leaves Controller/Action
    ///     empty, and since empty means "any" in rule 3, such a row would otherwise silently
    ///     grant access to every endpoint in the system. Field-level permission types are
    ///     reserved for view/menu/component checks and must not leak into endpoint decisions.
    ///
    ///  3. Within a participating permission, each segment matches if it is null, empty, "*" or
    ///     "All" (all meaning "any"), otherwise it must equal the request's value,
    ///     case-insensitively. So Controller="Role", Action="All" grants every action on
    ///     RoleController — this is the spelling Academy 5.5 specifies.
    ///
    ///  4. Name == "*" is a global grant — the superuser row the Default Admin seed writes.
    ///
    ///  5. AllowAnonymous is honoured as an escape valve: a permission row matching the endpoint
    ///     with AllowAnonymous=true opens it to everyone, signed in or not. This is what makes a
    ///     deny-by-default system workable without recompiling — a public endpoint is one row,
    ///     not a code change.
    /// </summary>
    public class PermissionEvaluator : IPermissionEvaluator
    {
        private const string Wildcard = "*";

        /// <summary>
        /// Second accepted spelling of "any", kept because Academy 5.5 specifies the wildcard as
        /// «"*" veya "All"» — both tokens must mean the same thing or the documented design and
        /// the running code would disagree.
        /// </summary>
        private const string WildcardAlias = "All";

        private readonly ServiceInjector _injector;

        public PermissionEvaluator(ServiceInjector injector)
        {
            _injector = injector;
        }

        public async Task<PermissionDecision> EvaluateEndpointAsync(Guid? userId, string? controller, string? action)
        {
            // Anonymous caller: the only way through is a permission row explicitly marked public.
            if (userId is null || userId == Guid.Empty)
            {
                return await EvaluateAnonymousAsync(controller, action);
            }

            var permissions = await GetEffectivePermissionsAsync(userId.Value);

            var globalGrant = permissions.FirstOrDefault(p => p.Enabled && p.Name == Wildcard);
            if (globalGrant != null)
                return PermissionDecision.Allow("Global wildcard permission", globalGrant.Name);

            var match = permissions.FirstOrDefault(p => ParticipatesInEndpointAuthorization(p) && Matches(p, controller, action));
            if (match != null)
                return PermissionDecision.Allow("Matched permission", match.Name);

            // The user holds no matching permission — but the endpoint itself may be public.
            var anonymousDecision = await EvaluateAnonymousAsync(controller, action);
            if (anonymousDecision.Allowed)
                return anonymousDecision;

            return PermissionDecision.Deny($"No permission grants {controller}/{action}");
        }

        public async Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId)
        {
            var effective = new Dictionary<Guid, Permission>();

            // 1. Permissions inherited through the user's roles.
            var roleSystemUserService = _injector.GetServiceLazy<RoleSystemUserService>().Value;
            var rolePermissionService = _injector.GetServiceLazy<RolePermissionService>().Value;

            var roles = await roleSystemUserService.GetRolesAsync(userId);

            foreach (var role in roles)
            {
                var rolePermissions = await rolePermissionService.GetPermissionsAsync(role.Id);

                foreach (var permission in rolePermissions)
                    effective[permission.Id] = permission;
            }

            // 2. Permissions granted directly to the user, bypassing roles.
            var systemUserPermissionService = _injector.GetServiceLazy<SystemUserPermissionService>().Value;

            var directPermissions = await systemUserPermissionService.GetPermissionsAsync(userId);

            foreach (var permission in directPermissions)
                effective[permission.Id] = permission;

            return effective.Values.Where(p => p.Enabled).ToList();
        }

        private async Task<PermissionDecision> EvaluateAnonymousAsync(string? controller, string? action)
        {
            var permissionService = _injector.GetServiceLazy<PermissionService>().Value;

            var publicPermissions = await permissionService.SelectThenCache(p => p.AllowAnonymous && p.Enabled);

            var match = publicPermissions.FirstOrDefault(p => ParticipatesInEndpointAuthorization(p) && Matches(p, controller, action));

            return match != null
                ? PermissionDecision.Allow("Endpoint is marked AllowAnonymous", match.Name)
                : PermissionDecision.Deny($"Anonymous access is not allowed for {controller}/{action}");
        }

        private static bool IsWildcard(string? value)
            => string.IsNullOrWhiteSpace(value)
               || value == Wildcard
               || string.Equals(value, WildcardAlias, StringComparison.OrdinalIgnoreCase);

        private static bool ParticipatesInEndpointAuthorization(Permission permission)
            => permission.Type == PermissionType.ControllerActionBased || permission.Name == Wildcard;

        private static bool Matches(Permission permission, string? controller, string? action)
            => SegmentMatches(permission.Controller, controller) && SegmentMatches(permission.Action, action);

        /// <summary>
        /// A permission segment matches when it is unset or "*" (any), or equals the request's
        /// value ignoring case. An unset value on the REQUEST side never matches a specific
        /// permission segment — we refuse rather than guess.
        /// </summary>
        private static bool SegmentMatches(string? permissionValue, string? requestValue)
        {
            if (IsWildcard(permissionValue))
                return true;

            if (string.IsNullOrWhiteSpace(requestValue))
                return false;

            return string.Equals(permissionValue, requestValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
