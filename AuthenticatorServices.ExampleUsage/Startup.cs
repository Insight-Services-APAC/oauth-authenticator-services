using System;
using System.Collections.Generic;
using System.Text;
using AuthenticatorServices.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticatorServices.ExampleUsage
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json");

            var configuration = builder.Build();
            services.AddScoped<AuthenticatorEndpointHandler>();
            services.AddAuthenticationServices(configuration.GetSection("AppSettings:AuthorizationSettings"));

            services.AddHttpClient<ITestClient, TestClient>(client =>
                {
                    client.BaseAddress = new Uri("https://google.com");
                })
                .AddHttpMessageHandler<AuthenticatorEndpointHandler>();
        }
    }
}
