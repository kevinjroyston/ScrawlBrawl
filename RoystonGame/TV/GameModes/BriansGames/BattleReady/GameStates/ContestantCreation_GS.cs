using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
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
            TimeSpan? multipliedCreationDuration = null;
            if (creationDuration != null)
            {
                multipliedCreationDuration = TimeSpan.FromSeconds(((TimeSpan)creationDuration).TotalSeconds * roundTracker.UsersToAssignedPrompts.Values.ToList()[0].Count); // will be cleaned up in the upcoming refactor of this game
            }
            MultiStateChain contestantsMultiStateChain = new MultiStateChain(MakePeopleUserStateChain, stateDuration: multipliedCreationDuration);

            this.Entrance.Transition(contestantsMultiStateChain);
            contestantsMultiStateChain.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Make your contestants on your devices." },
            };
        }

        private List<State> MakePeopleUserStateChain(User user)
        {
            List<State> stateChain = new List<State>();
            foreach(Prompt promptIter in RoundTracker.UsersToAssignedPrompts[user])
            {
                Prompt prompt = promptIter;
                stateChain.Add(new SimplePromptUserState(
                    promptGenerator: (User user) =>
                    {       
                        return new UserPrompt
                        {
                            Title = Invariant($"Make the best character for this prompt: \"{prompt.Text}\""),
                            SubPrompts = new SubPrompt[]
                            {
                            new SubPrompt
                            {
                                Selector = new SelectorPromptMetadata()
                                {
                                    HeightInPx = ThreePartPeopleConstants.Heights[DrawingType.Head],
                                    WidthInPx = ThreePartPeopleConstants.Widths[DrawingType.Head],
                                    ImageList = prompt.UsersToUserHands[user].Heads.Select(userDrawing => userDrawing.Drawing).ToArray()
                                }
                            },
                            new SubPrompt
                            {
                                Selector = new SelectorPromptMetadata()
                                {
                                    HeightInPx = ThreePartPeopleConstants.Heights[DrawingType.Body],
                                    WidthInPx = ThreePartPeopleConstants.Widths[DrawingType.Body],
                                    ImageList = prompt.UsersToUserHands[user].Bodies.Select(userDrawing => userDrawing.Drawing).ToArray()
                                }
                            },
                            new SubPrompt
                            {
                                Selector = new SelectorPromptMetadata()
                                {
                                    HeightInPx = ThreePartPeopleConstants.Heights[DrawingType.Legs],
                                    WidthInPx = ThreePartPeopleConstants.Widths[DrawingType.Legs],
                                    ImageList = prompt.UsersToUserHands[user].Legs.Select(userDrawing => userDrawing.Drawing).ToArray()
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
                        prompt.UsersToUserHands[user].Heads[(int)input.SubForms[0].Selector].Owner.AddScore(BattleReadyConstants.PointsForPartUsed);
                        prompt.UsersToUserHands[user].Bodies[(int)input.SubForms[1].Selector].Owner.AddScore(BattleReadyConstants.PointsForPartUsed);
                        prompt.UsersToUserHands[user].Legs[(int)input.SubForms[2].Selector].Owner.AddScore(BattleReadyConstants.PointsForPartUsed);
                        prompt.UsersToUserHands[user].Contestant = new Person
                        {
                            BodyPartDrawings = new Dictionary<DrawingType, PeopleUserDrawing>{
                                {DrawingType.Head, prompt.UsersToUserHands[user].Heads[(int)input.SubForms[0].Selector] },
                                {DrawingType.Body, prompt.UsersToUserHands[user].Bodies[(int)input.SubForms[1].Selector] },
                                {DrawingType.Legs, prompt.UsersToUserHands[user].Legs[(int)input.SubForms[2].Selector] }
                            },
                            Name = input.SubForms[3].ShortAnswer,
                            Owner = user
                        };
                        return (true, String.Empty);
                    }
                    ));
            }
            return stateChain;
        }
    }
}
