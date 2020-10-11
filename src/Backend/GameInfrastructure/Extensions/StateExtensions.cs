using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using System;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows;

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
            A.AddExitListener(() =>
            {
                A.Transition(B());
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
