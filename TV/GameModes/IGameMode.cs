
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes
{
    /// <summary>
    /// A state has an inlet and outlet.
    /// </summary>
    public abstract class IGameMode : StateOutlet
    {
        /// <summary>
        /// Gets the first GameState of the GameMode.
        /// </summary>
        public GameState EntranceState { get; protected set; }
    }
}
