using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV
{
    public class LobbyManager
    {
        public LobbyManager(RunGame gameRunner)
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                throw new Exception("Tried to instantiate a second instance of GameManager.cs");
            }

            this.GameRunner = gameRunner;
        }
    }
}
