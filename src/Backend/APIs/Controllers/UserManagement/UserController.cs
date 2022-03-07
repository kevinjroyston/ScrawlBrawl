using Microsoft.AspNetCore.Mvc;
using Backend.GameInfrastructure;
using Common.Code.Validation;
using Backend.GameInfrastructure.DataModels.Users;
using Newtonsoft.Json;
using System;

namespace Backend.APIs.Controllers.UserManagement
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        public UserController(GameManager gameManager)
        {
            this.GameManager = gameManager;
        }

        private GameManager GameManager { get; set; }

        [HttpGet]
        [Route("Get")]
        public IActionResult GetUser(string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            User user = GameManager.MapIdentifierToUser(id, out bool newUser);

            return Content(JsonConvert.SerializeObject(new User.Response(user)));
        }

        [HttpGet]
        [Route("Delete")]
        public IActionResult ResetUser(string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            if (!Sanitize.SanitizeString(id, out string error, Common.DataModels.Constants.RegexStrings.UserId))
            {
                return BadRequest(error);
            }

            // Find the user
            User user = GameManager.MapIdentifierToUser(id, out bool newUser);

            lock (user.LockObject)
            {
                // don't let this user be found ever again.
                GameManager.UnregisterUser(id);

                // If in a lobby and it has not started, leave it gracefully
                // Leave the user object alone as it may be important to the lobby that it is untouched.
                if (user?.Lobby != null)
                {
                    // Lets the lobby know we are trying to leave. It may keep the
                    // object around if the game has already started.
                    user.Lobby.TryLeaveLobbyGracefully(user);
                }

                // Set the last ping time to old enough that the user is considered disconnected.
                user.LastPingTime = DateTime.UtcNow.Subtract(Constants.UserDisconnectTimer);
            }

            return new OkResult();
        }
        [HttpGet]
        [Route("GetLobby")]
        public IActionResult GetLobby(string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            if (!Sanitize.SanitizeString(id, out string error, Common.DataModels.Constants.RegexStrings.UserId))
            {
                return BadRequest(error);
            }

            User user = GameManager.MapIdentifierToUser(id, out bool newUser);

            if (user == null)
            {
                return StatusCode(500, "Something went wrong finding that user, try again");
            }

            if (user.Lobby != null)
            {
                return new OkObjectResult(user.Lobby.GenerateLobbyMetadataResponseObject());
            }

             return StatusCode(404, "Lobby doesn't exist.");
        }

    }
}
