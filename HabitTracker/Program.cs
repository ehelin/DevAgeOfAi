using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Shared.interfaces;
using Shared.Interfaces;
using System;
using System.Windows.Forms;

namespace HabitTracker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Create a service collection and configure our DI
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Start the application
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create and run the main form, injecting the required dependencies
            var mainForm = serviceProvider.GetRequiredService<HabitTrackerForm>();
            Application.Run(mainForm);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // Retrieve the API key from environment variables
            var openAiKey = EnvironmentManager.GetVariable("OpenAiKey");
            var msAiKey = EnvironmentManager.GetVariable("MsAiKey");
            var msAiDeploymentId = EnvironmentManager.GetVariable("MsAiDeploymentId");

            if (string.IsNullOrEmpty(openAiKey) || string.IsNullOrEmpty(msAiKey))
            {
                throw new InvalidOperationException("API key(s) is missing in environment variables.");
            }

            services.AddSingleton(new BLL.Services.OpenAi.Client(openAiKey));
            services.AddSingleton(new BLL.Services.MicrosoftAi.Client(msAiKey, msAiDeploymentId));

            //services.AddScoped<IClient, OpenAiClient>();

            //// Or for MsAiClient
            //services.AddScoped<IClient, MsAiClient>();

            // Register the AI Service
            services.AddSingleton<IThirdPartyAiService, Service>();

            // Register the HabitTrackerForm
            services.AddSingleton<HabitTrackerForm>();

            return services;
        }
    }
}
