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
    public class AdminFetchController : ControllerBase
    {
        private readonly ILogger<CurrentContentController> _logger;

        public AdminFetchController(ILogger<CurrentContentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            AdminFetchResponse response = new AdminFetchResponse
            {
                ActiveLobbies = GameManager.GetLobbies()
                    .Select(lobby =>
                        new AdminFetchResponse.Lobby
                        {
                            LobbyCode = lobby.LobbyCode,
                            LobbyId = lobby.LobbyId,
                            LobbyOwner = lobby.Owner.DisplayName
                        })
                    .ToList(),
            };
            return new JsonResult("Helllllooooo");
        }
    }
}
