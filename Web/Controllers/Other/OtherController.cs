using System;
using Microsoft.AspNetCore.Mvc;
using RoystonGame.Web.DataModels.Requests.Other;


namespace RoystonGame.Web.Controllers.Other
{

    public class OtherController : ControllerBase

    {

        [HttpGet]
        [Route("Feedback")]
        public IActionResult SubmitFeedback(FeedbackRequest request)
        {
            Console.WriteLine(request);
            return new OkResult();
        }

    }
}
