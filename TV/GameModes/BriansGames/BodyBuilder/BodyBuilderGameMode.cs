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
                        displayPool: gameModeOptions[1].RadioAnswer == 1,
                        displayNames: gameModeOptions[2].RadioAnswer == 1);
                gameplay.Transition(CreateRevealAndScore);
                return gameplay;
            }
            GameState CreateRevealAndScore()
            {
                countRounds++;
                GameState displayPeople = new DisplayPeople_GS(
                    lobby: lobby,
                    title: "Here's What Everyone Made",
                    peopleList: roundTracker.UnassignedPeople);

                GameState scoreBoard = new ScoreBoardGameState(
                    lobby: lobby);

                if (countRounds >= numRounds)
                {
                    GameState finalDisplay = new DisplayPeople_GS(
                        lobby: lobby,
                        title: "And Here's The Original Peoeple",
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
            /*Func<GameState, Action> getListener = null; //returns a listener function for who called it
            getListener = (transitionFromGS) => 
            {
                return () =>
                {
                    GameState gameplay = new Gameplay_GS(
                        lobby: lobby,
                        setup_PeopleList: this.PeopleList,
                        roundTracker: roundTracker,
                        displayPool: gameModeOptions[1].RadioAnswer == 1,
                        displayNames: gameModeOptions[2].RadioAnswer == 1);

                    gameplay.AddStateEndingListener(() =>
                    {
                        countRounds++;
                        GameState displayPeople = new DisplayPeople_GS(
                            lobby: lobby,
                            roundTracker: roundTracker,
                            displayType: ThreePartPeopleConstants.DisplayTypes.PlayerHands,
                            peopleList: this.PeopleList);

                        GameState scoreBoard = new ScoreBoardGameState(
                            lobby: lobby);

                        if (countRounds >= numRounds)
                        {
                            GameState finalDisplay = new DisplayPeople_GS(
                                lobby: lobby,
                                roundTracker: roundTracker,
                                displayType: ThreePartPeopleConstants.DisplayTypes.OriginalPeople,
                                peopleList: this.PeopleList);
                            gameplay.Transition(displayPeople);
                            displayPeople.Transition(finalDisplay);
                            finalDisplay.Transition(scoreBoard);
                            scoreBoard.SetOutlet(this.Outlet);
                        }
                        else
                        {
                            gameplay.Transition(displayPeople);
                            displayPeople.Transition(scoreBoard);
                            scoreBoard.AddStateEndingListener(getListener(scoreBoard));
                        }
                    });

                    transitionFromGS.Transition(gameplay);
                };
            };
            Setup.AddStateEndingListener(getListener(Setup));
            this.EntranceState = Setup;*/

            /*for(int i =0; i< numRounds;i++)
            {
                Gameplays.Add(new Gameplay_GS(
                    lobby: lobby,
                    setup_PeopleList: this.PeopleList,
                    roundTracker: roundTracker,
                    displayPool: gameModeOptions[1].RadioAnswer == 1,
                    displayNames: gameModeOptions[2].RadioAnswer == 1));
                Scoreboards.Add(new ScoreBoardGameState(
                    lobby: lobby));
                DisplayPeoples.Add(new DisplayPeople_GS(
                    lobby: lobby,
                    roundTracker: roundTracker,
                    displayType: ThreePartPeopleConstants.DisplayTypes.PlayerHands,
                    peopleList: this.PeopleList));

                Gameplays[i].Transition(DisplayPeoples[i].Transition(Scoreboards[i]));
                if (i > 0)
                {
                    Scoreboards[i - 1].Transition(Gameplays[i]);
                }
                if (i == numRounds-1)
                {
                    DisplayPeople_GS finalDisplay = new DisplayPeople_GS(
                        lobby: lobby,
                        roundTracker: roundTracker,
                        displayType: ThreePartPeopleConstants.DisplayTypes.OriginalPeople,
                        peopleList: this.PeopleList);

                    DisplayPeoples[i].Transition(finalDisplay.Transition(Scoreboards[i]));
                }

            }
            Setup.Transition(this.Gameplays[0]);*/

        }


        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            if (!int.TryParse(gameModeOptions[0].ShortAnswer, out int parsedInteger)
                || parsedInteger < 1 || parsedInteger > 30)
            {
                throw new GameModeInstantiationException("Must be an integer from 1-30");
            }
        }
    }
}
