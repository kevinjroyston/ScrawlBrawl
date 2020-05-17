using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV;
using RoystonGame.Web.Helpers.Extensions;

namespace RoystonGame.Web.Controllers.UserManagement
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Route("Reset")]
        public IActionResult ResetUser()
        {
            GameManager.UnregisterUser(TV.DataModels.Users.User.GetUserIdentifier(this.HttpContext.Connection.RemoteIpAddress, Request.GetUserAgent()));
            return new OkResult();
        }
    }
}
