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
        [HttpGet]
        [Route("Delete")]
        public IActionResult ResetUser(string id)
        {
            if (!Sanitize.SanitizeString(id, out string error, "^([0-9A-Fa-f]){50}$"))
            {
                return BadRequest(error);
            }
            GameManager.UnregisterUser(id);
            return new OkResult();
        }
    }
}
