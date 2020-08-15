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
using RoystonGame.TV.GameModes.Common;

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

            int numRounds = (int)gameModeOptions[(int)GameModeOptionsEnum.numRounds].ValueParsed;
            int turnsForTimeout = (int)gameModeOptions[(int)GameModeOptionsEnum.turnsForTimeout].ValueParsed;
            bool displayNames = (bool)gameModeOptions[(int)GameModeOptionsEnum.displayNames].ValueParsed;
            bool displayImages = (bool)gameModeOptions[(int)GameModeOptionsEnum.displayImages].ValueParsed;
            int gameSpeed = (int)gameModeOptions[(int)GameModeOptionsEnum.gameSpeed].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? roundTimer = null;
            if (gameSpeed > 0)
            {
                setupTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BodyBuilderConstants.SetupTimerMin,
                    aveTimerLength: BodyBuilderConstants.SetupTimerAve,
                    maxTimerLength: BodyBuilderConstants.SetupTimerMax);
                drawingTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BodyBuilderConstants.DrawingTimerMin,
                    aveTimerLength: BodyBuilderConstants.DrawingTimerAve,
                    maxTimerLength: BodyBuilderConstants.DrawingTimerMax);
                roundTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BodyBuilderConstants.RoundTimerMin,
                    aveTimerLength: BodyBuilderConstants.RoundTimerAve,
                    maxTimerLength: BodyBuilderConstants.RoundTimerMax);
            }

            Setup = new Setup_GS(lobby: lobby, peopleList: this.PeopleList, setupTimeDurration: setupTimer);
            int countRounds = 0;

            GameState CreateGameplayGamestate()
            {
                GameState gameplay = new Gameplay_GS(
                        lobby: lobby,
                        setup_PeopleList: this.PeopleList,
                        roundTracker: RoundTracker,
                        roundTimeoutLimit: turnsForTimeout,
                        displayPool: (bool)gameModeOptions[2].ValueParsed,
                        displayNames: (bool)gameModeOptions[1].ValueParsed,
                        perRoundTimeoutDuration: roundTimer);
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
