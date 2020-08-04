using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoystonGame.Web.DataModels.Requests.Other;


namespace RoystonGame.Web.Controllers.Other
{

    public class OtherController : ControllerBase

    {
        public OtherController (ILogger<OtherController> logger, IWebHostEnvironment env)
        {
            Logger = logger;
            Env = env;
        }
        private ILogger<OtherController> Logger { get; set; }
        private IWebHostEnvironment Env { get; set; }

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

        [HttpGet]
        [Route(".well-known/acme-challenge/{id}")]
        public ActionResult LetsEncrypt(string id)
        {
            var file = Path.Combine(Env.ContentRootPath, ".well-known", "acme-challenge", id);
            return PhysicalFile(file, "text/plain");
        }

        // Temporary
        [HttpGet]
        [Route(".well-known/{filename}")]
        public ActionResult WellKnown(string filename)
        {
            var file = Path.Combine(Env.ContentRootPath, ".well-known", filename);
            return PhysicalFile(file, "text/plain");
        }
    }
}
