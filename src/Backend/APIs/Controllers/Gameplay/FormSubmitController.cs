using Microsoft.AspNetCore.Mvc;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.APIs.DataModels.Enums;
using Common.DataModels.Requests;
using System;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Common.Code.Validation;

namespace Backend.APIs.Controllers
{
    [ApiController]
    [Route("api/v1/Game/[controller]")]
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
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            
            if (!Sanitize.SanitizeString(id, out string error, Common.DataModels.Constants.RegexStrings.UserId, 50,50))
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
            bool success = false;
            try
            {
                lock (user.LockObject)
                {
                    // If user form input was valid, handle it, else return the error.
                    if (user.UserState.CleanUserFormInput(user, ref formData, out error) == UserState.CleanUserFormInputResult.Valid)
                    {
                        success = user.UserState.HandleUserFormInput(user, formData, out error);
                    }
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
