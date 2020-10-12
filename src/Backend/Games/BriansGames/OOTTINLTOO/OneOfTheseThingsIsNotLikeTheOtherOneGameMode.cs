using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.OOTTINLTOO.DataModels;
using Backend.Games.BriansGames.OOTTINLTOO.GameStates;
using Backend.Games.Common.GameStates;
using Common.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure;

using static System.FormattableString;

namespace Backend.Games.BriansGames.OOTTINLTOO
{
    public class OneOfTheseThingsIsNotLikeTheOtherOneGameMode : IGameMode
    {
        private List<ChallengeTracker> SubChallenges { get; set; } = new List<ChallengeTracker>();

        private GameState Setup { get; set; }
        private List<GameState> Gameplay { get; set; } = new List<GameState>();
        private List<GameState> Scoreboards { get; set; } = new List<GameState>();
        private List<GameState> Reveals { get; set; } = new List<GameState>();
        private Random Rand { get; } = new Random();
        public OneOfTheseThingsIsNotLikeTheOtherOneGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);

            Setup = new Setup_GS(lobby, this.SubChallenges, (int)gameModeOptions[0].ValueParsed);
            Setup.Transition(() =>
            {
                int index = 0;
                foreach (ChallengeTracker challenge in SubChallenges.OrderBy(_ => Rand.Next()))
                {
                    this.Gameplay.Add(new Gameplay_GS(lobby, challenge));
                    if (this.Scoreboards.Count > 0)
                    {
                        // I don't think the elvis operator is needed below.
                        this.Scoreboards.Last()?.Transition(this.Gameplay.Last());
                    }
                    this.Reveals.Add(new ImposterRevealed_GS(lobby, challenge));
                    this.Scoreboards.Add(new ScoreBoardGameState(lobby, title: Invariant($"{index + 1}/{SubChallenges.Count}")));
                    this.Gameplay.Last().Transition(this.Reveals.Last());
                    this.Reveals.Last().Transition(this.Scoreboards.Last());
                    index++;
                }
                this.Scoreboards.Last().Transition(this.Exit);
                return this.Gameplay[0];
            });

            this.Entrance.Transition(Setup);
        }

        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // Empty
        }
    }
}
