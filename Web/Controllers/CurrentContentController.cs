using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoystonGame.TV;
using RoystonGame.TV.DataModels;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrentContentController : ControllerBase
    {
        private readonly ILogger<CurrentContentController> _logger;

        public CurrentContentController(ILogger<CurrentContentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            // TODO handle IDs for real
            // TODO scramble radio button answer order
            User user = GameManager.MapIPToUser(this.HttpContext.Connection.RemoteIpAddress);
            return new JsonResult(user.UserState.UserRequestingCurrentPrompt(user));
            /* //Test response
            return new JsonResult(new UserPrompt
            {
                Title = "This is a title",
                Description = "This is a description",
                RefreshTimeInMs = 1000,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = "This is a sub prompt which only asks for a short answer",
                        ShortAnswer = true,
                    },
                    new SubPrompt
                    {
                        Prompt = "This is a sub prompt that asks for radio button answers",
                        Answers = new string[] {"This is an answer", "another answer", "the best answer", "all of the above"}
                    },
                    new SubPrompt
                    {
                        Prompt = "This is a sub prompt that asks for a drawing",
                        Drawing = true,
                    },
                    new SubPrompt
                    {
                        Prompt = "This is a sub prompt that asks for all 3 types",
                        ShortAnswer= true,
                        Drawing = true,
                        Answers = new string[] {"This is an answer", "another answer", "the best answer", "all of the above"}
                    },
                },
               SubmitButton = true
            });*/
        }
    }
}
