using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.BriansGames.HintHint.DataModels;
using Common.Code.Extensions;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using System.Threading.Tasks;

namespace Backend.Games.BriansGames.HintHint.GameStates
{
    public class HintRound_GS : GameState
    {
        public HintRound_GS(
            Lobby lobby,
            RealFakePair realFakePair,
            int maxHints,
            int maxGuesses,
            TimeSpan? guessingTimer = null) : base(lobby, guessingTimer)
        {
            ConcurrentBag<(string, DateTime)> hints = new ConcurrentBag<(string, DateTime)>();
            ConcurrentBag<(string, DateTime)> guesses = new ConcurrentBag<(string, DateTime)>();
            List<string> orderedHints = new List<string>();
            ConcurrentDictionary<User, int> userToNumLeft = new ConcurrentDictionary<User, int>();
            foreach (User user in lobby.GetAllUsers())
            {
                if (realFakePair.RealHintGivers.Contains(user))
                {
                    userToNumLeft.AddOrReplace(user, maxHints);
                }
                else if (realFakePair.FakeHintGivers.Contains(user))
                {
                    userToNumLeft.AddOrReplace(user, maxHints);
                }
                else
                {
                    userToNumLeft.AddOrReplace(user, maxGuesses);
                }
            }

            StateChain hintGuessChain = new StateChain(stateGenerator: (int counter) =>
            {
                if (userToNumLeft.Values.Any(numLeft => numLeft > 0))
                {
                    return new SelectivePromptUserState(
                        usersToPrompt: userToNumLeft.Keys.Where(user => userToNumLeft[user] > 0).ToList(),
                        promptGenerator: (User user) =>
                        {
                            if (realFakePair.RealHintGivers.Contains(user))
                            {
                                RealHintPromptGenerator(user);
                            }
                            else if (realFakePair.FakeHintGivers.Contains(user))
                            {
                                
                            }
                            else
                            {
                                
                            }
                        },
                        formSubmitHandler: (User user, UserFormSubmission input) =>
                        {

                        },
                        userTimeoutHandler: (User user, UserFormSubmission input) =>
                        {

                        });
                }
                else
                {
                    return null;
                }
            });



            this.UnityView = new UnityView(lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = "Hint Time!" },
                Instructions = new UnityField<string> { Value = "Be careful, someone may be trying to hint you to the wrong word." },
            };


            UserPrompt RealHintPromptGenerator(User user)
            {
                string description = "You cannot say any of the following words:";
                description += $"\n {realFakePair.RealGoal}";
                foreach (string banned in realFakePair.BannedWords)
                {
                    description += $"\n {banned}";
                }
                return new UserPrompt()
                {
                    UserPromptId = UserPromptId.HintHint_Hint,
                    Title = $"You are giving hints for the word: {realFakePair.RealGoal}",
                    Description = description,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = 
                        }
                    }
                };
            }
        }

    }
}
