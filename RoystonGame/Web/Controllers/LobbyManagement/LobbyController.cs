using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using RoystonGame.TV;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.Helpers.Extensions;
using RoystonGame.Web.Helpers.Telemetry;
using System;

namespace RoystonGame.Web.Controllers.LobbyManagement
{
    [ApiController]
    [Route("api/v1/[controller]")]
#if !DEBUG
    [Authorize(Policy = "LobbyManagement")]
#endif
    public class LobbyController : ControllerBase
    {
        public LobbyController(ILogger<LobbyController> logger, GameManager gameManager)
        {
            this.GameManager = gameManager;
            this.Logger = logger;
        }

        private GameManager GameManager { get; set; }
        private ILogger<LobbyController> Logger { get; set; }

        [HttpGet]
        [Route("Get")]
        public IActionResult GetLobby()
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());

            if (user == null)
            {
                return StatusCode(500, "Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby != null)
            {
                return new OkObjectResult(new LobbyMetadataResponse(user.OwnedLobby));
            }

            return StatusCode(404, "Lobby doesn't exist.");
        }

        [HttpPost]
        [Route("Create")]
        public IActionResult CreateLobby()
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());

            if (user == null)
            {
                return StatusCode(500, "Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby != null)
            {
                return new OkResult();
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

            Lobby newLobby = new Lobby(lobbyId, owner: user, gameManager: GameManager);

            if (!GameManager.RegisterLobby(newLobby))
            {
                return StatusCode(500, "Failed to create lobby, try again");
            }

            user.OwnedLobby = newLobby;
            return new OkResult();
        }

        [HttpGet]
        [Route("Delete")]
        public IActionResult DeleteLobby()
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());
            if (user == null)
            {
                return StatusCode(500, "Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby == null)
            {
                return new AcceptedResult();
            }

            GameManager.DeleteLobby(user.OwnedLobby);
            user.OwnedLobby = null;
            return new AcceptedResult();
        }

        [HttpPost]
        [Route("Configure")]
        public IActionResult ConfigureLobby([FromBody]ConfigureLobbyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());
            if (user == null)
            {
                return StatusCode(500, "Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby == null)
            {
                return StatusCode(404, "Lobby doesn't exist.");
            }

            if (!user.OwnedLobby.ConfigureLobby(request, out string error))
            {
                return StatusCode(400, error);
            }

            return new AcceptedResult();
        }

        [HttpGet]
        [Route("Start")]
        public IActionResult StartLobby()
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId());
            if (user == null)
            {
                return StatusCode(500, "Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby == null)
            {
                return StatusCode(404, "Lobby doesn't exist.");
            }

            if (!user.OwnedLobby.StartGame(out string error))
            {
                return StatusCode(400, error);
            }

            return new AcceptedResult();
        }

        [HttpGet]
        [Route("Games")]
        public IActionResult GetGames()
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            return new OkObjectResult(Lobby.GameModes);
        }
    }
}
