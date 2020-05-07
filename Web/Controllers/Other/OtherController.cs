using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV;
using RoystonGame.Web.DataModels.Requests.Other;
using RoystonGame.Web.Helpers.Extensions;


namespace RoystonGame.Web.Controllers.Other { 

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
