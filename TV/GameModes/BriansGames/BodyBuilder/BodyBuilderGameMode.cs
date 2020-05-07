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
            Setup.AddStateEndingListener(() =>
            {
                for(int i =0; i< numRounds;i++)
                {
                    Gameplays.Add(new Gameplay_GS(
                        lobby: lobby,
                        setup_PeopleList: this.PeopleList,
                        roundTracker = roundTracker,
                        displayPool: gameModeOptions[1].RadioAnswer == 1,
                        displayNames: gameModeOptions[2].RadioAnswer == 1,
                        outlet: null));
                    Scoreboards.Add(new ScoreBoardGameState(
                        lobby: lobby,
                        outlet: this.Outlet));
                    DisplayPeoples.Add(new DisplayPeople_GS(
                        lobby: lobby,
                        roundTracker = roundTracker,
                        displayType: BodyBuilderConstants.DisplayTypes.PlayerHands,
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
                            roundTracker = roundTracker,
                            displayType: BodyBuilderConstants.DisplayTypes.OriginalPeople,
                            peopleList: this.PeopleList);

                        DisplayPeoples[i].Transition(finalDisplay.Transition(Scoreboards[i]));
                    }

                }
                Setup.Transition(this.Gameplays[0]);
            });
            this.EntranceState = Setup;
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
