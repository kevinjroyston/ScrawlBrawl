using RoystonGame.Web.DataModels.Enums;
using RoystonGameAutomatedTestingClient.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest("TwoTone_Struct1")]
    public class StructuredTwoToneTest1 : ImposterTest, IStructuredTest
    {
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = 10,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = "2" }, // max num colors
                    new GameModeOptionRequest(){ Value = "4" }, // max num teams per prompt
                    new GameModeOptionRequest(){ Value = "true"}, // show other colors
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<IReadOnlyDictionary<UserPromptId, int>> UserPromptIdValidations => throw new NotImplementedException();
    }
}
