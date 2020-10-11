namespace BackendAutomatedTestingClient.Games
{
    // TODO: Testing this game will require more specialized logic.

    /*[EndToEndGameTest("BodySwap_Struct1")]
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
    }*/
}
