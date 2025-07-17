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


        public static ResponseObject Unauthorized(Guid? id = null, string? actionName = null)
        {
            return Failure("Unauthorized", "You are not authorized to perform this action.", id, actionName);
        }

        public static ResponseObject NotFound(string entityName, Guid? id = null, string? actionName = null)
        {
            return Failure("Not Found", $"{entityName} was not found.", id, actionName);
        }

        public static ResponseObject AlreadyExists(string entityName, Guid? id = null, string? actionName = null)
        {
            return Failure("Already Exists", $"{entityName} already exists.", id, actionName);
        }

        public static ResponseObject ServerError(Guid? id = null, string? actionName = null)
        {
            return Failure("Server Error", "An unexpected error occurred. Please try again later.", id, actionName);
        }
    }
}
