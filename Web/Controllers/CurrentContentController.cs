using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoystonGame.TV;
using RoystonGame.TV.DataModels;
using RoystonGame.Web.DataModels.Enums;
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
            User user = GameManager.MapIPToUser(this.HttpContext.Connection.RemoteIpAddress, out bool newUser);
            if (user?.UserState == null)
            {
                return new BadRequestResult();
            }

            try
            {
                return new JsonResult(user.UserState.UserRequestingCurrentPrompt(user));
            }
            catch(Exception e)
            {
                // If this is reached, the game state is likely corrupted and the lobby will need to be restarted or the user evicted.
                GameManager.ReportGameError(ErrorType.GetContent, user, e);
                return new BadRequestResult();
            }

            /*//Test response
            return new JsonResult(new UserPrompt
            {
                Id = Guid.Empty,
                Title = "This is a title",
                Description = "This is a description",
                RefreshTimeInMs = 1000,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = "This is a sub prompt which has numerous sub strings",
                        StringList = new string[]{"String 1", "<div class=\"color-box\" style=\"background-color: rgb(100,0,0);\"></div>Test", "String 3"}
                    },
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
                        Prompt = "This is a sub prompt with a background",
                        Drawing = new DrawingPromptMetadata
                        {
                            CanvasBackground="data:image/png;base64, iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==",
                            HeightInPx=200,
                            WidthInPx=300,
                        }
                    },
                    new SubPrompt
                    {
                        Prompt = "This is a sub prompt with a premade drawing",
                        Drawing = new DrawingPromptMetadata
                        {
                            PremadeDrawing="data:image/png;base64, iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==",
                            HeightInPx=300,
                            WidthInPx=200,
                        }
                    },
                    new SubPrompt
                    {
                        Prompt = "This is a sub prompt with a dropdown",
                        Dropdown = new string[]{ "A", "B", "C" }
                    }
                },
                SubmitButton = true
            });*/
        }
    }
}
