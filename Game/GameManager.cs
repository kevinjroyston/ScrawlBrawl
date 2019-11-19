using RoystonGame.Game.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game
{
    public class GameManager
    {
        public static GameManager Singleton { get; private set; }

        private List<User> UsersInGame { get; }
        private List<User> UnregisteredUsers { get; }

        public GameManager()
        {
            //TODO: move logic into Singleton getter. Make constructor private
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                throw new Exception("Tried to instantiate a second instance of Game.cs");
            }

            this.UsersInGame = new List<User>();
        }

        public IReadOnlyList<User> GetActiveUsers()
        {
            return UsersInGame.AsReadOnly();
        }

        public void RegisterUser(User user, string displayName, string selfPortrait)
        {
            //todo parameter validation
            if (!this.UnregisteredUsers.Contains(user))
            {
                throw new Exception("Tried to register unknown user");
            }

            user.DisplayName = displayName;
            user.SelfPortrait = selfPortrait;
            this.UsersInGame.Add(user);
            this.UnregisteredUsers.Remove(user);
        }
    }
}
