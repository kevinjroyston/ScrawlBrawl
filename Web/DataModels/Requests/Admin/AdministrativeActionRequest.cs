using System.Collections.Generic;

namespace RoystonGame.Web.DataModels.Requests
{
    public class AdministrativeActionRequest
    {
        /// <summary>
        /// List of Users to apply the action to.
        /// </summary>
        public List<string> Users { get; set; }

        /// <summary>
        /// List of Lobbies to apply the action to.
        /// </summary>
        public List<string> Lobbies { get; set; }
    }
}
