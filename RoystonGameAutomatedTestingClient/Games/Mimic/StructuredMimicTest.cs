using RoystonGame.Web.DataModels.Enums;
using RoystonGameAutomatedTestingClient.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest("Mimic_Struct1")]
    public class StructuredMimicTest1 : MimicTest, IStructuredTest
    {
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = 10,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = "2" }, // num starting drawings
                    new GameModeOptionRequest(){ Value = "5" }, // num drawings before vote
                    new GameModeOptionRequest(){ Value = "3" }, // num sets
                    new GameModeOptionRequest(){ Value = "10"}, // max for vote
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<IReadOnlyDictionary<UserPromptId, int>> UserPromptIdValidations => throw new NotImplementedException();
    }
}
