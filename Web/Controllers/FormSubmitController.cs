using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoystonGame.TV;
using RoystonGame.TV.DataModels;
using RoystonGame.Web.DataModels.Requests;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FormSubmitController : ControllerBase
    {
        private readonly ILogger<FormSubmitController> _logger;

        public FormSubmitController(ILogger<FormSubmitController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserFormSubmission formData)
        {
            if (!GameManager.Singleton.GameStarted)
            {
                return new BadRequestResult();
            }
            User user = GameManager.MapIPToUser(this.HttpContext.Connection.RemoteIpAddress);
            if (user == null)
            {
                return new BadRequestResult();
            }
            bool success = user.UserState.HandleUserFormInput(user, formData);
            return success ? new OkResult() : (IActionResult)new BadRequestResult();
        }
    }
}
