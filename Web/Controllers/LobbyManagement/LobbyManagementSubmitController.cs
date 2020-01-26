using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Controllers.LobbyManagement
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Users")]
    public class LobbyManagementSubmitController
    {
        /*
        public Temp()
        {
            string errorMsg = "Lobby not found";

            // TODO: move to a different lobby creation model.
            // TODO: add way to leave a lobby.
            if (!Singleton.LobbyCodeToLobbies.ContainsKey(lobbyCode))
            {
                Lobby newLobby = new Lobby(lobbyCode);

                if (!Singleton.LobbyCodeToLobbies.TryAdd(lobbyCode, newLobby)
                    || !Singleton.LobbyIdToLobbies.TryAdd(newLobby.LobbyId, newLobby))
                {
                    bool success = Singleton.LobbyCodeToLobbies.TryRemove(lobbyCode, out Lobby _);
                    success &= Singleton.LobbyIdToLobbies.TryRemove(newLobby.LobbyId, out Lobby _);
                    if (!success)
                    {
                        throw new Exception("Lobby lists corrupted");
                    }
                    return (false, "Unexpected error occurred while creating lobby. Refresh and try again.");
                }
            }
        }*/
    }
}
