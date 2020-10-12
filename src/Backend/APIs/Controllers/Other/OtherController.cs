using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.DataModels.Requests.Other;


namespace Backend.APIs.Controllers.Other
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
        [Route("api/v1/Feedback/Submit")]
        public IActionResult SubmitFeedback(FeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

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
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            if (!Regex.IsMatch(id, "^[0-9a-zA-Z]+$"))
            {
                return new BadRequestResult();
            }

            var file = Path.Combine(Env.ContentRootPath, ".well-known", "acme-challenge", id);
            return PhysicalFile(file, "text/plain");
        }
    }
}
