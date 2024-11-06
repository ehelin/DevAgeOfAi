using Microsoft.AspNetCore.Mvc;

namespace AiModelApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AiModelApiController : ControllerBase
	{
		public AiModelApiController()
		{
		}

		[HttpGet]
		public IActionResult Get()
		{
			return Ok("AI Model Api Controller");
		}

		[HttpGet("chat")]
		public IActionResult Chat(string message)
		{
			// clear out any previous response
			PythonRunner.Response = string.Empty;

			// set our message
			PythonRunner.Message = message;

			// wait for a response and then return if
			while (string.IsNullOrEmpty(PythonRunner.Response))
			{
				System.Threading.Thread.Sleep(10);
			}

			return Ok(PythonRunner.Response);
		}
	}
}