using BLL.Ai.Clients;
using Microsoft.Extensions.DependencyInjection;
using Shared.Interfaces;
using System;
using System.Windows.Forms;
using BLL.Ai.Services;

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

            // register clients
            services.AddScoped<IClient>(provider => new BLL.Ai.Clients.MicrosoftAi.Client(msAiKey, msAiDeploymentId));
            services.AddScoped<IClient>(provider => new BLL.Ai.Clients.OpenAi.Client(openAiKey));

            services.AddSingleton<IThirdPartyAiService, OpenAiService>();       // Register the AI Service(s)
            services.AddSingleton<IThirdPartyAiService, MicrosoftAiService>();  // Register the AI Service(s)

            // Register the HabitTrackerForm
            services.AddSingleton<HabitTrackerForm>();

            return services;
        }
    }
}
