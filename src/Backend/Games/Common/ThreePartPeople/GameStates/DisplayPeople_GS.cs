using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.Games.Common.ThreePartPeople.DataModels;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.BriansGames.Common.GameStates
{
    public class DisplayPeople_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.PartyLeader_SkipReveal,
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
            var unityImages = new List<Legacy_UnityImage>();
            foreach(Person person in peopleList)
            {
                unityImages.Add(person.GetUnityImage(
                    backgroundColor: backgroundColor(person),
                    imageIdentifier: imageIdentifier(person),
                    title: imageTitle(person),
                    header: imageHeader(person)
                    ));
            }
        
            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<Legacy_UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = title },
            };
        }
    }
}
