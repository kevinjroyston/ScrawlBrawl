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
using Backend.Games.Common.ThreePartPeople.Extensions;

namespace Backend.Games.BriansGames.BattleReady.GameStates
{
    public class ExtraSetupDrawing_GS : ExtraSetupGameState
    {
        private Random Rand = new Random();
        private ConcurrentBag<PeopleUserDrawing> Drawings { get; set; }
        private ConcurrentDictionary<User, BodyPartType> UsersToTypeDrawing { get; set; } = new ConcurrentDictionary<User, BodyPartType>();

        private ConcurrentDictionary<BodyPartType, int> BodyPartTypeToNumNeeded { get; set; } = new ConcurrentDictionary<BodyPartType, int>();

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

            BodyPartTypeToNumNeeded.AddOrReplace(BodyPartType.Head, numHeadsNeeded);
            BodyPartTypeToNumNeeded.AddOrReplace(BodyPartType.Body, numBodiesNeeded);
            BodyPartTypeToNumNeeded.AddOrReplace(BodyPartType.Legs, numLegsNeeded);
        }
        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            BodyPartType bodyPartType = MathHelpers.GetWeightedRandom(BodyPartTypeToNumNeeded);

            UsersToTypeDrawing.AddOrReplace(user, bodyPartType);
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.BattleReady_ExtraBodyPartDrawing,
                Title = Constants.UIStrings.DrawingPromptTitle,
                Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = Invariant($"Draw any \"{bodyPartType.ToString()}\""),
                        Drawing = new DrawingPromptMetadata()
                        {
                            DrawingType = bodyPartType.GetDrawingType(),
                        },
                    },
                },
                SubmitButton = true
            };
        }
        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            BodyPartType bodyPartType = UsersToTypeDrawing[user];
            this.Drawings.Add(new PeopleUserDrawing
            {
                Drawing = input.SubForms[0].Drawing,
                Owner = user,
                Type = bodyPartType
            });

            if (bodyPartType != BodyPartType.None)
            {
                BodyPartTypeToNumNeeded[bodyPartType]--;

                if (BodyPartTypeToNumNeeded[bodyPartType] <= 0)
                {
                    List<User> usersDrawingThisType = UsersToTypeDrawing.Keys.Where(user => UsersToTypeDrawing[user] == bodyPartType).ToList();
                    foreach (User userBodyPartType in usersDrawingThisType)
                    {
                        userBodyPartType.UserState.HurryUsers();
                    }
                }
            }

            return (true, string.Empty);
        }
    }
}
