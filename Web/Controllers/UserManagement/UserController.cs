using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV;
using RoystonGame.Web.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            GameManager.UnregisterUser(this.HttpContext.Connection.RemoteIpAddress);
            return new OkResult();
        }
    }
}
