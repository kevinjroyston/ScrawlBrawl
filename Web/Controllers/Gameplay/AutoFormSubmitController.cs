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
using RoystonGame.TV.DataModels.States.UserStates;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutoFormSubmitController : ControllerBase
    {
        public AutoFormSubmitController(GameManager gameManager)
        {
            this.GameManager = gameManager;
        }

        private GameManager GameManager { get; set; }

        [HttpPost]
        public IActionResult Post(
            [FromBody] UserFormSubmission formData,
            string id)
        {
            if (!Sanitize.SanitizeString(id, out string error, "^([0-9A-Fa-f]){50}$",50,50))
            {
                return BadRequest(error);
            }

            User user = GameManager.MapIdentifierToUser(id, out bool newUser);

            if (user != null)
            {
                user.LastSubmitTime = DateTime.UtcNow;
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
            try
            {
                lock (user.LockObject)
                {
                    // Cleans invalid fields and replaces with null.
                    if(user.UserState.CleanUserFormInput(user, ref formData, out error) != UserState.CleanUserFormInputResult.Invalid)
                    {
                        // Handles partial/null inputs.
                        user.UserState.HandleUserTimeout(user, formData);
                    }
                }
            }
            catch (Exception e)
            {
                error = "An unexpected error occurred, refresh and try again :(";

                // Let GameManager know so it can determine whether or not to abandon the lobby.
                GameManager.ReportGameError(ErrorType.UserSubmit, user?.LobbyId, user, e);

                return new BadRequestObjectResult(error);
            }
            return new OkResult();
        }
    }
}
