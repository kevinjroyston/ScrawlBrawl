using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV;
using RoystonGame.Web.Helpers.Extensions;
using RoystonGame.Web.Helpers.Validation;

namespace RoystonGame.Web.Controllers.UserManagement
{
    [ApiController]
    [Route("[controller]")]
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

            if (!Sanitize.SanitizeString(id, out string error, "^([0-9A-Fa-f]){50}$"))
            {
                return BadRequest(error);
            }
            GameManager.UnregisterUser(id);
            return new OkResult();
        }
    }
}
