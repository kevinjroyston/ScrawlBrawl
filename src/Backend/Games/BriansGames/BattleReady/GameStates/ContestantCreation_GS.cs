using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.BattleReady.DataModels;
using Backend.Games.Common.ThreePartPeople;
using Backend.Games.Common.ThreePartPeople.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.Code.Extensions;

namespace Backend.Games.BriansGames.BattleReady.GameStates
{
    public class ContestantCreation_GS : GameState
    {
        private RoundTracker RoundTracker { get; set; }

        public ContestantCreation_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? creationDuration)
            : base(
                  lobby:lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            this.RoundTracker = roundTracker;
            TimeSpan? multipliedCreationDuration = creationDuration.MultipliedBy(roundTracker.UsersToAssignedPrompts.Values.ToList()[0].Count);
            
            MultiStateChain contestantsMultiStateChain = new MultiStateChain(MakePeopleUserStateChain, stateDuration: multipliedCreationDuration);

            this.Entrance.Transition(contestantsMultiStateChain);
            contestantsMultiStateChain.Transition(this.Exit);

            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Make your contestants on your devices." },
            };
        }

        private List<State> MakePeopleUserStateChain(User user)
        {
            List<State> stateChain = new List<State>();
            if (!RoundTracker.UsersToAssignedPrompts.ContainsKey(user))
            {
                return stateChain;
            }
            foreach(Prompt promptIter in RoundTracker.UsersToAssignedPrompts[user])
            {
                Prompt prompt = promptIter;
                stateChain.Add(new SimplePromptUserState(
                    promptGenerator: (User user) =>
                    {       
                        return new UserPrompt
                        {
                            UserPromptId = UserPromptId.BattleReady_ContestantCreation,
                            Title = Invariant($"Make the best character for this prompt: \"{prompt.Text}\""),
                            SubPrompts = new SubPrompt[]
                            {
                            new SubPrompt
                            {
                                Selector = new SelectorPromptMetadata()
                                {
                                    HeightInPx = ThreePartPeopleConstants.Heights[BodyPartType.Head],
                                    WidthInPx = ThreePartPeopleConstants.Widths[BodyPartType.Head],
                                    ImageList = prompt.UsersToUserHands[user].HeadChoices.Select(userDrawing => userDrawing.Drawing).ToArray()
                                }
                            },
                            new SubPrompt
                            {
                                Selector = new SelectorPromptMetadata()
                                {
                                    HeightInPx = ThreePartPeopleConstants.Heights[BodyPartType.Body],
                                    WidthInPx = ThreePartPeopleConstants.Widths[BodyPartType.Body],
                                    ImageList = prompt.UsersToUserHands[user].BodyChoices.Select(userDrawing => userDrawing.Drawing).ToArray()
                                }
                            },
                            new SubPrompt
                            {
                                Selector = new SelectorPromptMetadata()
                                {
                                    HeightInPx = ThreePartPeopleConstants.Heights[BodyPartType.Legs],
                                    WidthInPx = ThreePartPeopleConstants.Widths[BodyPartType.Legs],
                                    ImageList = prompt.UsersToUserHands[user].LegChoices.Select(userDrawing => userDrawing.Drawing).ToArray()
                                }
                            },
                            new SubPrompt
                            {
                                Prompt = "Now give your character a name",
                                ShortAnswer = true
                            },
                            },
                            SubmitButton = true,
                        };
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        prompt.UsersToUserHands[user].BodyPartDrawings = new Dictionary<BodyPartType, PeopleUserDrawing>{
                                {BodyPartType.Head, prompt.UsersToUserHands[user].HeadChoices[(int)input.SubForms[0].Selector] },
                                {BodyPartType.Body, prompt.UsersToUserHands[user].BodyChoices[(int)input.SubForms[1].Selector] },
                                {BodyPartType.Legs, prompt.UsersToUserHands[user].LegChoices[(int)input.SubForms[2].Selector] }
                            };
                        prompt.UsersToUserHands[user].Name = input.SubForms[3].ShortAnswer;
                        prompt.UsersToUserHands[user].Owner = user;
                        return (true, String.Empty);
                    }
                    ));
            }
            return stateChain;
        }
    }
}
