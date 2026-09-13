using Hydra.MailManagement.WebApi.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Hydra.MailManagement.WebApi.Extensions
{
    public static class MvcBuilderExtensions
    {
        /// <summary>
        /// Registers Hydra.MailManagement API controllers (MailController, MailTemplateController)
        /// into the host WebApi application's routing pipeline via ApplicationPart.
        /// </summary>
        public static IMvcBuilder AddHydraMailManagementControllers(this IMvcBuilder builder)
        {
            builder.AddApplicationPart(typeof(MailController).Assembly);
            return builder;
        }

        /// <summary>
        /// Registers Hydra.MailManagement API controllers via IServiceCollection.
        /// </summary>
        public static IServiceCollection AddHydraMailManagementApi(this IServiceCollection services)
        {
            services.AddControllers()
                    .AddHydraMailManagementControllers();

            return services;
        }
    }
}
