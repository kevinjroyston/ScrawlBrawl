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
            public string LobbyCode { get; set; }
            public Guid LobbyId { get; set; }
            public TimeSpan ActiveDuration { get; set; }
        }
        public struct ConfigValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public List<Lobby> ActiveLobbies;
        public List<ConfigValue> CurrentConfigValues;
    }
}
