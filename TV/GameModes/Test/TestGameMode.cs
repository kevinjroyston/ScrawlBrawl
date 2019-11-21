using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Test
{
    public class TestGameMode : IGameMode
    {

        public TestGameMode()
        {
        }

        public GameState EntranceState => throw new NotImplementedException();

        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            throw new NotImplementedException();
        }

        public void SetOutlet(Action<User, UserStateResult, UserFormSubmission> outlet)
        {
            throw new NotImplementedException();
        }
    }
}
