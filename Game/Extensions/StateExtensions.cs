using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.Extensions
{
    /// <summary>
    /// Class containing extensions for the UserState object.
    /// </summary>
    public static class StateExtensions
    {
        /// <summary>
        /// Links a <paramref name="fromState"/> to a target <paramref name="toState"/> via Transition <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The transition to use, the transition MUST define constructor(UserState transitionTo).</typeparam>
        /// <param name="fromState">The state to transition out of.</param>
        /// <param name="toState">The state to transition into.</param>
        /// <returns><paramref name="transitionTo"/> unmodified.</returns>
        /// <remarks>Don't assign the result of this operation to anything! The response is purely meant for chaining.</remarks>
        public static State Transition<T>(this State A, State C) where T : UserStateTransition, new()
        {
            // Existence of constructor not checked at compile time.
            T B = new T();
            B.AddUsersToTransition(GameManager.GetActiveUsers());

            B.SetOutlet(C.Inlet);
            A.SetOutlet(B.Inlet);

            // Return C for easy chaining. Careful with references.
            return C;
        }
        /// <summary>
        /// Links a <paramref name="fromState"/> to a target <paramref name="toState"/> via Transition <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The transition to use, the transition MUST define constructor(UserState transitionTo).</typeparam>
        /// <param name="fromState">The state to transition out of.</param>
        /// <param name="toState">The state to transition into.</param>
        /// <returns><paramref name="transitionTo"/> unmodified.</returns>
        public static GameState Transition<T>(this GameState A, GameState C) where T : UserStateTransition, new()
        {
            // Existence of constructor not checked at compile time.
            T B = new T();
            B.AddUsersToTransition(GameManager.GetActiveUsers());

            B.SetOutlet(C.Inlet);
            A.SetOutlet((User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                // Prior to sending the first user to gamestate C we must enter the game state.
                GameManager.TransitionCurrentGameState(C);
                B.Inlet(user, result, userInput);
            });

            // Return C for easy chaining. Careful with references.
            return C;
        }
    }
}
