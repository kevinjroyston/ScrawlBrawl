using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoystonGame.TV;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Linq;
using System.Net;

using static System.FormattableString;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Admins")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<CurrentContentController> _logger;

        public AdminController(ILogger<CurrentContentController> logger)
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
                            LobbyId = lobby?.LobbyId,
                            LobbyOwner = lobby?.Owner?.UserId,
                            ActiveDuration = DateTime.Now.Subtract(lobby?.CreationTime ?? DateTime.Now)
                        })
                    .ToList(),
                ActiveUsers = GameManager.GetUsers()
                    .Select(user =>
                        new AdminFetchResponse.User
                        {
                            UserIP = user?.IP?.ToString(),
                            DisplayName = user?.DisplayName,
                            LobbyId = user?.LobbyId,
                            ActiveDuration = DateTime.Now.Subtract(user?.CreationTime ?? DateTime.Now),
                        })
                    .ToList(),
            };
            return new JsonResult(response);
        }

        [HttpPost]
        [Route("/Delete")]
        public IActionResult DeleteEntities(AdministrativeActionRequest input)
        {
            int deletedEntries = 0;
            Exception lastException = null;
            foreach (string userIP in input.Users)
            {
                try
                {
                    if (IPAddress.TryParse(userIP, out IPAddress userIPAddress))
                    {
                        GameManager.UnregisterUser(userIPAddress);
                        deletedEntries++;
                    }
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }
            foreach (string lobbyId in input.Lobbies)
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
        }
    }
}
