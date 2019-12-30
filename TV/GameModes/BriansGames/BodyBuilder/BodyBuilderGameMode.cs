using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder
{
    public class BodyBuilderGameMode : IGameMode
    {
        private List<Setup_Person> PeopleList { get; set; } = new List<Setup_Person>();
        private GameState Setup { get; set; }
        private GameState Gameplay { get; set; }
       
        private Random rand { get; } = new Random();
        public BodyBuilderGameMode()
        {
            Setup = new Setup_GS(this.PeopleList);
            Setup.SetStateEndingCallback(() =>
            {
                new Gameplay_GS(this.PeopleList, this.Outlet);
                Setup.Transition(this.Gameplay);
            });
            this.EntranceState = Setup;
        }
    }
}
