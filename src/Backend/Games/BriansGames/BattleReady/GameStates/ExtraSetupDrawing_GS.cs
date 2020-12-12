using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common;
using Backend.Games.Common.GameStates;
using Backend.Games.Common.ThreePartPeople;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Extensions;
using Common.Code.Helpers;

namespace Backend.Games.BriansGames.BattleReady.GameStates
{
    public class ExtraSetupDrawing_GS : ExtraSetupGameState
    {
        private Random Rand = new Random();
        private ConcurrentBag<PeopleUserDrawing> Drawings { get; set; }
        private ConcurrentDictionary<User, DrawingType> UsersToTypeDrawing { get; set; } = new ConcurrentDictionary<User, DrawingType>();

        private ConcurrentDictionary<DrawingType, int> DrawingTypeToNumNeeded { get; set; } = new ConcurrentDictionary<DrawingType, int>();

        public ExtraSetupDrawing_GS(
            Lobby lobby,
            ConcurrentBag<PeopleUserDrawing> drawings,
            int numHeadsNeeded,
            int numBodiesNeeded,
            int numLegsNeeded)
            : base(
                  lobby: lobby,
                  numExtraObjectsNeeded: numHeadsNeeded + numBodiesNeeded + numLegsNeeded)
        {
            this.Drawings = drawings;

            DrawingTypeToNumNeeded.AddOrReplace(DrawingType.Head, numHeadsNeeded);
            DrawingTypeToNumNeeded.AddOrReplace(DrawingType.Body, numBodiesNeeded);
            DrawingTypeToNumNeeded.AddOrReplace(DrawingType.Legs, numLegsNeeded);
        }
        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            DrawingType drawingType = MathHelpers.GetWeightedRandom(DrawingTypeToNumNeeded);

            UsersToTypeDrawing.AddOrReplace(user, drawingType);
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.BattleReady_ExtraBodyPartDrawing,
                Title = Constants.UIStrings.DrawingPromptTitle,
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
                            GalleryType = drawingType.ToString().ToUpper(),

                        },
                    },
                },
                SubmitButton = true
            };
        }
        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            DrawingType drawingType = UsersToTypeDrawing[user];
            this.Drawings.Add(new PeopleUserDrawing
            {
                Drawing = input.SubForms[0].Drawing,
                Owner = user,
                Type = drawingType
            });

            if (drawingType != DrawingType.None)
            {
                DrawingTypeToNumNeeded[drawingType]--;

                if (DrawingTypeToNumNeeded[drawingType] <= 0)
                {
                    List<User> usersDrawingThisType = UsersToTypeDrawing.Keys.Where(user => UsersToTypeDrawing[user] == drawingType).ToList();
                    foreach (User userDrawingType in usersDrawingThisType)
                    {
                        userDrawingType.UserState.HurryUsers();
                    }
                }
            }

            return (true, string.Empty);
        }
    }
}
