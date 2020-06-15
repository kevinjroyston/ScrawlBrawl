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

namespace RoystonGame.TV
{
    public class GameManager
    {
        #region Singleton
        private static GameManager _internalSingleton;
        public static GameManager Singleton
        {
            get
            {
                if (_internalSingleton == null)
                {
                    _internalSingleton = new GameManager();
                }
                return _internalSingleton;
            }
        }

        private GameManager()
        {
            if (_internalSingleton != null)
            {
                throw new Exception("Tried to instantiate a second instance of GameManager.cs");
            }
        }
        #endregion

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
        public static UserState CreateUserRegistrationUserState()
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

        public static void ReportGameError(ErrorType type, string lobbyId, User user = null, Exception error = null)
        {
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

        public static User MapIPToUser(IPAddress callerIP, string userAgent, string idOverride, out bool newUser)
        {
            newUser = false;
            string userIdentifier = User.GetUserIdentifier(callerIP, userAgent, idOverride);
            try
            {
                if (Singleton.Users.ContainsKey(userIdentifier))
                {
                    return Singleton.Users[userIdentifier];
                }

                User user = new User(callerIP, userAgent);
                if (Singleton.Users.TryAdd(userIdentifier, user))
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
                Console.Error.WriteLine(e);
                Debug.Assert(false, Invariant($"Error on user creation: {e}"));
                newUser = false;
                return null;
            }
        }


        public static AuthenticatedUser GetAuthenticatedUser(string userId)
        {
            try
            {
                if (!Singleton.AuthenticatedUsers.ContainsKey(userId))
                {
                    if (!Singleton.AuthenticatedUsers.TryAdd(userId, new AuthenticatedUser()
                    {
                        UserId = userId
                    }))
                    {
                        return null;
                    }
                }

                return Singleton.AuthenticatedUsers[userId];
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Debug.Assert(false, Invariant($"Error on create authenticated user: {e}"));
                return null;
            }
        }

        #region Lobby Management
        public static bool RegisterLobby(Lobby lobby)
        {
            if (lobby == null)
            {
                return false;
            }
            return Singleton.LobbyIdToLobby.TryAdd(lobby.LobbyId, lobby);
        }
        public static void DeleteLobby(Lobby lobby)
        {
            if (lobby == null)
            {
                return;
            }
            DeleteLobby(lobby.LobbyId);
        }
        public static void DeleteLobby(string lobbyId)
        {
            Lobby lobby = GetLobby(lobbyId);
            Singleton.LobbyIdToLobby.TryRemove(lobbyId, out Lobby _);

            lobby?.UnregisterAllUsers();
            if (lobby?.Owner?.OwnedLobby != null)
            {
                lobby.Owner.OwnedLobby = null;
            }
            Singleton.AbandonedLobbyIds.Add(lobbyId);
        }

        public static List<Lobby> GetLobbies()
        {
            return Singleton.LobbyIdToLobby.Values.ToList();
        }

        public static Lobby GetLobby(string lobbyId)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
            {
                return null;
            }
            return Singleton.LobbyIdToLobby.GetValueOrDefault(lobbyId);
        }
        #endregion

        #region User Management
        public static List<User> GetUsers()
        {
            return Singleton.Users.Values.ToList();
        }
        public static void UnregisterUser(User user)
        {
            if (user == null)
            {
                return;
            }
            UnregisterUser(user.Identifier);
        }
        public static void UnregisterUser(string userIdentifier)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                return;
            }
            Singleton.Users.TryRemove(userIdentifier, out User _);
        }

        private static (bool, string) RegisterUser(string lobbyId, User user, string displayName, string selfPortrait)
        {
            string userIdentifier = Singleton.Users.FirstOrDefault((kvp) => kvp.Value == user).Key;
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

            if (!Singleton.LobbyIdToLobby.ContainsKey(lobbyId))
            {
                return (false, "Lobby not found.");
            }

            if (!string.IsNullOrWhiteSpace(user.LobbyId))
            {
                // If the user is supposedly already registered to a lobby, try adding them again.
                if (!Singleton.LobbyIdToLobby[user.LobbyId].TryAddUser(user, out string errorMsg))
                {
                    // If that didn't work, clear them from this lobby.
                    user.LobbyId = null;
                    return (false, errorMsg);
                }
            }
            else
            {
                if (!Singleton.LobbyIdToLobby[lobbyId].TryAddUser(user, out string errorMsg))
                {
                    return (false, errorMsg);
                }
            }
            return (true, string.Empty);
        }
        #endregion
    }
}
