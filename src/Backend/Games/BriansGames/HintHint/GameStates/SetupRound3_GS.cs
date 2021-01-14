using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.HintHint.DataModels;
using Backend.Games.Common;
using Common.Code.Extensions;
using Common.Code.Helpers;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Backend.Games.BriansGames.HintHint.GameStates
{
    public class SetupRound3_GS : GameState
    {
        public SetupRound3_GS(Lobby lobby, List<RealFakePair> realFakePairs, TimeSpan? setupDuration)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            Dictionary<User, List<RealFakePair>> usersToRealPairs = new Dictionary<User, List<RealFakePair>>();
            ConcurrentDictionary<RealFakePair, ConcurrentBag<int>> pairToFakeGoalChoices = new ConcurrentDictionary<RealFakePair, ConcurrentBag<int>>();

            int largesRealList = 0;
            foreach (RealFakePair rfp in realFakePairs)
            {
                foreach (User user in rfp.RealHintGivers)
                {
                    if (!usersToRealPairs.ContainsKey(user))
                    {
                        usersToRealPairs.Add(user, new List<RealFakePair>());
                    }
                    usersToRealPairs[user].Add(rfp);
                    if (usersToRealPairs[user].Count > largesRealList)
                    {
                        largesRealList = usersToRealPairs[user].Count;
                    }
                }
            }

            StateChain getFakeGoal = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter >= largesRealList)
                    {
                        return null;
                    }
                    return new SelectivePromptUserState(
                        usersToPrompt: lobby.GetAllUsers().Where(user => usersToRealPairs.ContainsKey(user) && usersToRealPairs[user].Count > counter).ToList(),
                        promptGenerator: (User user) => new UserPrompt()
                        {
                            UserPromptId = UserPromptId.HintHint_SetupRound1,
                            Title = "Game Setup Round 2",
                            Description = "These are the words you are not allowed to use for your hints, pick one of them to be the fake clue giver's word (It will be picked based off of what you and your fellow clue givers choose)",
                            SubPrompts = new SubPrompt[]
                            {
                                new SubPrompt
                                {      
                                    Prompt = "Pick the one you want to be the fake word",
                                    Answers =  usersToRealPairs[user][counter].BannedWords.ToArray(),
                                },
                            },
                            SubmitButton = true
                        },
                        formSubmitHandler: (User user, UserFormSubmission input) =>
                        {
                            pairToFakeGoalChoices.AddOrAppend(usersToRealPairs[user][counter], input.SubForms[0].RadioAnswer ?? -1);
                            return (true, string.Empty);
                        },
                        maxPromptDuration: setupDuration,
                        userTimeoutHandler: (User user, UserFormSubmission input) =>
                        {
                            if (input.SubForms?.Count > 0)
                            {
                                pairToFakeGoalChoices.AddOrAppend(usersToRealPairs[user][counter], input.SubForms[0]?.RadioAnswer ?? -1);
                            }
                            return UserTimeoutAction.None;
                        },
                       exit: new WaitForUsers_StateExit(lobby: this.Lobby, usersToWaitFor: WaitForUsersType.All));
                });

            getFakeGoal.AddExitListener(() =>
            {
                foreach (RealFakePair rfp in pairToFakeGoalChoices.Keys)
                {
                    Dictionary<string, int> bannedWordsToWeight = new Dictionary<string, int>();
                    foreach (int choice in pairToFakeGoalChoices[rfp])
                    {
                        if (choice >= 0)
                        {
                            if (!bannedWordsToWeight.ContainsKey(rfp.BannedWords[choice]))
                            {
                                bannedWordsToWeight.Add(rfp.BannedWords[choice], 0);
                            }
                            bannedWordsToWeight[rfp.BannedWords[choice]] = bannedWordsToWeight[rfp.BannedWords[choice]] + 1;
                        }
                    }
                    rfp.FakeGoal = MathHelpers.GetWeightedRandom(bannedWordsToWeight);
                }
            });

            this.Entrance.Transition(getFakeGoal);
            getFakeGoal.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Instructions = new UnityField<string> { Value = "These are the words that you will be giving real hints for. On your devices chose one of your banned words to be the word that the fake hint givers will try to get people to guess." },
            };
        }
    }
}
