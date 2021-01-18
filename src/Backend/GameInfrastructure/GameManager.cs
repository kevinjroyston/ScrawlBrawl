using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.APIs.DataModels;
using Backend.APIs.DataModels.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static System.FormattableString;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Common.DataModels.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.GameInfrastructure
{
    public class GameManager
    {
        // TODO: hook this up to applications insights.
        private ILogger<GameManager> Logger { get; }

        public static GameManager Singleton { get; private set; }
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
                },
                userTimeoutHandler: (User user, UserFormSubmission userInput) => throw new Exception("Can't timeout on this prompt"));

            toReturn.Transition((User user)=> GetLobby(user.LobbyId));
            return toReturn;
        }
        public static UserPrompt UserNamePrompt(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.JoinLobby,
            Title = "Join a game",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Nickname",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "Lobby Code",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "Self Portrait",
                    Drawing = new DrawingPromptMetadata{
                        DrawingType = DrawingType.Profile,
                        GalleryOptions = new GalleryOptionsMetadata {
                            GalleryAutoLoadMostRecent = true 
                        },
                    }

                }
            },
            SubmitButtonText ="Join",
            SubmitButton = true,
        };
        #endregion

        public GameManager(ILogger<GameManager> logger)
        {
            if (Singleton != null)
            {
                throw new Exception("GameManager instantiated twice");
            }
            Singleton = this;
            Logger = logger;
        }

        public void ReportGameError(ErrorType type, string lobbyId, User user = null, Exception error = null)
        {
            // Log error to console (TODO redirect to file on release builds).
            Debug.WriteLine(Invariant($"ERROR ERROR ERROR ERROR ERROR ~~~~~~~~~~~~~~~~~~~~~~~~~ {error}"));
            Debug.Assert(false, error.ToString());
            Logger.LogError(error, $"LobbyId:{lobbyId},ErrorType:{type},User:[{user?.DisplayName}|{user?.Id}|{user?.AuthenticatedUserPrincipalName}]");

            if (!string.IsNullOrWhiteSpace(lobbyId))
            {
                DeleteLobby(lobbyId);
            }

            // All users or no users for now.
            /*if (user != null)
            {
                UnregisterUser(user);
            }*/
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
