using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Tests.Helpers;
using Xunit;

namespace Tests
{
    public class PythonScriptTests
    {
        private const string ScriptPath = "C:\\temp\\New folder\\Ai\\AiModelRunner\\PythonApplication1.py";
        private static PythonScriptRunner runner;

        public PythonScriptTests()
        {
            if (runner == null)
            {
                runner = new PythonScriptRunner(ScriptPath);
            } 
        }

        [Theory]
        [InlineData("Can you tell me about yourself?")]
        [InlineData("Can you tell me one good joke?")]
        [InlineData("Do you have an opinion on jokes?")]
        [InlineData("Can you tell me something smart?")]
        public async Task TestPythonScriptResponse(string input)
        {
            // Run the script and get the response
            var response = await runner.SendInputAsync(input);

            Assert.NotNull(response);
            Assert.Contains("Please respond to the following question independently:", response);
            Assert.Contains("Answer", response);
        }
    }
}
