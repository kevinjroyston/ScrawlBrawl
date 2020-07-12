using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.Helpers;
using RoystonGame.Web.Helpers.Validation;
using RoystonGame.Web.Helpers.Extensions;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using static System.FormattableString;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FormSubmitController : ControllerBase
    {
        public FormSubmitController(GameManager gameManager)
        {
            this.GameManager = gameManager;
        }

        private GameManager GameManager { get; set; }

        [HttpPost]
        public IActionResult Post(
            [FromBody] UserFormSubmission formData,
            string id)
        {
            if (!Sanitize.SanitizeString(id, out string error, "^([0-9A-Fa-f]){50}$"))
            {
                return BadRequest(error);
            }

            User user = GameManager.MapIdentifierToUser(id, out bool newUser);

            if (user != null)
            {
                user.LastHeardFrom = DateTime.UtcNow;
            }

            if (user?.UserState == null || newUser)
            {
                return BadRequest("Error finding user object, try again.");
            }

            if (!Sanitize.SanitizeAllStrings(formData, out error))
            {
                return BadRequest(error);
            }

            // Make sure HandleUserFormInput is never called concurrently for the same user.
            bool success;
            try
            {
                lock (user.LockObject)
                {
                    success = user.UserState.HandleUserFormInput(user, formData, out error);
                }
            }
            catch (Exception e)
            {
                error = "An unexpected error occurred, refresh and try again :(";
                success = false;

                // Let GameManager know so it can determine whether or not to abandon the lobby.
                GameManager.ReportGameError(ErrorType.UserSubmit, user?.LobbyId, user, e);
            }
            return success ? new OkResult() : BadRequest(error);
        }

        private static IActionResult BadRequest(string err)
        {
            return new BadRequestObjectResult(err);
        }
    }
}
