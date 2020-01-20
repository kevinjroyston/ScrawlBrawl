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
using RoystonGame.Web.DataModels.UnityObjects;
using System.Collections.Concurrent;
using RoystonGame.Web.DataModels;

namespace RoystonGame.TV
{
    public class GameManager
    {
        #region References
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
        #endregion

        #region User and Object trackers
        private ConcurrentDictionary<IPAddress, User> RegisteredUsers { get; } = new ConcurrentDictionary<IPAddress, User>();
        private ConcurrentDictionary<IPAddress, User> UnregisteredUsers { get; } = new ConcurrentDictionary<IPAddress, User>();
        private ConcurrentDictionary<Guid, Lobby> LobbyIdToLobbies { get; } = new ConcurrentDictionary<Guid, Lobby>();
        private ConcurrentDictionary<string, Lobby> LobbyCodeToLobbies { get; } = new ConcurrentDictionary<string, Lobby>();

        /// <summary>
        /// Hacky fix for passing this info to GameNotifier thread.
        /// </summary>
        public ConcurrentBag<Guid> AbandonedLobbyIds { get; } = new ConcurrentBag<Guid>();
        #endregion

        #region States
        private UserState UserRegistration { get; set; } = new SimplePromptUserState(
            prompt: UserNamePrompt,
            outlet: (User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                GetLobby(user.LobbyId.Value).Inlet(user, result, userInput);
            },
            // Outlet won't be called until the below returns true
            formSubmitListener: (User user, UserFormSubmission userInput) =>
            {
                return RegisterUser(userInput.SubForms[1].ShortAnswer, user, userInput.SubForms[0].ShortAnswer, userInput.SubForms[2].Drawing);
            });
        public static UserPrompt UserNamePrompt(User user) => new UserPrompt()
        {
            Title = "Welcome to the game!",
            Description = "Fill in the information below!",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Nickname:",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "Lobby Code:",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "Self Portrait:",
                    Drawing = new DrawingPromptMetadata()
                }
            },
            SubmitButton = true,
        };
        #endregion

        internal static void ReportGameError(ErrorType type, Guid? lobbyId = null, User user = null, Exception error = null)
        {
            // Log error to console (TODO redirect to file on release builds).
            Console.Error.WriteLine(error);
            if (lobbyId.HasValue)
            {
                Lobby lobby = GetLobby(lobbyId.Value);
                Singleton.LobbyCodeToLobbies.TryRemove(lobby.LobbyCode, out Lobby _);
                Singleton.LobbyIdToLobbies.TryRemove(lobby.LobbyId, out Lobby _);

                lobby.UnregisterAllUsers();
                Singleton.AbandonedLobbyIds.Add(lobby.LobbyId);
            }
            // TODO: restart lobbies or evict users based on error type and volume.
        }

        private GameManager()
        {
            if (_internalSingleton != null)
            {
                throw new Exception("Tried to instantiate a second instance of GameManager.cs");
            }
        }

        public static void UnregisterUser(User user)
        {
            if (Singleton.RegisteredUsers.ContainsKey(user.IP))
            {
                Singleton.RegisteredUsers.Remove(user.IP, out User _);
            }
        }

        public static (bool, string) RegisterUser(string lobbyCode, User user, string displayName, string selfPortrait)
        {
            IPAddress callerIP = Singleton.UnregisteredUsers.FirstOrDefault((kvp) => kvp.Value == user).Key;
            if (callerIP == null)
            {
                return (false, "Can't register unknown user. Refresh the page.");
            }

            user.DisplayName = displayName;
            user.SelfPortrait = selfPortrait;

            string errorMsg = "Lobby not found";

            // TODO: move to a different lobby creation model.
            // TODO: add way to leave a lobby.
            if (!Singleton.LobbyCodeToLobbies.ContainsKey(lobbyCode))
            {
                Lobby newLobby = new Lobby(lobbyCode);

                if (!Singleton.LobbyCodeToLobbies.TryAdd(lobbyCode, newLobby)
                    || !Singleton.LobbyIdToLobbies.TryAdd(newLobby.LobbyId, newLobby))
                {
                    bool success = Singleton.LobbyCodeToLobbies.TryRemove(lobbyCode, out Lobby _);
                    success &= Singleton.LobbyIdToLobbies.TryRemove(newLobby.LobbyId, out Lobby _);
                    if (!success)
                    {
                        throw new Exception("Lobby lists corrupted");
                    }
                    return (false, "Unexpected error occurred while creating lobby. Refresh and try again.");
                }
            }

            if(!Singleton.LobbyCodeToLobbies[lobbyCode].TryAddUser(user, out errorMsg))
            {
                return (false, errorMsg);
            }

            if(!Singleton.UnregisteredUsers.TryRemove(callerIP, out User usr)
                || usr.UserId != user.UserId
                || !Singleton.RegisteredUsers.TryAdd(callerIP, user))
            {
                throw new Exception("Unexpected error occurred while registering user. Refresh and try again");
            }
            return (true, string.Empty);
        }

        public static User MapIPToUser(IPAddress callerIP, out bool newUser)
        {
            newUser = false;
            try
            {
                if (Singleton.UnregisteredUsers.ContainsKey(callerIP))
                {
                    return Singleton.UnregisteredUsers[callerIP];
                }
                else if (Singleton.RegisteredUsers.ContainsKey(callerIP))
                {
                    return Singleton.RegisteredUsers[callerIP];
                }

                User user = new User(callerIP);
                if(Singleton.UnregisteredUsers.TryAdd(callerIP, user))
                {
                    newUser = true;
                    Singleton.UserRegistration.Inlet(user, UserStateResult.Success, null);
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

        public static List<Lobby> GetLobbies()
        {
            return Singleton.LobbyCodeToLobbies.Values.ToList();
        }

        public static Lobby GetLobby(Guid lobbyId)
        {
            return Singleton.LobbyIdToLobbies.GetValueOrDefault(lobbyId);
        }

        public static Lobby GetLobby(string friendlyName)
        {
            return Singleton.LobbyCodeToLobbies.GetValueOrDefault(friendlyName);
        }
    }
}
