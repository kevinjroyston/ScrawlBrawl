using RoystonGame.Game.ControlFlows;
using RoystonGame.Game.DataModels;
using RoystonGame.Game.DataModels.UserStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game.Extensions
{
    /// <summary>
    /// Class containing extensions for the UserState object.
    /// </summary>
    public static class UserStateExtensions
    {
        /// <summary>
        /// Links a <paramref name="userState"/> to a target UserState <paramref name="transitionTo"/> via Transition <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The transition to use, the transition MUST define constructor(UserState transitionTo).</typeparam>
        /// <param name="userState">The user state to transition out of.</param>
        /// <param name="transitionTo">The user state to transition into.</param>
        /// <returns><paramref name="transitionTo"/> unmodified.</returns>
        public static UserState Transition<T>(this UserState userState, UserState transitionTo) where T : UserStateTransition
        {
            // Existence of constructor not checked at compile time.
            T transition = (T)Activator.CreateInstance(typeof(T), transitionTo);
            transition.AddUsersToTransition(Game.Singleton.GetActiveUsers());
            userState.SetStateCompletedCallback(transition.Inlet);

            return transitionTo;
        }

    }
}
