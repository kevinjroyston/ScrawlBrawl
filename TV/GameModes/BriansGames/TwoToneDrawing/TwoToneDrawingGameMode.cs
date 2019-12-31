using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private Random rand { get; } = new Random();
        public TwoToneDrawingGameMode()
        {
            Setup = new Setup_GS(this.SubChallenges);
            Setup.SetStateEndingCallback(() =>
            {
                int index = 0;
                foreach (ChallengeTracker challenge in SubChallenges.OrderBy(_ => rand.Next()))
                {
                    this.Gameplay.Add(new Gameplay_GS(challenge, null));
                    if (this.Scoreboards.Count > 0)
                    {
                        this.Scoreboards.Last()?.Transition(this.Gameplay.Last());
                    }
                    this.VoteReveals.Add(new VoteRevealed_GS(challenge));
                    this.Scoreboards.Add(new ScoreBoardGameState(null, null, title: Invariant($"{index+1}/{SubChallenges.Count}")));
                    this.Gameplay.Last().Transition(this.VoteReveals.Last());
                    this.VoteReveals.Last().Transition(this.Scoreboards.Last());
                    index++;
                }
                Setup.Transition(this.Gameplay[0]);
                this.Scoreboards.Last().SetOutlet(this.Outlet);
            });

            this.EntranceState = Setup;
        }
    }
}
