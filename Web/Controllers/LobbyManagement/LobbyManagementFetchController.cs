using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoystonGame.TV;
using RoystonGame.Web.DataModels.Responses;
using System.Linq;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Admins")]
    public class LobbyManagementFetchController : ControllerBase
    {
        private readonly ILogger<CurrentContentController> _logger;

        public LobbyManagementFetchController(ILogger<CurrentContentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult("Helllllooooo");
        }
    }
}
