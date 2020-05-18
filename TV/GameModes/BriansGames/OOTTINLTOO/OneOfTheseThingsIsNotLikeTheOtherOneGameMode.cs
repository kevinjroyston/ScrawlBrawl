﻿using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;

using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO
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

            Setup = new Setup_GS(lobby, this.SubChallenges, int.Parse(gameModeOptions[0].ShortAnswer));
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
            if (!int.TryParse(gameModeOptions[0].ShortAnswer, out int parsedInteger))
            {
                throw new GameModeInstantiationException("Could not parse input as integer");
            }

            if (parsedInteger < 3 || parsedInteger > lobby.GetAllUsers().Count - 1)
            {
                throw new GameModeInstantiationException(Invariant($"Invalid number of drawings per prompt pair, must be between ({3}) and ({lobby.GetAllUsers().Count - 1})"));
            }
        }
    }
}
