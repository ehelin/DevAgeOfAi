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
        static async Task Main()
        {
            // Create a service collection and configure our DI
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            await InitializeServicesAsync(serviceProvider);

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
            var pythonScriptPath = EnvironmentManager.GetVariable("PythonScriptPath"); // New environment variable

            ValidateInput(openAiKey, msAiKey, msAiDeploymentId, pythonScriptPath);

            // register clients
            services.AddScoped<IClient>(provider => new BLL.Ai.Clients.MicrosoftAi.Client(msAiKey, msAiDeploymentId));
            services.AddScoped<IClient>(provider => new BLL.Ai.Clients.OpenAi.Client(openAiKey));

            // Register the AI Service(s)
            services.AddSingleton<IThirdPartyAiService, OpenAiService>();        
            services.AddSingleton<IThirdPartyAiService, MicrosoftAiService>(); 
            services.AddSingleton(provider => new PythonScriptService(pythonScriptPath));
            services.AddSingleton<IPythonScriptService>(provider => provider.GetRequiredService<PythonScriptService>());
            services.AddSingleton<IThirdPartyAiService>(provider => provider.GetRequiredService<PythonScriptService>());

            services.AddSingleton<IHabitPromptService, HabitPromptService>();

            // Register the HabitTrackerForm
            services.AddSingleton<HabitTrackerForm>();

            return services;
        }

        private static void ValidateInput(string openAiKey, string msAiKey, string msAiDeploymentId, string pythonScriptPath)
        {
            if (string.IsNullOrEmpty(openAiKey))
            {
                throw new InvalidOperationException("openAiKey is missing in environment variables.");
            }

            if (string.IsNullOrEmpty(msAiKey))
            {
                throw new InvalidOperationException("msAiKey is missing in environment variables.");
            }

            if (string.IsNullOrEmpty(msAiDeploymentId))
            {
                throw new InvalidOperationException("msAiDeploymentId is missing in environment variables.");
            }

            if (string.IsNullOrEmpty(pythonScriptPath))
            {
                throw new InvalidOperationException("Python script path is missing in environment variables.");
            }
        }

        private static async Task InitializeServicesAsync(IServiceProvider serviceProvider)
        {
            // Resolve and start the PythonScriptService
            //var pythonScriptService = serviceProvider.GetRequiredService<IPythonScriptService>();
            //await pythonScriptService.StartAsync();
        }
    }
}
