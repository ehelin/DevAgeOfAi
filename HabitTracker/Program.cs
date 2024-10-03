using Microsoft.Extensions.DependencyInjection;
using Shared.interfaces;
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

            // Register the OpenAiClient
            services.AddSingleton<BLL.Services.OpenAi.Client>();

            // Register the AI Service
            services.AddSingleton<IThirdPartyAiService, BLL.Services.OpenAi.Service>();

            // Register the HabitTrackerForm
            services.AddSingleton<HabitTrackerForm>();

            return services;
        }
    }
}
