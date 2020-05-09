using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.TV.GameModes.BriansGames.Common.GameStates;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.TV.GameModes.Common.GameStates;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder
{
    public class BodyBuilderGameMode : IGameMode
    {
        private List<Setup_Person> PeopleList { get; set; } = new List<Setup_Person>();
        private GameState Setup { get; set; }
        private List<GameState> Gameplays { get; set; } = new List<GameState>();
        private List<GameState> Scoreboards { get; set; } = new List<GameState>();
        private List<GameState> DisplayPeoples { get; set; } = new List<GameState>();
        private RoundTracker roundTracker = new RoundTracker();
        public BodyBuilderGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);

            int numRounds = int.Parse(gameModeOptions[0].ShortAnswer);
            Setup = new Setup_GS(lobby: lobby, peopleList: this.PeopleList, gameModeOptions: gameModeOptions);
            int countRounds = 0;

            GameState CreateGameplayGamestate()
            {
                GameState gameplay = new Gameplay_GS(
                        lobby: lobby,
                        setup_PeopleList: this.PeopleList,
                        roundTracker: roundTracker,
                        gameModeOptions: gameModeOptions,
                        displayPool: gameModeOptions[2].RadioAnswer == 1,
                        displayNames: gameModeOptions[1].RadioAnswer == 1);
                gameplay.Transition(CreateRevealAndScore);
                return gameplay;
            }
            GameState CreateRevealAndScore()
            {
                countRounds++;
                GameState displayPeople = new DisplayPeople_GS(
                    lobby: lobby,
                    title: "Here's What Everyone Made",
                    peopleList: roundTracker.AssignedPeople.Values.ToList());

                GameState scoreBoard = new ScoreBoardGameState(
                    lobby: lobby);

                if (countRounds >= numRounds)
                {
                    GameState finalDisplay = new DisplayPeople_GS(
                        lobby: lobby,
                        title: "And Here's The Original people",
                        peopleList: this.PeopleList);
                    displayPeople.Transition(finalDisplay);
                    finalDisplay.Transition(scoreBoard);
                    scoreBoard.SetOutlet(this.Outlet);
                }
                else
                {
                    displayPeople.Transition(scoreBoard);
                    scoreBoard.Transition(CreateGameplayGamestate);
                }
                return displayPeople;
            }
            Setup.Transition(CreateGameplayGamestate);
            this.EntranceState = Setup;
        }


        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            if (!int.TryParse(gameModeOptions[0].ShortAnswer, out int parsedInteger1)
                || parsedInteger1 < 1 || parsedInteger1 > 10)
            {
                throw new GameModeInstantiationException("Must be an integer from 1-30");
            }
            if (!int.TryParse(gameModeOptions[3].ShortAnswer, out int parsedInteger2)
                || parsedInteger2 < 1 || parsedInteger2 > 100)
            {
                throw new GameModeInstantiationException("Must be an integer from 1-100");
            }
        }
    }
}
