using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
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
        public static void Transition(this Outlet A, Inlet B)
        {
            A.SetOutlet(B);
        }
        public static void Transition(this Outlet A, Func<Inlet> B)
        {
            A.AddListener(() =>
            {
                A.Transition(B());
            });
        }
        public static void Transition(this StateOutlet A, Func<User, StateInlet> B)
        {
            A.Transition((User user, UserStateResult result, UserFormSubmission input) =>
            {
                // This call doesn't actually happen until after all prompts are submitted
                B(user).Inlet(user, result, input);
            });
        }
    }
}
