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
using Common.DataModels.Responses.LobbyManagement;
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

            lock (user.Lock)
            {
                if (user.OwnedLobby != null)
                {
                    return new OkObjectResult(user.OwnedLobby.GenerateLobbyMetadataResponseObject());
                }

                return StatusCode(404, "Lobby doesn't exist.");
            }
        }

        [HttpPost]
        [Route("CreateAndJoin")]
#if !DEBUG
 //   [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult CreateAndJoinLobby([FromBody] JoinLobbyRequest request, [FromQuery(Name = "Id")] string id)
        {
            request.LobbyId = "temp"; // make LobbyId optional to the modelstate.
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }
            (bool success, string result) = internalCreateLobby(id);
            if (!success)
            {
                return StatusCode(500, result);
            }
            request.LobbyId = result;
            return JoinLobby(request, id);
        }

        [HttpPost]
        [Route("Create")]
#if !DEBUG
 //   [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult CreateLobby([FromQuery(Name = "Id")] string id)
        {
            (bool success, string result) = internalCreateLobby(id);
            if (!success)
            {
                return StatusCode(500, result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("Join")]
        public IActionResult JoinLobby([FromBody] JoinLobbyRequest request, [FromQuery(Name = "Id")] string id)
        {
            if (!ModelState.IsValid || request == null)
            {
                return new BadRequestResult();
            }

            (bool success, string error) = InternalJoinLobby(request, id);
            if (!success)
            {
                return StatusCode(400, error);
            }
            return new OkResult();
        }

        private (bool, string) InternalJoinLobby(JoinLobbyRequest request, string id)
        {
            if (!Sanitize.SanitizeString(request.DisplayName, minLength:1, maxLength:30, error: out string _))
            {
                return (false, "DisplayName invalid.");
            }

            if (!Sanitize.SanitizeString(request.LobbyId, minLength: 1, maxLength: 10, error: out string _))
            {
                return (false, "LobbyId invalid.");
            }

            User user = GameManager.MapIdentifierToUser(id, out bool newUser);

            lock (user.LockObject)
            {
                if (user.Lobby != null & user.LobbyId == request.LobbyId)
                {
                    // Another thread beat us to it. Return generic error.
                    return (false, "Try Again.");
                }

                if (user.Lobby != null)
                {
                    // Either another thread beat us to it (with multiple lobbies). Or we got the user stuck in a bad state.
                    // Unregister the user and return failure.
                    GameManager.UnregisterUser(user);
                    return (false, "Try Again.");
                }

                (bool, string) result = GameManager.RegisterUser(user, request);
                if (!result.Item1) // Check success.
                {
                    return (false, result.Item2); // Return error message.
                }

                if (user?.Lobby == null) // Confirm user is now in a lobby.
                {
                    return (false, "Unknown error occurred while joining Lobby.");
                }

                return (true, string.Empty);
            }
        }

        private (bool, string) internalCreateLobby(string id)
        {
            AuthenticatedUser user = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(id));

            if (user == null)
            {
                return (false, "Something went wrong finding that user, try again");
            }

            if (user.OwnedLobby != null)
            {
                return (true, user.OwnedLobby.LobbyId);
            }

            lock (user.Lock)
            {
                if (user.OwnedLobby != null)
                {
                    return (true, user.OwnedLobby.LobbyId);
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
                    return (false, "Failed to create lobby, try again");
                }

                user.OwnedLobby = newLobby;
                return (true,lobbyId);
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

            lock (user.Lock)
            {
                if (user.OwnedLobby == null)
                {
                    return new AcceptedResult();
                }

                GameManager.DeleteLobby(user.OwnedLobby);
                user.OwnedLobby = null;
                return new AcceptedResult();
            }
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

            lock (user.Lock)
            {
                if (user.OwnedLobby == null)
                {
                    return StatusCode(404, "Lobby doesn't exist.");
                }

                if (!user.OwnedLobby.ConfigureLobby(request, out string error, out ConfigureLobbyResponse response))
                {
                    return StatusCode(400, error);
                }

                return new OkObjectResult(response);
            }
        }

        [HttpPost]
        [Route("Start")]
#if !DEBUG
 //   [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult StartLobby([FromQuery(Name = "Id")]string testHookId, [FromBody] StandardGameModeOptions standardOptions)
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

            lock (user.Lock)
            {
                if (user.OwnedLobby == null)
                {
                    return StatusCode(404, "Lobby doesn't exist.");
                }

                if (!user.OwnedLobby.StartGame(standardOptions, out string error))
                {
                    return StatusCode(400, error);
                }

                return new AcceptedResult();
            }
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
