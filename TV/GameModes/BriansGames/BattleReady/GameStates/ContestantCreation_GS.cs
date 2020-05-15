using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
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
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class ContestantCreation_GS : GameState
    {
        private RoundTracker RoundTracker { get; set; }

        public ContestantCreation_GS(Lobby lobby, RoundTracker roundTracker, Connector outlet = null, Func<StateInlet> delayedOutlet = null) : base(lobby, outlet, delayedOutlet)
        {
            this.RoundTracker = roundTracker;

            State waitForAllPlayers = new WaitForAllPlayers(lobby: lobby, outlet: this.Outlet);
            State waitForALlPlayersIntro = new WaitForAllPlayers(lobby: lobby);
            Func<User, StateInlet> makePeopleStateChainHelper(Connector outlet)
            {
                return (user) => MakePeopleUserStateChain(user, outlet);
            }
            waitForALlPlayersIntro.Transition(makePeopleStateChainHelper(waitForAllPlayers.Inlet));
            this.Entrance = waitForALlPlayersIntro;

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Make your contestants on your devices." },
            };
        }

        private StateInlet MakePeopleUserStateChain(User user, Connector outlet)
        {
            List<UserState> stateChain = new List<UserState>();
            foreach(Prompt promptIter in RoundTracker.UsersToAssignedPrompts[user])
            {
                Prompt prompt = promptIter;
                stateChain.Add(new SimplePromptUserState(
                    prompt: (User user) =>
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
            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain[0];
        }
    }
}
