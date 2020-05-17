using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
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
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class ContestantCreation_GS : GameState
    {
        private RoundTracker RoundTracker { get; set; }

        public ContestantCreation_GS(Lobby lobby, RoundTracker roundTracker)
            : base(
                  lobby:lobby,
                  exit: new WaitForAllUsers_StateExit(lobby))
        {
            this.RoundTracker = roundTracker;
            MultiStateChain contestantsMultiStateChain = new MultiStateChain(MakePeopleUserStateChain);

            this.Entrance.Transition(contestantsMultiStateChain);
            contestantsMultiStateChain.Transition(this.Exit);

            this.UnityView = new UnityView
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
                                Prompt = "Pick your Head",
                                Answers = prompt.UsersToUserHands[user].Heads.Select(userDrawing => CommonHelpers.HtmlImageWrapper(userDrawing.Drawing, ThreePartPeopleConstants.Widths[DrawingType.Body], ThreePartPeopleConstants.Heights[DrawingType.Body])).ToArray()                       
                            },
                            new SubPrompt
                            {
                                Prompt = "Pick your Body",
                                Answers = prompt.UsersToUserHands[user].Bodies.Select(userDrawing => CommonHelpers.HtmlImageWrapper(userDrawing.Drawing, ThreePartPeopleConstants.Widths[DrawingType.Body], ThreePartPeopleConstants.Heights[DrawingType.Body])).ToArray()
                            },
                            new SubPrompt
                            {
                                Prompt = "Pick your Legs",
                                Answers = prompt.UsersToUserHands[user].Legs.Select(userDrawing => CommonHelpers.HtmlImageWrapper(userDrawing.Drawing, ThreePartPeopleConstants.Widths[DrawingType.Body], ThreePartPeopleConstants.Heights[DrawingType.Body])).ToArray()
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
                    formSubmitListener: (User user, UserFormSubmission input) =>
                    {
                        prompt.UsersToUserHands[user].Heads[(int)input.SubForms[0].RadioAnswer].Owner.Score += BattleReadyConstants.PointsForPartUsed;
                        prompt.UsersToUserHands[user].Bodies[(int)input.SubForms[1].RadioAnswer].Owner.Score += BattleReadyConstants.PointsForPartUsed;
                        prompt.UsersToUserHands[user].Legs[(int)input.SubForms[2].RadioAnswer].Owner.Score += BattleReadyConstants.PointsForPartUsed;
                        prompt.UsersToUserHands[user].Contestant = new Person
                        {
                            BodyPartDrawings = new Dictionary<DrawingType, PeopleUserDrawing>{
                                {DrawingType.Head, prompt.UsersToUserHands[user].Heads[(int)input.SubForms[0].RadioAnswer] },
                                {DrawingType.Body, prompt.UsersToUserHands[user].Bodies[(int)input.SubForms[1].RadioAnswer] },
                                {DrawingType.Legs, prompt.UsersToUserHands[user].Legs[(int)input.SubForms[2].RadioAnswer] }
                            },
                            Name = input.SubForms[3].ShortAnswer
                        };
                        return (true, String.Empty);
                    }
                    ));
            }
            return stateChain;
        }
    }
}
