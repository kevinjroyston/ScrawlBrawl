using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.TV.DataModels.Enums;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class SetupDrawings_GS : GameState
    {
        private State GetDrawingState(ConcurrentBag<PeopleUserDrawing> drawings)
        {
            DrawingType drawingType = (DrawingType)Rand.Next(0, 3);
            return new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
                {
                    Title = "Time to draw!",
                    Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"Draw any \"{drawingType.ToString()}\""),
                            Drawing = new DrawingPromptMetadata()
                            {
                                WidthInPx = ThreePartPeopleConstants.Widths[drawingType],
                                HeightInPx = ThreePartPeopleConstants.Heights[drawingType],
                                CanvasBackground = ThreePartPeopleConstants.Backgrounds[drawingType],
                            },
                        },
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    drawings.Add(new PeopleUserDrawing
                    {
                        Drawing = input.SubForms[0].Drawing,
                        Owner = user,
                        Type = drawingType
                    });
                    return (true, string.Empty);
                },
                userTimeoutHandler: (User user, UserFormSubmission input) =>
                {
                    if (input?.SubForms?[0]?.Drawing != null)
                    {
                        drawings.Add(new PeopleUserDrawing
                        {
                            Drawing = input.SubForms[0].Drawing,
                            Owner = user,
                            Type = drawingType
                        });
                    }
                    return UserTimeoutAction.None;
                });
        }
        private Random Rand = new Random();
        public SetupDrawings_GS(Lobby lobby, ConcurrentBag<PeopleUserDrawing> drawings, TimeSpan? setupDrawingDurration, int perUserDrawingLimit)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            StateChain drawingStateChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter < perUserDrawingLimit)
                    {
                        return GetDrawingState(drawings);
                    }
                    else
                    {
                        return null;
                    }   
                },
                stateDuration: setupDrawingDurration);

            this.Entrance.Transition(drawingStateChain);
            drawingStateChain.Transition(this.Exit);
            this.UnityView = new UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete as many drawings as possible before the time runs out" },
            };
        }
    }
}
