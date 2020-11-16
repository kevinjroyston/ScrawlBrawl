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
            if (!Sanitize.SanitizeString(id, out string error, Constants.RegexStrings.UserId,50,50))
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
                    if (!user.UserState.UserRequestingCurrentPrompt(user).SubmitButton)
                    {
                        return BadRequest("You shouldn't be able to time out on this prompt.");
                    }

                    // Cleans invalid fields and replaces with null.
                    if (user.UserState.CleanUserFormInput(user, ref formData, out error) != UserState.CleanUserFormInputResult.Invalid)
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
