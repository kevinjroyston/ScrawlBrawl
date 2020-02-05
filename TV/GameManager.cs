using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.Web.DataModels.Responses;
using System.Net;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Enums;
using System.Collections.Concurrent;
using RoystonGame.Web.DataModels;

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
        private ConcurrentDictionary<IPAddress, User> Users { get; } = new ConcurrentDictionary<IPAddress, User>();
        private ConcurrentDictionary<string, AuthenticatedUser> AuthenticatedUsers { get; } = new ConcurrentDictionary<string, AuthenticatedUser>();
        private ConcurrentDictionary<string, Lobby> LobbyIdToLobby { get; } = new ConcurrentDictionary<string, Lobby>();

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
        public static UserState CreateUserRegistrationUserState () => new SimplePromptUserState(
            prompt: UserNamePrompt,
            outlet: (User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                GetLobby(user.LobbyId).Inlet(user, result, userInput);
            },
            // Outlet won't be called until the below returns true
            formSubmitListener: (User user, UserFormSubmission userInput) =>
            {
                return RegisterUser(userInput.SubForms[1].ShortAnswer, user, userInput.SubForms[0].ShortAnswer, userInput.SubForms[2].Drawing);
            });
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
            Console.Error.WriteLine(error);
            if (!string.IsNullOrWhiteSpace(lobbyId))
            {
                DeleteLobby(lobbyId);
            }

            if (user != null)
            {
                UnregisterUser(user);
            }
        }

        public static User MapIPToUser(IPAddress callerIP, out bool newUser)
        {
            newUser = false;
            try
            {
                if (Singleton.Users.ContainsKey(callerIP))
                {
                    return Singleton.Users[callerIP];
                }

                User user = new User(callerIP);
                if(Singleton.Users.TryAdd(callerIP, user))
                {
                    Singleton.CreateUserRegistrationUserState().Inlet(user, UserStateResult.Success, null);
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
                    if(!Singleton.AuthenticatedUsers.TryAdd(userId, new AuthenticatedUser()
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
                return null;
            }
        }

        #region Lobby Management
        public static bool RegisterLobby(Lobby lobby)
        {
            return Singleton.LobbyIdToLobby.TryAdd(lobby.LobbyId, lobby);
        }
        public static void DeleteLobby(Lobby lobby)
        {
            DeleteLobby(lobby.LobbyId);
        }
        public static void DeleteLobby(string lobbyId)
        {
            Lobby lobby = GetLobby(lobbyId);
            Singleton.LobbyIdToLobby.TryRemove(lobbyId, out Lobby _);

            lobby?.UnregisterAllUsers();
            if (lobby?.Owner?.OwnedLobby != null) {
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
            UnregisterUser(user.IP);
        }
        public static void UnregisterUser(IPAddress callerIP)
        {
            Singleton.Users.TryRemove(callerIP, out User _);
        }

        private static (bool, string) RegisterUser(string lobbyId, User user, string displayName, string selfPortrait)
        {
            IPAddress callerIP = Singleton.Users.FirstOrDefault((kvp) => kvp.Value == user).Key;
            if (callerIP == null)
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
