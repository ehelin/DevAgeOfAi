using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AiModelApi.Services;

namespace AiModelApi.Controllers
{
	/// <summary>
	/// AI Model API Controller for habit suggestions
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[Produces("application/json")]
	public class AiModelApiController : ControllerBase
	{
		private readonly AiModelHttpService _aiModelService;

		public AiModelApiController()
		{
			_aiModelService = new AiModelHttpService();
		}

		/// <summary>
		/// Health check endpoint to verify the API is running
		/// </summary>
		/// <returns>API status message</returns>
		[HttpGet]
		[ProducesResponseType(200)]
		public async Task<IActionResult> Get()
		{
			var isModelReady = await _aiModelService.IsModelReadyAsync();
			return Ok(new { 
				status = "API is running", 
				modelReady = isModelReady,
				message = isModelReady ? "AI model is ready" : "AI model is not available" 
			});
		}

		/// <summary>
		/// Chat with the AI model to get habit suggestions
		/// </summary>
		/// <param name="message">The message/prompt to send to the AI model</param>
		/// <returns>AI-generated habit suggestion</returns>
		/// <response code="200">Returns the AI-generated response</response>
		/// <response code="400">If the message parameter is empty or null</response>
		/// <response code="503">If the AI model is not ready</response>
		/// <response code="408">If the AI model response times out</response>
		[HttpGet("chat")]
		[ProducesResponseType(200, Type = typeof(string))]
		[ProducesResponseType(400)]
		[ProducesResponseType(503)]
		[ProducesResponseType(408)]
		public async Task<IActionResult> Chat([Required] string message)
		{
			if (string.IsNullOrEmpty(message))
				return BadRequest("Message cannot be empty");

			// Check if AI model is ready
			var isReady = await _aiModelService.IsModelReadyAsync();
			if (!isReady)
				return StatusCode(503, "AI model is not ready. Please start the Python AI model server first.");

			// Get response from AI model
			var response = await _aiModelService.GetResponseAsync(message);
			
			// Check for error responses
			if (response.StartsWith("Error:") || response.StartsWith("Connection error:") || response.StartsWith("An error occurred:"))
				return StatusCode(500, response);
			
			if (response.StartsWith("Request timeout"))
				return StatusCode(408, response);

			return Ok(response);
		}

		public void Dispose()
		{
			_aiModelService?.Dispose();
		}
	}
}