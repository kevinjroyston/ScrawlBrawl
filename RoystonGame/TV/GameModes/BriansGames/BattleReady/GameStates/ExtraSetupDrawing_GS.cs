using RoystonGame.TV.DataModels.Users;
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
        private Dictionary<User, List<DrawingType>> UsersToDrawingTypesStillNeeded { get; set; }
        public ExtraSetupDrawing_GS(
            Lobby lobby,
            ConcurrentBag<PeopleUserDrawing> drawings,
            List<DrawingType> typesOfDrawingsStillNeeded)
            : base(
                  lobby: lobby,
                  numExtraObjectsNeeded: typesOfDrawingsStillNeeded.Count)
        {
            this.Drawings = drawings;

            foreach (User user in lobby.GetAllUsers())
            {
                UsersToDrawingTypesStillNeeded.Add(user, typesOfDrawingsStillNeeded.OrderBy(_ => Rand.Next()).ToList());
            }
        }
        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            DrawingType drawingType = UsersToDrawingTypesStillNeeded[user][counter];
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
            DrawingType drawingType = UsersToDrawingTypesStillNeeded[user][counter];
            this.Drawings.Add(new PeopleUserDrawing
            {
                Drawing = input.SubForms[0].Drawing,
                Owner = user,
                Type = drawingType
            });
            return (true, string.Empty);
        }
    }
}
