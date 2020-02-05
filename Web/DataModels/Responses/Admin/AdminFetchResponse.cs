using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.Responses
{
    /// <summary>
    /// Class containing response payload containing senstive data for Admin view.
    /// </summary>
    public class AdminFetchResponse
    {
        public struct Lobby
        {
            public string LobbyOwner { get; set; }
            public string LobbyId { get; set; }
            public TimeSpan ActiveDuration { get; set; }
        }
        public struct User
        {
            public string UserIP { get; set; }
            public string DisplayName { get; set; }
            public string LobbyId { get; set; }
            public TimeSpan ActiveDuration { get; set; }
        }
        public List<Lobby> ActiveLobbies;
        public List<User> ActiveUsers;
    }
}
