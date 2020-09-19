using RoystonGame.Web.DataModels.Enums;
using RoystonGameAutomatedTestingClient.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest("BodyBuilder_Struct1")]
    public class StructuredBodyBuildTest1 : ImposterTest, IStructuredTest
    {
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = 10,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = "3" }, // num rounds
                    new GameModeOptionRequest(){ Value = "2" }, //num prompts
                    new GameModeOptionRequest(){ Value = "4"}, // num drawings expected
                    new GameModeOptionRequest(){ Value = "2"}, // num players per prompt
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<IReadOnlyDictionary<UserPromptId, int>> UserPromptIdValidations => throw new NotImplementedException();
    }
}
