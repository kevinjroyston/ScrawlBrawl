using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.TV.GameModes.BriansGames.Common.GameStates;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using System;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder
{
    public class BodyBuilderGameMode : IGameMode
    {
        private List<Setup_Person> PeopleList { get; set; } = new List<Setup_Person>();
        private GameState Setup { get; set; }
        private RoundTracker RoundTracker { get; } = new RoundTracker();
        public BodyBuilderGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);

            int numRounds = (int)gameModeOptions[0].ValueParsed;
            Setup = new Setup_GS(lobby: lobby, peopleList: this.PeopleList, gameModeOptions: gameModeOptions);
            int countRounds = 0;
            // TODO: make this an optional param and bring back minvalue 3 instead of having this invisible constraint.
            int roundTimeoutInSeconds = (int)gameModeOptions[4].ValueParsed;
            TimeSpan? roundTimeout = null;
            if (roundTimeoutInSeconds >= 3)
            {
                roundTimeout = TimeSpan.FromSeconds(roundTimeoutInSeconds);
            }

            GameState CreateGameplayGamestate()
            {
                GameState gameplay = new Gameplay_GS(
                        lobby: lobby,
                        setup_PeopleList: this.PeopleList,
                        roundTracker: RoundTracker,
                        gameModeOptions: gameModeOptions,
                        displayPool: (bool)gameModeOptions[2].ValueParsed,
                        displayNames: (bool)gameModeOptions[1].ValueParsed,
                        perRoundTimeoutDuration: roundTimeout);
                gameplay.Transition(CreateRevealAndScore);
                return gameplay;
            }
            GameState CreateRevealAndScore()
            {
                countRounds++;
                GameState displayPeople = new DisplayPeople_GS(
                    lobby: lobby,
                    title: "Here's What Everyone Made",
                    peopleList: RoundTracker.AssignedPeople.Values.ToList());

                GameState scoreBoard = new ScoreBoardGameState(
                    lobby: lobby);

                if (countRounds >= numRounds)
                {
                    GameState finalDisplay = new DisplayPeople_GS(
                        lobby: lobby,
                        title: "And Here's The Original People",
                        peopleList: this.PeopleList);
                    displayPeople.Transition(finalDisplay);
                    finalDisplay.Transition(scoreBoard);
                    scoreBoard.Transition(this.Exit);
                }
                else
                {
                    displayPeople.Transition(scoreBoard);
                    scoreBoard.Transition(CreateGameplayGamestate);
                }
                return displayPeople;
            }
            Setup.Transition(CreateGameplayGamestate);
            this.Entrance.Transition(Setup);
        }


        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // Empty
        }
    }
}
