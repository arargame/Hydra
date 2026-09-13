using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydra.Http
{
    /// <summary>
    /// Broad classification of an HTTP status code, used by the UI to pick a tone
    /// (colour, icon, whether to offer a retry) without hard-coding per-code logic.
    /// </summary>
    public enum HydraStatusKind
    {
        Success,
        Redirect,
        ClientError,
        ServerError
    }

    /// <summary>
    /// One row of the status-code catalog: the code itself, a short internal title,
    /// and the sentence that is actually shown to the end user.
    /// </summary>
    public sealed class HydraStatusDefinition
    {
        public int Code { get; }

        /// <summary>Short label, e.g. "Permission Denied". Shown as the page/banner heading.</summary>
        public string Title { get; }

        /// <summary>The sentence the end user reads. This is the ONLY user-facing text for a code.</summary>
        public string UserMessage { get; }

        public HydraStatusKind Kind { get; }

        /// <summary>
        /// Symbolic icon name (not an image path, not an emoji) so each UI can map it to
        /// whatever icon set it uses: "lock", "search", "warning", "server", "clock", "info".
        /// </summary>
        public string Icon { get; }

        /// <summary>Whether it is meaningful to offer the user a "try again" button.</summary>
        public bool IsRetryable { get; }

        public HydraStatusDefinition(int code, string title, string userMessage, HydraStatusKind kind, string icon, bool isRetryable = false)
        {
            Code = code;
            Title = title;
            UserMessage = userMessage;
            Kind = kind;
            Icon = icon;
            IsRetryable = isRetryable;
        }
    }

    /// <summary>
    /// The single place where HTTP status codes are turned into something a human can read.
    ///
    /// Hydra's transport contract is unchanged: every endpoint still returns a
    /// <see cref="ResponseObject"/> envelope. What this catalog adds is the missing half —
    /// the envelope now carries a <see cref="ResponseObject.StatusCode"/>, and this class
    /// says what that number MEANS to the person looking at the screen.
    ///
    /// Localisation note: this class is the only file that has to change to translate every
    /// status message in every Hydra-based application.
    /// </summary>
    public static class HydraStatusCatalog
    {
        private static readonly Dictionary<int, HydraStatusDefinition> _all = new[]
        {
            // --- 2xx -------------------------------------------------------------------
            new HydraStatusDefinition(200, "OK", "The operation completed successfully.", HydraStatusKind.Success, "info"),
            new HydraStatusDefinition(201, "Created", "The record was created successfully.", HydraStatusKind.Success, "info"),
            new HydraStatusDefinition(204, "No Content", "The operation completed. There is nothing to display.", HydraStatusKind.Success, "info"),

            // --- 3xx -------------------------------------------------------------------
            new HydraStatusDefinition(301, "Moved Permanently", "This page has moved to a new address.", HydraStatusKind.Redirect, "info"),
            new HydraStatusDefinition(302, "Found", "You are being redirected to another page.", HydraStatusKind.Redirect, "info"),
            new HydraStatusDefinition(304, "Not Modified", "Nothing has changed since you last loaded this.", HydraStatusKind.Redirect, "info"),
            new HydraStatusDefinition(307, "Temporary Redirect", "You are being redirected to another page.", HydraStatusKind.Redirect, "info"),
            new HydraStatusDefinition(308, "Permanent Redirect", "This page has moved to a new address.", HydraStatusKind.Redirect, "info"),

            // --- 4xx -------------------------------------------------------------------
            new HydraStatusDefinition(400, "Bad Request", "Some of the information sent was not valid. Please review the form and try again.", HydraStatusKind.ClientError, "warning", isRetryable: true),
            new HydraStatusDefinition(401, "Sign In Required", "Your session has ended or you are not signed in. Please sign in again to continue.", HydraStatusKind.ClientError, "lock"),
            new HydraStatusDefinition(403, "Permission Denied", "You do not have permission for this operation. If you believe this is a mistake, contact your system administrator.", HydraStatusKind.ClientError, "lock"),
            new HydraStatusDefinition(404, "Not Found", "The record or page you are looking for could not be found.", HydraStatusKind.ClientError, "search"),
            new HydraStatusDefinition(405, "Not Allowed", "This operation is not available on this page.", HydraStatusKind.ClientError, "warning"),
            new HydraStatusDefinition(408, "Timed Out", "The request took too long and was cancelled. Please try again.", HydraStatusKind.ClientError, "clock", isRetryable: true),
            new HydraStatusDefinition(409, "Conflict", "This record was changed by someone else in the meantime. Please reload and try again.", HydraStatusKind.ClientError, "warning", isRetryable: true),
            new HydraStatusDefinition(410, "Gone", "This record is no longer available.", HydraStatusKind.ClientError, "search"),
            new HydraStatusDefinition(413, "Too Large", "The file you are sending is too large.", HydraStatusKind.ClientError, "warning"),
            new HydraStatusDefinition(415, "Unsupported Type", "This file type is not supported.", HydraStatusKind.ClientError, "warning"),
            new HydraStatusDefinition(422, "Validation Failed", "Some fields could not be accepted. Please check the highlighted fields.", HydraStatusKind.ClientError, "warning", isRetryable: true),
            new HydraStatusDefinition(429, "Too Many Requests", "You have made too many requests in a short time. Please wait a moment and try again.", HydraStatusKind.ClientError, "clock", isRetryable: true),

            // --- 5xx -------------------------------------------------------------------
            new HydraStatusDefinition(500, "Something Went Wrong", "An unexpected error occurred on our side. The problem has been logged.", HydraStatusKind.ServerError, "server", isRetryable: true),
            new HydraStatusDefinition(501, "Not Implemented", "This feature is not available yet.", HydraStatusKind.ServerError, "server"),
            new HydraStatusDefinition(502, "Bad Gateway", "We could not reach a service we depend on. Please try again shortly.", HydraStatusKind.ServerError, "server", isRetryable: true),
            new HydraStatusDefinition(503, "Service Unavailable", "The service is temporarily unavailable. Please try again shortly.", HydraStatusKind.ServerError, "server", isRetryable: true),
            new HydraStatusDefinition(504, "Gateway Timeout", "A service we depend on took too long to answer. Please try again.", HydraStatusKind.ServerError, "clock", isRetryable: true),
        }.ToDictionary(d => d.Code);

        public static IReadOnlyDictionary<int, HydraStatusDefinition> All => _all;

        public static bool IsKnown(int code) => _all.ContainsKey(code);

        /// <summary>
        /// Returns the definition for a code. An unknown code never throws and never shows
        /// the user a bare number — it falls back to a generic definition for its range.
        /// </summary>
        public static HydraStatusDefinition Get(int code)
        {
            if (_all.TryGetValue(code, out var definition))
                return definition;

            return KindOf(code) switch
            {
                HydraStatusKind.Success => new HydraStatusDefinition(code, "Completed", "The operation completed.", HydraStatusKind.Success, "info"),
                HydraStatusKind.Redirect => new HydraStatusDefinition(code, "Redirecting", "You are being redirected to another page.", HydraStatusKind.Redirect, "info"),
                HydraStatusKind.ClientError => new HydraStatusDefinition(code, "Request Rejected", "The request could not be completed. Please review what you sent and try again.", HydraStatusKind.ClientError, "warning", isRetryable: true),
                _ => new HydraStatusDefinition(code, "Something Went Wrong", "An unexpected error occurred on our side. The problem has been logged.", HydraStatusKind.ServerError, "server", isRetryable: true),
            };
        }

        public static HydraStatusKind KindOf(int code)
        {
            if (code >= 500) return HydraStatusKind.ServerError;
            if (code >= 400) return HydraStatusKind.ClientError;
            if (code >= 300) return HydraStatusKind.Redirect;
            return HydraStatusKind.Success;
        }

        /// <summary>
        /// Convenience for building an envelope message straight from the catalog.
        /// <paramref name="detail"/> lets the caller append a specific reason
        /// ("Permission 'Role/Delete' required") after the generic sentence.
        /// </summary>
        public static ResponseObjectMessage ToMessage(int code, string? detail = null)
        {
            var definition = Get(code);

            var text = string.IsNullOrWhiteSpace(detail)
                ? definition.UserMessage
                : $"{definition.UserMessage} ({detail})";

            return new ResponseObjectMessage(definition.Title, text, showWhenSuccess: false);
        }
    }
}
