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
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Common.Code.Validation;
#if !DEBUG
//using Microsoft.AspNetCore.Authorization;
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
//    [Authorize(Policy = "LobbyManagement")]
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
 //   [Authorize(Policy = "LobbyManagement")]
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

            if (!GameManager.RegisterLobby(newLobby))
            {
                return StatusCode(500, "Failed to create lobby, try again");
            }

            user.OwnedLobby = newLobby;
            return Ok(lobbyId);
        }

        [HttpPost]
        [Route("Join")]
        public IActionResult JoinLobby([FromBody] JoinLobbyRequest request, [FromQuery(Name = "Id")] string id)
        {
            if (!ModelState.IsValid || request == null)
            {
                return new BadRequestResult();
            }

            if (Sanitize.SanitizeString(request.DisplayName, out string _))
            {
                return new BadRequestResult();
            }

            if (Sanitize.SanitizeString(request.LobbyId, out string _))
            {
                return new BadRequestResult();
            }

            User user = GameManager.MapIdentifierToUser(id, out bool newUser);
            lock (user.LockObject)
            {
                // If the user is currently in a different lobby, unregister the user and create a new one.
                // If the user is already in this lobby calling register user should no-op.
                if (user.Lobby != null && !user.LobbyId.Equals(request.LobbyId))
                {
                    GameManager.UnregisterUser(user);
                    user = GameManager.MapIdentifierToUser(id, out bool _);
                }

                // Separate lock as the first user object may have been unregistered. Nested lock because of race conditions fetching current user state.
                // Locks are re-entrant so locking the same object twice from one thread poses no risks.
                lock (user.LockObject)
                {
                    // Race condition.
                    if (user.Lobby != null && !user.LobbyId.Equals(request.LobbyId))
                    {
                        return StatusCode(400, "Try Again.");
                    }

                    (bool, string) result = GameManager.RegisterUser(user, request);
                    if (!result.Item1)
                    {
                        return StatusCode(400, result.Item2);
                    }

                    if (user?.Lobby == null)
                    {
                        return StatusCode(400, "Unknown error occurred while joining Lobby.");
                    }

                    return new OkResult();
                }
            }
        }

        [HttpGet]
        [Route("Delete")]
#if !DEBUG
 //   [Authorize(Policy = "LobbyManagement")]
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
 //   [Authorize(Policy = "LobbyManagement")]
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
 //   [Authorize(Policy = "LobbyManagement")]
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
