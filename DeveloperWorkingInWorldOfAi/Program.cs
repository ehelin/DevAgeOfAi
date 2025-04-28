namespace DeveloperWorkingInWorldOfAi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.Services.AddTransient<IThirdPartyAiService, OpenAiService>();
            builder.Services.AddControllersWithViews();
            // space   
            var app = builder.Build();
               //test
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
              
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();  // Dummy update to fix Git history.

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
