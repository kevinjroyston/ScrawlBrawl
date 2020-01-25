using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.GameStates;

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
