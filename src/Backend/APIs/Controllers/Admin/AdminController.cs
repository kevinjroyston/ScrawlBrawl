using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Configuration;

namespace Backend.APIs.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<CurrentContentController> _logger;
        private readonly IConfiguration Config;

        public AdminController(ILogger<CurrentContentController> logger, IConfiguration config)
        {
            _logger = logger;
            Config = config;
        }
        /*
        [HttpGet]
        [Route("Get")]
        public IActionResult Get()
        {
            this.HttpContext.User.VerifyHasGroupRole(Config);
            AdminFetchResponse response = new AdminFetchResponse
            {
                ActiveLobbies = GameManager.GetLobbies()
                    .Select(lobby =>
                        new AdminFetchResponse.Lobby
                        {
                            LobbyId = lobby?.LobbyId,
                            LobbyOwner = lobby?.Owner?.UserId,
                            ActiveDuration = DateTime.Now.Subtract(lobby?.CreationTime ?? DateTime.Now)
                        })
                    .ToList(),
                ActiveUsers = GameManager.GetUsers()
                    .Select(user =>
                        new AdminFetchResponse.User
                        {
                            UserIdentifier = user?.Identifier?.ToString(),
                            DisplayName = user?.DisplayName,
                            LobbyId = user?.LobbyId,
                            ActiveDuration = DateTime.Now.Subtract(user?.CreationTime ?? DateTime.Now),
                        })
                    .ToList(),
            };
            return new JsonResult(response);
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult DeleteEntities(AdministrativeActionRequest input)
        {
            this.HttpContext.User.VerifyHasGroupRole(Config);
            int deletedEntries = 0;
            Exception lastException = null;
            foreach (string userIdentifier in input?.Users ?? new List<string>())
            {
                try
                {
                    GameManager.UnregisterUser(userIdentifier);
                    deletedEntries++;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }

            foreach (string lobbyId in input?.Lobbies ?? new List<string>())
            {
                try
                {
                    GameManager.DeleteLobby(lobbyId);
                    deletedEntries++;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }
            return new JsonResult(Invariant($"Deleted ({deletedEntries}) entries. Last exception: '{lastException}'"));
        }*/
    }
}
