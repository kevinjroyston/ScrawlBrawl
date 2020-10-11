using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Responses;
using Common.DataModels.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using Backend.Games.Common.ThreePartPeople;
using Common.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.Games.Common.GameStates;
using Backend.GameInfrastructure;

namespace Backend.Games.BriansGames.BattleReady.GameStates
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
                UserPromptId = UserPromptId.BattleReady_BodyPartDrawing,
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
