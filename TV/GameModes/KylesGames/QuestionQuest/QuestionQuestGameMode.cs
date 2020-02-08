using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KylesGames.QuestionQuest.DataModels;
using RoystonGame.TV.GameModes.KylesGames.QuestionQuest.GameStates;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using static System.FormattableString;

namespace RoystonGame.TV.GameModes.KylesGames.QuestionQuest
{
    public class QuestionQuestGameMode : IGameMode
    {
        private ConcurrentDictionary<User, ChallengeTracker> SubChallenges { get; set; } = new ConcurrentDictionary<User, ChallengeTracker>();

        private GameState Setup { get; set; }
        private List<GameState> Gameplay { get; set; } = new List<GameState>();
        private List<GameState> Scoreboards { get; set; } = new List<GameState>();
        private List<GameState> Reveals { get; set; } = new List<GameState>();
        private Random rand { get; } = new Random();
        public QuestionQuestGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);

            Setup = new Setup_GS(lobby, this.SubChallenges);
            Setup.AddStateEndingListener(() =>
            {
                int index = 0;
                foreach (ChallengeTracker challenge in SubChallenges.Values.OrderBy(_ => rand.Next()))
                {
                    this.Gameplay.Add(new FindTheAchilles_GS(lobby, challenge, null));
                    if (this.Scoreboards.Count > 0)
                    {
                        // I don't think the elvis operator is needed below.
                        this.Scoreboards.Last()?.Transition(this.Gameplay.Last());
                    }
                    this.Reveals.Add(new RevealTheAchilles_GS(lobby, challenge));
                    this.Scoreboards.Add(new ScoreBoardGameState(lobby, null, null, title: Invariant($"{index + 1}/{SubChallenges.Count}")));
                    this.Gameplay.Last().Transition(this.Reveals.Last());
                    this.Reveals.Last().Transition(this.Scoreboards.Last());
                    index++;
                }
                Setup.Transition(this.Gameplay[0]);
                this.Scoreboards.Last().SetOutlet(this.Outlet);
            });

            this.EntranceState = Setup;
        }

        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // TODO
        }
    }
}
