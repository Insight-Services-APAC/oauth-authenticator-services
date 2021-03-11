using System.Configuration;
using AuthenticatorServices.Models;
using AuthenticatorServices.Services;
using AuthenticatorServices.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticatorServices.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAuthenticationServices(this IServiceCollection services, IConfigurationSection authorizationSettings)
        {
            services.Configure<AuthorizationSettingsModel>(authorizationSettings);
            services.AddSingleton<AuthorizationSettingsModel>();
            services.AddTransient<IAuthTokenService, AuthTokenService>();
            services.AddTransient<IAuthTokenService, AuthTokenService>();
            services.AddMemoryCache();
            services.AddLogging();
        }
    }
}

