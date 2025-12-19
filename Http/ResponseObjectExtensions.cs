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

    }

}
