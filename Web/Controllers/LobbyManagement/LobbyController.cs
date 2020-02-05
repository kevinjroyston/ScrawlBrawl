using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.Helpers.Extensions;
using System;

using static System.FormattableString;

namespace RoystonGame.Web.Controllers.LobbyManagement
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Users")]
    public class LobbyController : ControllerBase
    {
        [HttpGet]
        [Route("GetOrCreate")]
        public IActionResult GetOrCreateLobby()
        {
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());
            if (user == null)
            {
                // should not be reached
                return new BadRequestObjectResult("Something went wrong finding that user, try again");
            }

            // If the user already has a lobby made they shouldnt be entering this flow. But in case we messed up bookkeeping, lets just clean the old one up in that case.
            if (user.OwnedLobby != null)
            {
                return new OkObjectResult(new LobbyMetadataResponse(user.OwnedLobby));
            }

            string lobbyId;
            int safety = 0;
            do
            {
                // Keep making lobbyIds until a new one is found
                lobbyId = Guid.NewGuid().ToString().Substring(0, 5);
                if (safety++ > 20)
                {
                    throw new Exception("Yikes");
                }
            } while (GameManager.GetLobby(lobbyId) != null);

            Lobby newLobby = new Lobby(lobbyId, owner: user);

            if (!GameManager.RegisterLobby(newLobby))
            {
                return new BadRequestObjectResult("Something went wrong creating the lobby, try again");
            }

            user.OwnedLobby = newLobby;
            return new OkObjectResult(new LobbyMetadataResponse(user.OwnedLobby));
        }

        [HttpGet]
        [Route("Delete")]
        public IActionResult DeleteLobby()
        {
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());
            if (user == null)
            {
                // should not be reached
                return new BadRequestObjectResult("Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby == null)
            {
                return new BadRequestObjectResult("No lobby to delete!");
            }

            GameManager.DeleteLobby(user.OwnedLobby);
            return new OkResult();
        }

        [HttpPost]
        [Route("Configure")]
        public IActionResult ConfigureLobby([FromBody]ConfigureLobbyRequest request)
        {
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());
            if (user == null)
            {
                // should not be reached
                return new BadRequestObjectResult("Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby == null)
            {
                return new BadRequestObjectResult("No lobby to select game mode for!");
            }

            if (!user.OwnedLobby.ConfigureLobby(request, out string error))
            {
                return new BadRequestObjectResult(error);
            }

            return new OkResult();
        }

        [HttpGet]
        [Route("Start")]
        public IActionResult StartLobby()
        {
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());
            if (user == null)
            {
                // should not be reached
                return new BadRequestObjectResult("Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby == null)
            {
                return new BadRequestObjectResult("No lobby to start!");
            }

            if (!user.OwnedLobby.StartGame(out string error))
            {
                return new BadRequestObjectResult(error);
            }

            return new OkResult();
        }
    }
}
