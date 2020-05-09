using RoystonGame.TV.DataModels;
using System;

namespace RoystonGame.TV.Extensions
{
    /// <summary>
    /// Class containing extensions for the UserState object.
    /// </summary>
    public static class StateExtensions
    {
        public static void Transition(this StateOutlet A, StateInlet B)
        {
            A.SetOutlet(B.Inlet);
        }
        public static void Transition(this StateOutlet A, Func<StateInlet> B )
        {
            A.AddStateEndingListener(() =>
            {
                A.Transition(B());
            });
        }
    }
}
