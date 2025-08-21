using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace AiModelApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddControllers();
			
			// Add Swagger services
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
				{
					Title = "AI Model API",
					Version = "v1",
					Description = "API for interacting with fine-tuned AI models for habit suggestions (HTTP-based)",
					Contact = new Microsoft.OpenApi.Models.OpenApiContact
					{
						Name = "DevAgeOfAi Project",
						Url = new Uri("https://github.com/ehelin/DevAgeOfAi")
					}
				});
				
				// Include XML comments
				var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				if (File.Exists(xmlPath))
				{
					c.IncludeXmlComments(xmlPath);
				}
			});

			var app = builder.Build();
			
			app.UseHttpsRedirection();
			
			// Configure Swagger UI
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Model API v1");
				c.RoutePrefix = "swagger";
			});
			
			app.UseAuthorization();
			app.MapControllers();
			
			Console.WriteLine("API Server starting...");
			Console.WriteLine("Make sure to start the Python AI model server first:");
			Console.WriteLine("cd Ai\\AiModelRunner && python ai_model_server.py");
			Console.WriteLine("");
			
			app.Run();
		}
	}
}