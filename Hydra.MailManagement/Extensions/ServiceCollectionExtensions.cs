using FluentEmail.Smtp;
using Hydra.MailManagement.Configuration;
using Hydra.MailManagement.Engine;
using Hydra.MailManagement.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mail;

namespace Hydra.MailManagement.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers Hydra.MailManagement services, configuration options, and FluentEmail SMTP transport engine.
        /// </summary>
        public static IServiceCollection AddHydraMailManagement(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(HydraMailOptions.SectionName);
            var options = new HydraMailOptions();
            section.Bind(options);

            services.Configure<HydraMailOptions>(opt => section.Bind(opt));

            // 1. Configure SmtpClient for FluentEmail
            var smtpClient = new SmtpClient(options.Host, options.Port)
            {
                EnableSsl = options.EnableSsl,
                UseDefaultCredentials = options.UseDefaultCredentials
            };

            if (!string.IsNullOrWhiteSpace(options.User) && !string.IsNullOrWhiteSpace(options.Password))
            {
                smtpClient.Credentials = new NetworkCredential(options.User, options.Password);
            }

            // 2. Register FluentEmail with default sender
            services
                .AddFluentEmail(options.DefaultFromEmail, options.DefaultFromName)
                .AddSmtpSender(smtpClient);

            // 3. Register Hydra Mail Engine and Services
            services.AddScoped<IMailEngineService, FluentEmailEngineService>();
            services.AddScoped<MailService>();
            services.AddScoped<MailTemplateService>();

            return services;
        }

        /// <summary>
        /// Registers Hydra.MailManagement with custom action configuration.
        /// </summary>
        public static IServiceCollection AddHydraMailManagement(
            this IServiceCollection services,
            Action<HydraMailOptions> configureOptions)
        {
            var options = new HydraMailOptions();
            configureOptions(options);

            services.Configure(configureOptions);

            var smtpClient = new SmtpClient(options.Host, options.Port)
            {
                EnableSsl = options.EnableSsl,
                UseDefaultCredentials = options.UseDefaultCredentials
            };

            if (!string.IsNullOrWhiteSpace(options.User) && !string.IsNullOrWhiteSpace(options.Password))
            {
                smtpClient.Credentials = new NetworkCredential(options.User, options.Password);
            }

            services
                .AddFluentEmail(options.DefaultFromEmail, options.DefaultFromName)
                .AddSmtpSender(smtpClient);

            services.AddScoped<IMailEngineService, FluentEmailEngineService>();
            services.AddScoped<MailService>();
            services.AddScoped<MailTemplateService>();

            return services;
        }
    }
}
