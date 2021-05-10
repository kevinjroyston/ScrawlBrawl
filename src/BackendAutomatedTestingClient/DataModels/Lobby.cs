using Common.DataModels.Requests.LobbyManagement;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using System.Threading.Tasks;
using BackendAutomatedTestingClient.WebClient;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

namespace BackendAutomatedTestingClient.DataModels
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
            this.InternalPlayers.Add(this.Owner); // Lobby owner will be added on lobby creation.
        }
        public Lobby(string lobbyId)
        {
            this.Id = lobbyId;
            this.Owner = new LobbyOwner();
            this.InternalPlayers = new List<LobbyPlayer>();
            this.InternalPlayers.Add(this.Owner); // Lobby owner will be added on lobby creation.
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
            for (int i = 0; i < numPlayers-1; i++)
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

        public async Task Configure(IEnumerable<GameModeOptionRequest> options, StandardGameModeOptions standardOptions, int GameMode)
        {
            ConfigureLobbyRequest configLobby = new ConfigureLobbyRequest()
            {
                GameMode = GameMode,
                Options = options.ToList()
            };
            await CommonSubmissions.ConfigureLobby(configLobby, standardOptions, Owner.UserId);
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