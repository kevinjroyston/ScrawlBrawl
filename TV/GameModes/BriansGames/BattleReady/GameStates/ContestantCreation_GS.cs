using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class ContestantCreation_GS : GameState
    {
        private Random Rand { get; set; } = new Random();
        private List<string> Prompts { get; set; }
        private List<PeopleUserDrawing> Drawings { get; set; }
        private RoundTracker roundTracker;
        public ContestantCreation_GS(Lobby lobby, List<PeopleUserDrawing> drawings, List<string> prompts, RoundTracker roundTracker, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            this.Drawings = drawings;
            this.Prompts = prompts;
            this.roundTracker = roundTracker;
        }

        private void AssignDrawingsAndPrompts()
        {
            roundTracker.ResetRoundVariables();
            List<PeopleUserDrawing> randomizedDrawings = Drawings.OrderBy(_ => Rand.Next()).ToList()
        }
    }
}
