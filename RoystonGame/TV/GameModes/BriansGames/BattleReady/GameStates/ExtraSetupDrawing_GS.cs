using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
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
            DrawingType drawingType = GetNeededDrawingType();
            UsersToTypeDrawing.AddOrReplace(user, drawingType);
            return new UserPrompt()
            {
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

            List<DrawingType> drawingTypes = new List<DrawingType>() { DrawingType.Head, DrawingType.Body, DrawingType.Legs };
            foreach (DrawingType baseType in drawingTypes)
            {
                if (drawingType == baseType)
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
            }

            return (true, string.Empty);
        }

        private DrawingType GetNeededDrawingType()
        {
            int numHeadsNeeded = DrawingTypeToNumNeeded[DrawingType.Head];
            int numBodiesNeeded = DrawingTypeToNumNeeded[DrawingType.Body];
            int numLegsNeeded = DrawingTypeToNumNeeded[DrawingType.Legs];

            int random = Rand.Next(0, numHeadsNeeded + numBodiesNeeded + numLegsNeeded);

            if (random < numHeadsNeeded)
            {
                return DrawingType.Head;
            }
            else if (random < numHeadsNeeded + numBodiesNeeded)
            {
                return DrawingType.Body;
            }
            else
            {
                return DrawingType.Legs;
            }
        }
    }
}
