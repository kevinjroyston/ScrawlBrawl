using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class Setup_GS : GameState
    {
        private Random Rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <param name="outlet">The state to link the end of the chain to.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<State> GetDrawingsAndPromptsUserStateChain(int numDrawingsPerUser, int numPromptsPerUser, List<PeopleUserDrawing> drawings, List<(User, string)> prompts)
        {
            List<State> stateChain = new List<State>();
            List<PeopleUserDrawing> drawingsToAdd = new List<PeopleUserDrawing>();
            for (int i = 0; i< numDrawingsPerUser; i++)
            {
                PeopleUserDrawing headDrawing = new PeopleUserDrawing
                {
                    Type = DrawingType.Head
                };
                drawingsToAdd.Add(headDrawing);

                PeopleUserDrawing bodyDrawing = new PeopleUserDrawing
                {
                    Type = DrawingType.Body
                };
                drawingsToAdd.Add(bodyDrawing);

                PeopleUserDrawing legsDrawing = new PeopleUserDrawing
                {
                    Type = DrawingType.Legs
                };
                drawingsToAdd.Add(legsDrawing);
            }
            drawingsToAdd = drawingsToAdd.OrderBy(_ => Rand.Next()).ToList();
            int drawingNumber = 1;
            foreach(PeopleUserDrawing drawing in drawingsToAdd) 
            {
                int tempDrawingNumber = drawingNumber;
                stateChain.Add(new SimplePromptUserState((User user) => new UserPrompt()
                {
                    Title = Invariant($"Time to draw! Drawing \"{tempDrawingNumber}\" of \"{numDrawingsPerUser * 3}\""),
                    Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"Draw any \"{drawing.Type.ToString()}\""),
                            Drawing = new DrawingPromptMetadata()
                            {
                                WidthInPx = ThreePartPeopleConstants.Widths[drawing.Type],
                                HeightInPx = ThreePartPeopleConstants.Heights[drawing.Type],
                                CanvasBackground = ThreePartPeopleConstants.Backgrounds[drawing.Type],
                            },
                        },
                    },
                    SubmitButton = true
                },
                formSubmitListener: (User user, UserFormSubmission input) =>
                {
                    drawings.Add(new PeopleUserDrawing
                    {
                        Drawing = input.SubForms[0].Drawing,
                        Owner = user,
                        Type = drawing.Type
                    });
                    return (true, String.Empty);
                }));
                drawingNumber++;
            }
            int  promptNumber = 1;
            for (int i = 0; i < numPromptsPerUser; i++)
            {
                int tempPromptNumber = promptNumber;
                stateChain.Add(new SimplePromptUserState((User user) => new UserPrompt()
                {
                    Title = Invariant($"Now lets make some battle prompts! Prompt \"{tempPromptNumber}\" of \"{numPromptsPerUser}\""),
                    Description = "Examples: Who would win in a fight, Who would make the best ____, Etc.",
                    RefreshTimeInMs = 1000,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt="Prompt",
                            ShortAnswer=true
                        },
                    },
                    SubmitButton = true
                },
                formSubmitListener: (User user, UserFormSubmission input) =>
                {
                    if(prompts.Select((tuple)=>tuple.Item2).Contains(input.SubForms[0].ShortAnswer))
                    {
                        return (false, "Someone has already entered that prompt");
                    }
                    prompts.Add((user, input.SubForms[0].ShortAnswer));
                    return (true, String.Empty);
                }));
                promptNumber++;
            }

            return stateChain;
        }
     

        public Setup_GS(Lobby lobby, List<PeopleUserDrawing> drawings, List<(User, string)> prompts, int numDrawingsPerUserPerPart, int numPromptsPerUser)
            : base(
                  lobby: lobby,
                  exit: new WaitForAllUsers_StateExit(lobby))
        {
            StateChain stateChain = new StateChain(GetDrawingsAndPromptsUserStateChain(
                numDrawingsPerUser: numDrawingsPerUserPerPart,
                numPromptsPerUser: numPromptsPerUser,
                drawings: drawings,
                prompts: prompts));
            this.Entrance.Transition(stateChain);
            stateChain.Transition(this.Exit);
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete all the drawings and prompts on your devices." },
            };
        }
        
    }
}
