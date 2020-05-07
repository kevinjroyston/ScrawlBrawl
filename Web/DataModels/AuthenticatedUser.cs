using RoystonGame.TV;

namespace RoystonGame.Web.DataModels
{
    /// <summary>
    /// This class represents an authenticated user.
    /// </summary>
    public class AuthenticatedUser
    {
        public string UserId { get; set; }
        public Lobby OwnedLobby { get; set; }
    }
}
