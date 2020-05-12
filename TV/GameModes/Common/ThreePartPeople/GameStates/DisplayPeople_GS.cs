using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;


namespace RoystonGame.TV.GameModes.BriansGames.Common.GameStates
{
    public class DisplayPeople_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };
        public DisplayPeople_GS(Lobby lobby, string title, IReadOnlyList<Person> peopleList, Action<User, UserStateResult, UserFormSubmission> outlet = null, Func<StateInlet> delayedOutlet = null) : base(lobby, outlet, delayedOutlet)
        {
            if(peopleList == null || peopleList.Count == 0)
            {
                throw new ArgumentException("PeopleList cannot be empty");
            }
            UserState partyLeaderState = new SimplePromptUserState(PartyLeaderSkipButton);
            WaitingUserState waitingState = new WaitingUserState();

            State waitForLeader = new WaitForPartyLeader(
                lobby: this.Lobby,
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderState,
                waitingState: waitingState);

            this.Entrance = waitForLeader;

            var unityImages = new List<UnityImage>();
            foreach(Person person in peopleList)
            {
                unityImages.Add(person.GetPersonImage());
            }
        
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = title },
            };
        }
    }
}
