using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

using static System.FormattableString;
using RoystonGame.TV.Extensions;
using Microsoft.Extensions.Logging;
using RoystonGame.Web.Helpers.Telemetry;

namespace RoystonGame.TV
{
    public class GameManager
    {
        private ILogger<GameManager> Logger { get; set; }
        private RgEventSource EventSource { get; set; }
        #region User and Object trackers
        private ConcurrentDictionary<string, User> Users { get; } = new ConcurrentDictionary<string, User>();
        private ConcurrentDictionary<string, AuthenticatedUser> AuthenticatedUsers { get; } = new ConcurrentDictionary<string, AuthenticatedUser>();
        private ConcurrentDictionary<string, Lobby> LobbyIdToLobby { get; } = new ConcurrentDictionary<string, Lobby>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Hacky fix for passing this info to GameNotifier thread.
        /// </summary>
        public ConcurrentBag<string> AbandonedLobbyIds { get; } = new ConcurrentBag<string>();
        #endregion

        #region States
        /// <summary>
        /// States track all of the users that enter it. In order to aid in garbage collection, create a new state instance for each user.
        /// </summary>
        /// <returns>A new user registration user state.</returns>
        public UserState CreateUserRegistrationUserState()
        {
            UserState toReturn = new SimplePromptUserState(
                promptGenerator: UserNamePrompt,
                // Outlet won't be called until the below returns true
                formSubmitHandler: (User user, UserFormSubmission userInput) =>
                {
                    return RegisterUser(userInput.SubForms[1].ShortAnswer, user, userInput.SubForms[0].ShortAnswer, userInput.SubForms[2].Drawing);
                });

            toReturn.Transition((User user)=> GetLobby(user.LobbyId));
            return toReturn;
        }
        public static UserPrompt UserNamePrompt(User user) => new UserPrompt()
        {
            Title = "join a game:",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "nickname",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "lobby code",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "self portrait",
                    Drawing = new DrawingPromptMetadata()
                }
            },
            SubmitButton = true,
        };
        #endregion

        public GameManager(ILogger<GameManager> logger, RgEventSource eventSource)
        {
            Logger = logger;
            EventSource = eventSource;
        }

        public void ReportGameError(ErrorType type, string lobbyId, User user = null, Exception error = null)
        {
            Logger.LogError(EventIds.Error, error, $"{type},{lobbyId},{user.Identifier},{user.UserId}");
            EventSource.GameErrorCounter.WriteMetric(1);
            // Log error to console (TODO redirect to file on release builds).
            Debug.WriteLine(Invariant($"ERROR ERROR ERROR ERROR ERROR ~~~~~~~~~~~~~~~~~~~~~~~~~ {error}"));
            Debug.Assert(false, error.ToString());
            if (!string.IsNullOrWhiteSpace(lobbyId))
            {
                DeleteLobby(lobbyId);
            }

            if (user != null)
            {
                UnregisterUser(user);
            }
        }

        public User MapIdentifierToUser(string identifier, out bool newUser)
        {
            newUser = false;
            if (identifier.Length < 30)
            {
                return null;
            }

            try
            {
                if (Users.ContainsKey(identifier))
                {
                    return Users[identifier];
                }

                User user = new User(identifier);
                if (Users.TryAdd(identifier, user))
                {
                    CreateUserRegistrationUserState().Inlet(user, UserStateResult.Success, null);
                    newUser = true;
                }
                else
                {
                    return null;
                }
                return user;
            }
            catch (Exception e)
            {
                Logger.LogWarning(exception: e, $"Error thrown when looking up user: '{identifier}'");
                Console.Error.WriteLine(e);
                Debug.Assert(false, Invariant($"Error on user creation: {e}"));
                newUser = false;
                return null;
            }
        }


        public AuthenticatedUser GetAuthenticatedUser(string userId)
        {
            try
            {
                if (!AuthenticatedUsers.ContainsKey(userId))
                {
                    if (!AuthenticatedUsers.TryAdd(userId, new AuthenticatedUser()
                    {
                        UserId = userId
                    }))
                    {
                        return null;
                    }
                }

                return AuthenticatedUsers[userId];
            }
            catch (Exception e)
            {
                Logger.LogWarning(exception: e, message: $"Error creating authenticated user object: '{userId}'");
                Console.Error.WriteLine(e);
                Debug.Assert(false, Invariant($"Error on create authenticated user: {e}"));
                return null;
            }
        }

        #region Lobby Management
        public bool RegisterLobby(Lobby lobby)
        {
            if (lobby == null)
            {
                return false;
            }
            EventSource.LobbyStartCounter.WriteMetric(1);
            return LobbyIdToLobby.TryAdd(lobby.LobbyId, lobby);
        }
        public void DeleteLobby(Lobby lobby)
        {
            if (lobby == null)
            {
                return;
            }
            DeleteLobby(lobby.LobbyId);
        }
        public void DeleteLobby(string lobbyId)
        {
            EventSource.LobbyEndCounter.WriteMetric(1);
            Lobby lobby = GetLobby(lobbyId);
            LobbyIdToLobby.TryRemove(lobbyId, out Lobby _);

            lobby?.UnregisterAllUsers();
            if (lobby?.Owner?.OwnedLobby != null)
            {
                lobby.Owner.OwnedLobby = null;
            }
            AbandonedLobbyIds.Add(lobbyId);
        }

        public List<Lobby> GetLobbies()
        {
            return LobbyIdToLobby.Values.ToList();
        }

        public Lobby GetLobby(string lobbyId)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
            {
                return null;
            }
            return LobbyIdToLobby.GetValueOrDefault(lobbyId);
        }
        #endregion

        #region User Management
        public List<User> GetUsers()
        {
            return Users.Values.ToList();
        }
        public void UnregisterUser(User user)
        {
            if (user == null)
            {
                return;
            }
            UnregisterUser(user.Identifier);
        }
        public void UnregisterUser(string userIdentifier)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                return;
            }
            Users.TryRemove(userIdentifier, out User _);
        }

        private (bool, string) RegisterUser(string lobbyId, User user, string displayName, string selfPortrait)
        {
            string userIdentifier = Users.FirstOrDefault((kvp) => kvp.Value == user).Key;
            if (userIdentifier == null)
            {
                return (false, "Can't register unknown user. Refresh the page.");
            }
            if (string.IsNullOrWhiteSpace(lobbyId))
            {
                return (false, "Invalid Lobby Code.");
            }

            user.DisplayName = displayName;
            user.SelfPortrait = selfPortrait;

            if (!LobbyIdToLobby.ContainsKey(lobbyId))
            {
                return (false, "Lobby not found.");
            }

            if (!string.IsNullOrWhiteSpace(user.LobbyId))
            {
                // If the user is supposedly already registered to a lobby, try adding them again.
                if (!LobbyIdToLobby[user.LobbyId].TryAddUser(user, out string errorMsg))
                {
                    // If that didn't work, clear them from this lobby.
                    user.LobbyId = null;
                    return (false, errorMsg);
                }
            }
            else
            {
                if (!LobbyIdToLobby[lobbyId].TryAddUser(user, out string errorMsg))
                {
                    return (false, errorMsg);
                }
            }
            return (true, string.Empty);
        }
        #endregion
    }
}
