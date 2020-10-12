using Microsoft.AspNetCore.Mvc;
using Backend.GameInfrastructure;
using Common.Code.Validation;

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
        [Route("Delete")]
        public IActionResult ResetUser(string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            if (!Sanitize.SanitizeString(id, out string error, Constants.RegexStrings.UserId))
            {
                return BadRequest(error);
            }
            GameManager.UnregisterUser(id);
            return new OkResult();
        }
    }
}
