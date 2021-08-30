using Microsoft.AspNetCore.Mvc;
using Backend.GameInfrastructure;
using Common.Code.Validation;
using Backend.GameInfrastructure.DataModels.Users;
using Newtonsoft.Json;

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

            // If in a lobby and it has not started, leave it gracefully
            User user = GameManager.MapIdentifierToUser(id, out bool newUser);
            if (!newUser && user?.Lobby != null)
            {
                // Lets the lobby know we are trying to leave. It may keep the
                // object around if the game has already started.
                user.Lobby.TryLeaveLobbyGracefully(user);
            }
            // Leave the user object alone as it may be important to the lobby that it is untouched.
            GameManager.UnregisterUser(id);
            return new OkResult();
        }
    }
}
