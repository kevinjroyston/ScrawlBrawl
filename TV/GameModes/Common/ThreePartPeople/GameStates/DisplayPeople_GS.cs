using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;

namespace RoystonGame.TV.GameModes.BriansGames.Common.GameStates
{
    public class DisplayPeople_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };
        public DisplayPeople_GS(
            Lobby lobby,
            string title,
            IReadOnlyList<Person> peopleList,     
            Func<Person, Color?> backgroundColor = null,
            Func<Person, string> imageIdentifier = null,
            Func<Person, string> imageTitle = null,
            Func<Person, string> imageHeader = null)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            if(peopleList == null || peopleList.Count == 0)
            {
                throw new ArgumentException("PeopleList cannot be empty");
            }

            this.Entrance.Transition(this.Exit);

            backgroundColor ??= (person) => null;
            imageIdentifier ??= (person) => null;
            imageTitle ??= (person) => null;
            imageHeader ??= (person) => null;
            var unityImages = new List<UnityImage>();
            foreach(Person person in peopleList)
            {
                unityImages.Add(person.GetPersonImage(
                    backgroundColor: backgroundColor(person),
                    imageIdentifier: imageIdentifier(person),
                    title: imageTitle(person),
                    header: imageHeader(person)
                    ));
            }
        
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = title },
            };
        }
    }
}
