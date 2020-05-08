using RoystonGame.TV.DataModels;
using System;

namespace RoystonGame.TV.Extensions
{
    /// <summary>
    /// Class containing extensions for the UserState object.
    /// </summary>
    public static class StateExtensions
    {
        public static State Transition(this StateOutlet A, State B)
        {
            A.SetOutlet(B.Inlet);
            return B;
        }
        public static void Transition(this StateOutlet A, Func<State> B )
        {
            A.AddStateEndingListener(() =>
            {
                A.Transition(B());
            });
        }
    }
}
