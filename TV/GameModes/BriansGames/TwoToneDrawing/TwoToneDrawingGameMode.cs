using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;

using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing
{
    public class TwoToneDrawingGameMode : IGameMode
    {
        private List<ChallengeTracker> SubChallenges { get; set; } = new List<ChallengeTracker>();
        private GameState Setup { get; set; }
        private List<GameState> Gameplay { get; set; } = new List<GameState>();
        private List<GameState> Scoreboards { get; set; } = new List<GameState>();
        private List<GameState> VoteReveals { get; set; } = new List<GameState>();
        private Random Rand { get; } = new Random();
        public TwoToneDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);

            //ToDo Refactor to move to vote reveal after the votes are done and scrambled 
            Setup = new Setup_GS(lobby, this.SubChallenges, gameModeOptions);
            Setup.Transition(() =>
            {
                int index = 0;
                foreach (ChallengeTracker challenge in SubChallenges.OrderBy(_ => Rand.Next()))
                {
                    this.Gameplay.Add(new Gameplay_GS(lobby, challenge));
                    if (this.Scoreboards.Count > 0)
                    {
                        this.Scoreboards.Last()?.Transition(this.Gameplay.Last());
                    }
                    this.VoteReveals.Add(new VoteRevealed_GS(lobby, challenge));
                    this.Scoreboards.Add(new ScoreBoardGameState(lobby, title: Invariant($"{index + 1}/{SubChallenges.Count}")));
                    this.Gameplay.Last().Transition(this.VoteReveals.Last());
                    this.VoteReveals.Last().Transition(this.Scoreboards.Last());
                    index++;
                }
                this.Scoreboards.Last().Transition(this.Exit);
                return this.Gameplay[0];
            });

            this.Entrance.Transition(Setup);
        }

        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // None
        }
    }
}
