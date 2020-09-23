using McMaster.Extensions.CommandLineUtils;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGameAutomatedTestingClient.TestFramework;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using static RoystonGameAutomatedTestingClient.TestFramework.TestRunner;
using RoystonGame.Web.DataModels;
using RoystonGame.Web.Controllers.LobbyManagement;
using RoystonGame.Web.DataModels.Requests;
using System.Net.Http;
using System.Threading.Tasks;
using RoystonGameAutomatedTestingClient.WebClient;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

namespace RoystonGameAutomatedTestingClient.DataModels
{
    public class Lobby
    {
        public string Id { get; private set; }
        private List<LobbyPlayer> InternalPlayers { get; }
        public IReadOnlyList<LobbyPlayer> Players { get { return this.InternalPlayers; } }
        public LobbyOwner Owner { get; }

        public Lobby()
        {
            this.Owner = new LobbyOwner();
            this.InternalPlayers = new List<LobbyPlayer>();
        }
        public Lobby(string lobbyId)
        {
            this.Id = lobbyId;
            this.Owner = new LobbyOwner();
            this.InternalPlayers = new List<LobbyPlayer>();
        }

        public async Task Create()
        {
            string lobbyId = await CommonSubmissions.MakeLobby(Owner.UserId);
            Id = lobbyId;
        }

        public async Task Delete()
        {
            await CommonSubmissions.DeleteLobby(Owner.UserId);
        }

        public async Task Populate(int numPlayers)
        {
            for (int i = 0; i < numPlayers; i++)
            {
                LobbyPlayer newPlayer = new LobbyPlayer();
                this.InternalPlayers.Add(newPlayer);
                await CommonSubmissions.JoinLobby(
                    userId: newPlayer.UserId,
                    lobbyId: Id,
                    name: "TestUser" + i);
                Console.WriteLine($"Player {newPlayer.UserId} joined lobby");
                Thread.Sleep(Math.Clamp(500 - 5 * i, 1, 100));
            }
        }

        public async Task Configure(IEnumerable<GameModeOptionRequest> options, int GameMode)
        {
            ConfigureLobbyRequest configLobby = new ConfigureLobbyRequest()
            {
                GameMode = GameMode,
                Options = options.ToList()
            };
            await CommonSubmissions.ConfigureLobby(configLobby, Owner.UserId);
        }
    }

    public class LobbyPlayer
    {
        public string UserId { get; } = Helpers.GenerateRandomId();
        public bool Owner { get; protected set; } = false;

        public LobbyPlayer()
        {
            // Empty
        }
    }

    public class LobbyOwner : LobbyPlayer
    {
        public LobbyOwner() : base()
        {
            this.Owner = true;
        }
    }
}