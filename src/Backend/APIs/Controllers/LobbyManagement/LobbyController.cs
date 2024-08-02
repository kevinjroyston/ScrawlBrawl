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
        public IActionResult CreateAndJoinLobby([FromBody] CreateAndJoinLobbyRequest request, [FromQuery(Name = "Id")] string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            AuthenticatedUser authUser = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(id));
            User user = GameManager.MapIdentifierToUser(id, out bool newUser);
            if ((user == null) || (authUser==null))
            {
                return new BadRequestResult();
            }

            if (authUser.OwnedLobby!=null)
            {
                return new OkResult();
            }
            lock (authUser.Lock){
                lock (user.LockObject) {
                    if (authUser.OwnedLobby != null)
                    {
                        return new OkResult();
                    }

                    (bool createSuccess, string createResult) = internalCreateLobby(id, authUser);
                    if (!createSuccess)
                    {
                        return StatusCode(500, createResult);
                    }
                    string createdLobbyId = createResult;
                    var joinLobbyRequest = new JoinLobbyRequest()
                    {
                        DisplayName = request.DisplayName,
                        LobbyId = createdLobbyId,
                        SelfPortrait = request.SelfPortrait
                    };

                    (bool, string) joinLobbyResponse;
                    try {
                        joinLobbyResponse = InternalJoinLobby(joinLobbyRequest, id, user);
                    }
                    catch
                    {
                        joinLobbyResponse = (false,"Internal Error");
                    }

                    if (!joinLobbyResponse.Item1)
                    {  // we created a lobby, but it wouldn't let us join, so delete the lobby
                        GameManager.DeleteLobby(createdLobbyId); 
                        authUser.OwnedLobby = null;
                        return StatusCode(400, joinLobbyResponse.Item2);
                    }
                    return new OkResult();
                } 
            }

        }

        [HttpGet]
        [Route("Create")]
#if !DEBUG
 //   [Authorize(Policy = "LobbyManagement")]
#endif
        public IActionResult CreateLobby([FromQuery(Name = "Id")] string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            AuthenticatedUser authUser = GameManager.GetAuthenticatedUser(this.HttpContext.User.GetUserId(id));

            lock (authUser.Lock)
            {
                (bool success, string result) = internalCreateLobby(id, authUser);
                if (!success)
                {
                    return StatusCode(500, result);
                }
                // Browser expects the response body to be parsable as a JSON object, so do not return the lobbyId as a string.
                // Front end will call GetLobby to get the lobbyId.  If you want to return it here, put it in a JSON object.
                return new OkResult();  
            }
        }

        [HttpPost]
        [Route("Join")]
        public IActionResult JoinLobby([FromBody] JoinLobbyRequest request, [FromQuery(Name = "Id")] string id)
        {
            if (!ModelState.IsValid || request == null)
            {
                return new BadRequestResult();
            }

            User user = GameManager.MapIdentifierToUser(id, out bool newUser);

            lock (user.LockObject)
            {
                (bool success, string error) = InternalJoinLobby(request, id, user);
                if (!success)
                {
                    return StatusCode(400, error);
                }
                return new OkResult();
            }
        }

        private (bool, string) InternalJoinLobby(JoinLobbyRequest request, string id, User user)
        {
            request.LobbyId = request.LobbyId?.Trim() ?? String.Empty;
            if (!Sanitize.SanitizeString(request.DisplayName, minLength:1, maxLength:30, error: out string _))
            {
                return (false, "DisplayName invalid.");
            }

            if (!Sanitize.SanitizeString(request.LobbyId, minLength: 1, maxLength: 10, error: out string _))
            {
                return (false, "LobbyId invalid.");
            }

            lock (user.LockObject)
            {
                if (user.Lobby != null && user.LobbyId.Equals(request.LobbyId, StringComparison.InvariantCultureIgnoreCase))
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

        private (bool, string) internalCreateLobby(string id, AuthenticatedUser user)
        {
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

                if (user.OwnedLobby.IsGameOrTutorialInProgress())
                {
                    return new AcceptedResult();
                }
                lock (user.OwnedLobby.UserJoinLock)
                {
                    if (user.OwnedLobby == null) {

                        return StatusCode(404, "Lobby no longer exists.");
                    }

                    if (user.OwnedLobby.IsGameOrTutorialInProgress())
                    {
                        return new AcceptedResult();
                    }

                    try
                    {
                        if (!user.OwnedLobby.StartGame(standardOptions, out string error))
                        {
                            return StatusCode(400, error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                        // Something went wrong, delete lobby and throw.
                        GameManager.DeleteLobby(user.OwnedLobby);
                        user.OwnedLobby = null;
                        throw;
                    }
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
