using System;

namespace Hydra.AccessManagement.Authorization
{
    /// <summary>
    /// Marks an endpoint as "any signed-in user may call this, no specific permission needed".
    ///
    /// Needed because the system is deny-by-default: without this marker, an endpoint that should
    /// be available to every authenticated user (logging out, reading your own profile, changing
    /// your own password) would require a Permission row for every single user or role — and a
    /// user who cannot log out because they lack a "Logout" permission is a bug, not security.
    ///
    /// This is strictly weaker than [AllowAnonymous]: identity is still required, only the
    /// permission lookup is skipped.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class HydraAllowAuthenticatedAttribute : Attribute
    {
    }
}
