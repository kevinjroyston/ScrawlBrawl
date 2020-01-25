using RoystonGame.TV.DataModels;

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
    }
}
