using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoystonGame.Web.DataModels.Requests.Other;


namespace RoystonGame.Web.Controllers.Other
{

    public class OtherController : ControllerBase

    {
        public OtherController (ILogger<OtherController> logger)
        {
            Logger = logger;
        }
        private ILogger<OtherController> Logger { get; set; }

        [HttpGet]
        [Route("Feedback-Api")]
        public IActionResult SubmitFeedback(FeedbackRequest request)
        {
            Console.WriteLine(request);
            if (!string.IsNullOrWhiteSpace(request?.Feedback))
            {
                Logger.LogInformation(request.Feedback);
            }
            return new OkResult();
        }

    }
}
