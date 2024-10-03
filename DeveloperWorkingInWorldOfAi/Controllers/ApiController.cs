using DeveloperWorkingInWorldOfAi.Models;
using Microsoft.AspNetCore.Mvc;
using Shared.interfaces;
using System.Diagnostics;

namespace DeveloperWorkingInWorldOfAi.Controllers
{
    public class OpenAiApiController : Controller
    {
        private readonly IThirdPartyAiService openAiService;

        public OpenAiApiController(IThirdPartyAiService openAiService)
        {
            this.openAiService = openAiService;
        }

        //[HttpGet]
        //public string Get(string prompt)
        //{
        //    var result = this.openAiService.GetSuggestion(prompt);

        //    return result;
        //}
    }
}
