using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.BodyBuilder.DataModels;
using Backend.Games.BriansGames.BodyBuilder.GameStates;
using Common.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using System.Linq;
using Backend.Games.BriansGames.Common.GameStates;
using Backend.Games.Common.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using System;
using Backend.Games.Common;
using Backend.GameInfrastructure;
using Common.DataModels.Responses;

namespace Backend.Games.BriansGames.BodyBuilder
{
    public class BodyBuilderGameMode : IGameMode
    {
        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Body Swap", // in code references as Body Builder
            Description = "Try to make a complete character before your opponents can.",
            MinPlayers = 3,
            MaxPlayers = null,
            Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of rounds",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 10,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Display names on screen",
                        DefaultValue = true,
                        ResponseType = ResponseType.Boolean
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Display images on screen",
                        DefaultValue = false,
                        ResponseType = ResponseType.Boolean
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of turns for round timeout",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 25,
                        MinValue = 1,
                        MaxValue = 100,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Length of the game (10 for longest 1 for shortest 0 for no timer)",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 0,
                        MaxValue = 10,
                    }
                }
        };

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
            int gameLength = (int)gameModeOptions[(int)GameModeOptionsEnum.GameLength].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? roundTimer = null;
            if (gameLength > 0)
            {
                setupTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: BodyBuilderConstants.SetupTimerMin,
                    aveTimerLength: BodyBuilderConstants.SetupTimerAve,
                    maxTimerLength: BodyBuilderConstants.SetupTimerMax);
                drawingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: BodyBuilderConstants.DrawingTimerMin,
                    aveTimerLength: BodyBuilderConstants.DrawingTimerAve,
                    maxTimerLength: BodyBuilderConstants.DrawingTimerMax);
                roundTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: BodyBuilderConstants.RoundTimerMin,
                    aveTimerLength: BodyBuilderConstants.RoundTimerAve,
                    maxTimerLength: BodyBuilderConstants.RoundTimerMax);
            }

            Setup = new Setup_GS(lobby: lobby, peopleList: this.PeopleList, setupTimeDuration: setupTimer);
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
