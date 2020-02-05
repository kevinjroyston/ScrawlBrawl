using RoystonGame.TV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
