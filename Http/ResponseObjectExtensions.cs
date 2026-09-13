using Hydra.Core;
using Hydra.Services.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Http
{
    public static class ResponseObjectExtensions
    {
        public static T Ok<T>(this T response, object data) where T : IResponseObject
            => (T)response.SetSuccess(true).SetData(data);

        public static T Fail<T>(this T response, string title, string message) where T : IResponseObject
            => (T)response.SetSuccess(false).AddExtraMessage(new ResponseObjectMessage(title, message, false));

        public static T MergeRepositoryMessages<T, TEntity>(this T response, Service<TEntity> service)
            where T : IResponseObject
            where TEntity : BaseObject<TEntity>, new()
        {
            if (!response.Success)
            {
                var responseObjectMessages = service.GetRepositoryMessages();

                if (responseObjectMessages.Any())
                    response.AddExtraMessages(responseObjectMessages);
            }

            return response;
        }

        public static IResponseObject AddValidationMessages(this IResponseObject response, IEnumerable<ValidationResult> validations)
        {
            foreach (var validation in validations)
            {
                var message = ResponseMessageProvider.FromValidationResult(validation);

                response.AddExtraMessage(message);
            }

            return response;
        }

        /// <summary>
        /// Marks the envelope as a failure carrying a specific HTTP status code, and appends the
        /// user-facing sentence for that code from <see cref="HydraStatusCatalog"/>.
        ///
        /// This does not replace anything: the response is still a plain ResponseObject with the
        /// same fields. It just stops the caller from having to invent wording for "403" at every
        /// call site — the wording lives in exactly one place.
        /// </summary>
        /// <param name="detail">Optional specific reason, appended in parentheses after the generic sentence.</param>
        public static T AsStatus<T>(this T response, int statusCode, string? detail = null) where T : IResponseObject
        {
            response.SetStatusCode(statusCode);

            var isFailure = HydraStatusCatalog.KindOf(statusCode) is HydraStatusKind.ClientError or HydraStatusKind.ServerError;

            if (isFailure)
            {
                response.SetSuccess(false);
                response.AddExtraMessage(HydraStatusCatalog.ToMessage(statusCode, detail));
            }

            return response;
        }

        /// <summary>401 — the caller is not signed in, or the token is missing/expired/invalid.</summary>
        public static T AsUnauthorized<T>(this T response, string? detail = null) where T : IResponseObject
            => response.AsStatus(401, detail);

        /// <summary>403 — the caller IS signed in, but has no permission for this endpoint.</summary>
        public static T AsForbidden<T>(this T response, string? detail = null) where T : IResponseObject
            => response.AsStatus(403, detail);
    }

}
