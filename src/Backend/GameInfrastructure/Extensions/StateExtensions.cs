using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using System;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows;
using System.Linq;

namespace Backend.GameInfrastructure.Extensions
{
    /// <summary>
    /// Class containing extensions for the UserState object.
    /// </summary>
    public static class StateExtensions
    {
        public static void Transition(this IOutlet A, IInlet B)
        {
            A.SetOutlet(B);
        }
        public static void Transition(this IOutlet A, Func<IInlet> B)
        {
            A.SetOutlet(B);
        }
        /// <summary>
        /// Transitions from an outlet to a list of states, stops after first object that does not implement IOutlet,
        /// for example: this.Exit
        /// </summary>
        /// <param name="A">The outlet being transitioned from</param>
        /// <param name="NextStates">The Inlets to transition to, the functions are called as late as possible. Note, must implement IInlet, optionally IOutlet</param>
        public static void Transition(this IOutlet A, params Func<dynamic>[] NextStates)
        {
            if (NextStates.Length <= 0)
            {
                return;
            }
            A.SetOutlet(() =>
            {
                dynamic B = NextStates[0]();
                IInlet BIn = B as IInlet;
                IOutlet BOut = B as IOutlet;
                if (BIn == null)
                {
                    throw new Exception("Lambda must return an object implementing IInlet");
                }
                if (BOut != null)
                {
                    BOut.Transition(NextStates[1..]);
                }
                else
                {
                    if (NextStates.Length > 1)
                    {
                        throw new Exception("Object not implementing IOutlet was not the last element");
                    }
                }

                return BIn;
            });
        }
        public static void Transition(this StateOutlet A, Func<User, IInlet> B)
        {
            A.Transition(new InletConnector((User user, UserStateResult result, UserFormSubmission input) =>
            {
                // This call doesn't actually happen until after all prompts are submitted
                B(user).Inlet(user, result, input);
            }));
        }
    }
}
