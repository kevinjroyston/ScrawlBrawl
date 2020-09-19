using RoystonGame.Web.DataModels.Enums;
using RoystonGameAutomatedTestingClient.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest("BodySwap_Struct1")]
    public class StructuredBodySwapTest1 : BodySwapTest, IStructuredTest
    {
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = 10,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = "2" }, // num rounds
                    new GameModeOptionRequest(){ Value = "true" }, //show names
                    new GameModeOptionRequest(){ Value = "false"}, // show images
                    new GameModeOptionRequest(){ Value = "25"}, //num turns before timeout
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<IReadOnlyDictionary<UserPromptId, int>> UserPromptIdValidations => throw new NotImplementedException();
    }
}
