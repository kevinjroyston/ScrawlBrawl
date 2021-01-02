using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Backend.GameInfrastructure;
using Backend.APIs.DataModels;
using Common.DataModels.Requests.LobbyManagement;
using Backend.APIs.Helpers.Extensions;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Backend.GameInfrastructure.DataModels;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif

namespace Backend.APIs.Controllers.LobbyManagement
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LobbyController : ControllerBase
    {
        public LobbyController(ILogger<LobbyController> logger, GameManager gameManager, IServiceProvider serviceProvider, InMemoryConfiguration inMemoryConfiguration)
        {
            this.GameManager = gameManager;
            this.Logger = logger;
            this.ServiceProvider = serviceProvider;
            this.InMemoryConfiguration = inMemoryConfiguration;
        }

        private GameManager GameManager { get; set; }
        private ILogger<LobbyController> Logger { get; set; }
        private IServiceProvider ServiceProvider { get; set; }
        private InMemoryConfiguration InMemoryConfiguration { get; set; }


        [HttpGet]
        [Route("Get")]
#if !DEBUG
    [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult GetLobby([FromQuery(Name = "Id")]string testHookId)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(testHookId));

            if (user == null)
            {
                return StatusCode(500, "Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby != null)
            {
                return new OkObjectResult(user.OwnedLobby.GenerateLobbyMetadataResponseObject());
            }

            return StatusCode(404, "Lobby doesn't exist.");
        }

        [HttpPost]
        [Route("Create")]
#if !DEBUG
    [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult CreateLobby([FromQuery(Name = "Id")]string testHookId)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(testHookId));

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

            Lobby newLobby = ActivatorUtilities.CreateInstance<Lobby>(this.ServiceProvider, lobbyId, user);
            newLobby.InitializeAllGameStates();

            if (!GameManager.RegisterLobby(newLobby))
            {
                return StatusCode(500, "Failed to create lobby, try again");
            }

            user.OwnedLobby = newLobby;
            return new OkResult();
        }

        [HttpGet]
        [Route("Delete")]
#if !DEBUG
    [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult DeleteLobby([FromQuery(Name = "Id")]string testHookId)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(testHookId));
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
#if !DEBUG
    [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult ConfigureLobby([FromBody]ConfigureLobbyRequest request, [FromQuery(Name ="Id")]string testHookId)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(testHookId));
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
#if !DEBUG
    [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult StartLobby([FromQuery(Name = "Id")]string testHookId)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(testHookId));
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
            return new OkObjectResult(InMemoryConfiguration.GameModes.Select(gameHolder => gameHolder.GameModeMetadata));
        }
    }
}
