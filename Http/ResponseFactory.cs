using Hydra.Http;
using System;
using System.Collections.Generic;

namespace Hydra.Core.Http
{
    public static class ResponseFactory
    {
        public static ResponseObject Success(
            object? data = null,
            Guid? id = null,
            string? actionName = null,
            string? title = "Success",
            string? text = null,
            string? redirectionLink = null,
            string? redirectionLinkText = "Back")
        {
            var response = new ResponseObject
            {
                Success = true,
                Id = id ?? Guid.NewGuid(),
                ActionName = actionName,
                Data = data
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                var msg = new ResponseObjectMessage(title, text, showWhenSuccess: true);
                if (!string.IsNullOrEmpty(redirectionLink))
                {
                    msg.SetRedirectionLink(redirectionLink, redirectionLinkText);
                }

                response.Messages.Add(msg);
            }

            return response;
        }

        public static ResponseObject Failure(
            List<(string title, string text)> errors,
            Guid? id = null,
            string? actionName = null,
            string? redirectionLink = null,
            string? redirectionLinkText = "Back")
        {
            var response = new ResponseObject
            {
                Success = false,
                Id = id ?? Guid.NewGuid(),
                ActionName = actionName
            };

            foreach (var (title, text) in errors)
            {
                var msg = new ResponseObjectMessage(title, text, showWhenSuccess: false);
                if (!string.IsNullOrEmpty(redirectionLink))
                {
                    msg.SetRedirectionLink(redirectionLink, redirectionLinkText);
                }

                response.Messages.Add(msg);
            }

            return response;
        }

        public static ResponseObject Failure(
            string title,
            string text,
            Guid? id = null,
            string? actionName = null,
            string? redirectionLink = null,
            string? redirectionLinkText = "Back")
        {
            return Failure(
                new List<(string, string)> { (title, text) },
                id,
                actionName,
                redirectionLink,
                redirectionLinkText
            );
        }

        // ---------------------------------------------------------------------------------
        // Named failures.
        //
        // These already existed and their wording/behaviour is unchanged; the only addition is
        // that each one now also stamps the matching HTTP status code onto the envelope, so the
        // transport layer can mirror it on the wire and the UI can look the code up in
        // HydraStatusCatalog instead of pattern-matching on message text.
        // ---------------------------------------------------------------------------------

        public static ResponseObject Unauthorized(Guid? id = null, string? actionName = null)
        {
            var response = Failure("Unauthorized", "You are not authorized to perform this action.", id, actionName);
            response.StatusCode = 401;
            return response;
        }

        /// <summary>
        /// 403 — signed in, but not permitted. Distinct from <see cref="Unauthorized"/> (401),
        /// which means "we do not know who you are". Signing in again fixes a 401; it does not
        /// fix a 403.
        /// </summary>
        public static ResponseObject Forbidden(string? detail = null, Guid? id = null, string? actionName = null)
        {
            var definition = HydraStatusCatalog.Get(403);

            var text = string.IsNullOrWhiteSpace(detail)
                ? definition.UserMessage
                : $"{definition.UserMessage} ({detail})";

            var response = Failure(definition.Title, text, id, actionName);
            response.StatusCode = 403;
            return response;
        }

        public static ResponseObject BadRequest(string? detail = null, Guid? id = null, string? actionName = null)
        {
            var definition = HydraStatusCatalog.Get(400);

            var text = string.IsNullOrWhiteSpace(detail)
                ? definition.UserMessage
                : $"{definition.UserMessage} ({detail})";

            var response = Failure(definition.Title, text, id, actionName);
            response.StatusCode = 400;
            return response;
        }

        public static ResponseObject NotFound(string entityName, Guid? id = null, string? actionName = null)
        {
            var response = Failure("Not Found", $"{entityName} was not found.", id, actionName);
            response.StatusCode = 404;
            return response;
        }

        public static ResponseObject AlreadyExists(string entityName, Guid? id = null, string? actionName = null)
        {
            var response = Failure("Already Exists", $"{entityName} already exists.", id, actionName);
            response.StatusCode = 409;
            return response;
        }

        public static ResponseObject ServerError(Guid? id = null, string? actionName = null)
        {
            var response = Failure("Server Error", "An unexpected error occurred. Please try again later.", id, actionName);
            response.StatusCode = 500;
            return response;
        }

        /// <summary>
        /// Builds a failure envelope for ANY status code straight from the catalog — the generic
        /// entry point when no named helper above fits (422, 429, 503, ...).
        /// </summary>
        public static ResponseObject FromStatus(int statusCode, string? detail = null, Guid? id = null, string? actionName = null)
        {
            var definition = HydraStatusCatalog.Get(statusCode);

            var text = string.IsNullOrWhiteSpace(detail)
                ? definition.UserMessage
                : $"{definition.UserMessage} ({detail})";

            var response = Failure(definition.Title, text, id, actionName);
            response.StatusCode = statusCode;
            return response;
        }
    }
}
