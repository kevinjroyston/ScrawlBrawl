using System;

namespace RoystonGame.Web.DataModels.Exceptions
{
    /// <summary>
    /// These exceptions will be caught and shown to the user.
    /// </summary>
    public class GameModeInstantiationException : Exception
    {
        public GameModeInstantiationException(string message = null) : base(message)
        {
            // Empty
        }
    }
}
