using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows;

namespace RoystonGame.TV.Extensions
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
