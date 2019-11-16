using RoystonGame.Game.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game
{
    public class Game
    {
        public static Game Singleton { get; private set; }

        private List<User> UsersInGame { get; } 

        public Game()
        {
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
    }
}
