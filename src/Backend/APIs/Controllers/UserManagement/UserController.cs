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
            GameManager.UnregisterUser(id);
            return new OkResult();
        }
    }
}
