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
using RoystonGame.TV.GameModes.Common.GameStates;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class SetupDrawings_GS : SetupGameState
    {
        private Random Rand { get; } = new Random();
        private Dictionary<User, List<DrawingType>> UsersToRandomizedDrawingTypes { get; set; } = new Dictionary<User, List<DrawingType>>();
        private int NumExpectedPerUser { get; set; }
        private ConcurrentBag<PeopleUserDrawing> Drawings { get; set; }
        public SetupDrawings_GS(
            Lobby lobby,
            ConcurrentBag<PeopleUserDrawing> drawings,
            int numExpectedPerUser,
            TimeSpan? setupDurration = null)
            : base(
                lobby: lobby,
                numExpectedPerUser: numExpectedPerUser,
                unityTitle: "",
                unityInstructions: "Complete as many drawings as possible before the time runs out",
                setupDurration: setupDurration)
        {
            this.NumExpectedPerUser = numExpectedPerUser;
            this.Drawings = drawings;
            
            foreach(User user in lobby.GetAllUsers())
            {
                UsersToRandomizedDrawingTypes.Add(user, ThreePartPeopleConstants.DrawingTypesList.OrderBy(_ => Rand.Next()).ToList());
            }
        }

        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            if (counter % 3 == 0)
            {
                UsersToRandomizedDrawingTypes[user] = ThreePartPeopleConstants.DrawingTypesList.OrderBy(_ => Rand.Next()).ToList();
            }
            DrawingType drawingType = UsersToRandomizedDrawingTypes[user][counter % 3];
            return new UserPrompt()
            {
                Title = Invariant($"Time to draw! Drawing {counter + 1} of {NumExpectedPerUser} expected"),
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
            };
        }
        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            this.Drawings.Add(new PeopleUserDrawing
            {
                Drawing = input.SubForms[0].Drawing,
                Owner = user,
                Type = UsersToRandomizedDrawingTypes[user][counter % 3]
            });
            return (true, string.Empty);
        }
        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter)
        {
            if (input?.SubForms?[0]?.Drawing != null)
            {
                Drawings.Add(new PeopleUserDrawing
                {
                    Drawing = input.SubForms[0].Drawing,
                    Owner = user,
                    Type = UsersToRandomizedDrawingTypes[user][counter % 3]
                });
            }
            return UserTimeoutAction.None;
        }
    }
}
