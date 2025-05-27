using Microsoft.Extensions.DependencyInjection;
using RevitTemplate.Core.Services;
using RevitTemplate.Infrastructure;
using RevitTemplate.Providers;
using RevitTemplate.Services;
using RevitTemplate.UI.Views;
using RevitTemplate.UI.Views.Pages;
using System;
using System.Net.Http;

namespace RevitTemplate.Services
{
    /// <summary>
    /// Configuration class for dependency injection services.
    /// </summary>
    public static class DependencyInjectionConfig
    {        /// <summary>
        /// Configures services for dependency injection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<RevitApp>(); // Ensure RevitApp is registered as a singleton
            services.AddSingleton<RevitUiApplicationProvider>();
            // Core Services
            services.AddTransient<IRevitDocumentService>(provider => 
            {
                // UIApplication will be provided at runtime when needed
                // For now, we'll create a service that can work without it
                return new RevitDocumentService(null);
            });
            services.AddSingleton<TokenService>();
            services.AddTransient<FamilyParameterService>();
            services.AddSingleton<WorkerServiceProxy>();

            // Event Handlers
            services.AddSingleton<AddCiclopeParametersEventHandler>();
            services.AddSingleton<FillCiclopeParametersEventHandler>();
            services.AddSingleton<SelectElementsEventHandler>();
            services.AddSingleton<UpdateElementsGridEventHandler>();
            services.AddSingleton<UpdateFamilyTypesGridEventHandler>();

            // HTTP Client
            services.AddSingleton<HttpService>();
            services.AddSingleton<HubService>();

            // UI Services and Windows
            services.AddTransient<CiclopeWindow>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LogPage>();
        }
    }
}
