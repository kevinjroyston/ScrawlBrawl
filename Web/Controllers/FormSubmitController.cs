using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            User user = GameManager.MapIPToUser(this.HttpContext.Connection.RemoteIpAddress);
            if (user?.UserState == null)
            {
                return new BadRequestResult();
            }

            // Make sure HandleUserFormInput is never called concurrently for the same user.
            bool success;
            try
            {
                lock (user.LockObject)
                {
                    success = user.UserState.HandleUserFormInput(user, formData);
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                success = false;
            }
            return success ? new OkResult() : (IActionResult)new BadRequestResult();
        }
    }
}
