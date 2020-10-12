using System.Collections.Generic;

namespace Common.DataModels.Requests
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
