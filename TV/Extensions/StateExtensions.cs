using RoystonGame.TV.DataModels;
using System;
using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.Extensions
{
    /// <summary>
    /// Class containing extensions for the UserState object.
    /// </summary>
    public static class StateExtensions
    {
        public static void Transition(this StateOutlet A, Connector B)
        {
            A.SetOutlet(B);
        }
        public static void Transition(this StateOutlet A, StateInlet B)
        {
            A.Transition(B.Inlet);
        }
        public static void Transition(this StateOutlet A, Func<StateInlet> B )
        {
            A.Transition(() => B().Inlet);
        }     
        public static void Transition(this StateOutlet A, Func<Connector> B)
        {
            A.AddStateEndingListener(() =>
            {
                A.Transition(B());
            });
        }
    }
}
