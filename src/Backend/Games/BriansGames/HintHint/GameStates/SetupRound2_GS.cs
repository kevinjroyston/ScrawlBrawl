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
using Common.Code.Extensions;
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
    public class SetupRound2_GS : GameState
    {
        public SetupRound2_GS(Lobby lobby, List<RealFakePair> realFakePairs, int numBannedWordsPerPerson, TimeSpan? setupDuration)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            List<SubPrompt> subPrompts = new List<SubPrompt>();
            for (int i = 0; i < numBannedWordsPerPerson; i++)
            {
                subPrompts.Add(new SubPrompt
                {
                    Prompt = Invariant($"Banned Word #{i + 1}"),
                    ShortAnswer = true,
                });
            }
            Dictionary<User, List<RealFakePair>> usersToFakePairs = new Dictionary<User, List<RealFakePair>>();
            ConcurrentDictionary<RealFakePair, List<UserSubForm>> pairToInputSubForms = new ConcurrentDictionary<RealFakePair, List<UserSubForm>>();
            int largestFakeList = 0;
            foreach (RealFakePair rfp in realFakePairs)
            {
                foreach (User user in rfp.FakeHintGivers)
                {
                    if (!usersToFakePairs.ContainsKey(user))
                    {
                        usersToFakePairs.Add(user, new List<RealFakePair>());
                    }
                    usersToFakePairs[user].Add(rfp);
                    if (usersToFakePairs[user].Count > largestFakeList)
                    {
                        largestFakeList = usersToFakePairs[user].Count;
                    }
                }
            }

            StateChain getBannedWords = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter >= largestFakeList)
                    {
                        return null;
                    }
                    return new SelectivePromptUserState(
                        usersToPrompt: lobby.GetAllUsers().Where(user => usersToFakePairs.ContainsKey(user) && usersToFakePairs[user].Count > counter).ToList(),
                        promptGenerator: (User user) => new UserPrompt()
                        {
                            UserPromptId = UserPromptId.HintHint_SetupRound1,
                            Title = "Game Setup Round 2",
                            Description = Invariant($"You will be giving fake clues for this word: {usersToFakePairs[user][counter].RealGoal}! What {numBannedWordsPerPerson} words can't the other clue givers say?"),
                            SubPrompts = subPrompts.ToArray(),
                            SubmitButton = true
                        },
                        formSubmitHandler: (User user, UserFormSubmission input) =>
                        {

                            pairToInputSubForms.AddOrUpdate(
                                key: usersToFakePairs[user][counter],
                                addValue: input.SubForms,
                                updateValueFactory: (RealFakePair rfp, List<UserSubForm> subForms) =>
                                {
                                    subForms.AddRange(input.SubForms);
                                    return subForms;
                                });
                            return (true, string.Empty);
                        },
                        maxPromptDuration: setupDuration,
                        userTimeoutHandler: (User user, UserFormSubmission input) =>
                        {
                            if (input.SubForms?.Count > 0)
                            {
                                pairToInputSubForms.AddOrUpdate(
                                key: usersToFakePairs[user][counter],
                                addValue: input.SubForms,
                                updateValueFactory: (RealFakePair rfp, List<UserSubForm> subForms) =>
                                {
                                    subForms.AddRange(input.SubForms);
                                    return subForms;
                                });
                            }
                            return UserTimeoutAction.None;
                        },
                       exit: new WaitForUsers_StateExit(lobby: this.Lobby, usersToWaitFor: WaitForUsersType.All));
                });

            getBannedWords.AddExitListener( () => 
            {
                foreach (RealFakePair rfp in pairToInputSubForms.Keys)
                {
                    rfp.BannedWords = new List<string>();
                    foreach (UserSubForm subForm in pairToInputSubForms[rfp])
                    {
                        if (subForm?.ShortAnswer != null 
                        && !rfp.BannedWords.Any(str => !str.FuzzyEquals(subForm.ShortAnswer))
                        && !rfp.RealGoal.FuzzyEquals(subForm.ShortAnswer))
                        {
                            rfp.BannedWords.Add(subForm.ShortAnswer);
                        }
                    }
                }
            });
            this.Entrance.Transition(getBannedWords);
            getBannedWords.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId =TVScreenId.WaitForUserInputs,
                Instructions = new UnityField<string> { Value = "These are the words that you will be giving fake hints for. On your devices write some banned words that the other clue givers can't use." },
            };
        }
    }
}
