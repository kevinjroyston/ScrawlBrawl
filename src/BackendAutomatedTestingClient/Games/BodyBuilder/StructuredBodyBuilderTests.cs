using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;


namespace BackendAutomatedTestingClient.Games
{
    public abstract class StructuredBodyBuilderTest : BodyBuilderTest, IStructuredTest
    {
        // Test might not work with some param combos.
        protected abstract int NumPlayers { get; }
        protected abstract int NumDrawings { get; }
        protected abstract int NumPlayersPerPrompt { get; }
        protected abstract int NumPromptsPerUserPerRound { get; }
        protected abstract int NumRounds { get; }
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = NumRounds + "" }, // num rounds
                    new GameModeOptionRequest(){ Value = NumPromptsPerUserPerRound + "" }, //num prompts per user per round
                    new GameModeOptionRequest(){ Value = NumDrawings + ""}, // num drawings expected
                    new GameModeOptionRequest(){ Value = NumPlayersPerPrompt + ""}, // num players per prompt
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>();

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_BodyPartDrawing),
                    },
                    repeatCounter: NumDrawings);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_BattlePrompts),
                    },
                    repeatCounter: NumRounds * (int)Math.Ceiling(1.0*NumPromptsPerUserPerRound / NumPlayersPerPrompt));

                var round = new List<GameStep>();
                round.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_ContestantCreation),
                    },
                    repeatCounter: NumPromptsPerUserPerRound);

                round.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns),
                    },
                    repeatCounter: NumPlayers);
                round.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.Waiting));
                round.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns));

                toReturn.AppendRepetitiveGameSteps(round, NumRounds);

                return toReturn;
            }
        }
    }

    [EndToEndGameTest("BodyBuilder_Struct1")]
    public class Struct_BB_Test1 : StructuredBodyBuilderTest
    {
        protected override int NumPlayers => 5;
        protected override int NumDrawings => 4;
        protected override int NumPlayersPerPrompt => 2;
        protected override int NumPromptsPerUserPerRound => 2;
        protected override int NumRounds => 3;
    }

    [EndToEndGameTest("BodyBuilder_Struct2")]
    public class Struct_BB_Test2 : StructuredBodyBuilderTest
    {
        protected override int NumPlayers => 3;
        protected override int NumDrawings => 3;
        protected override int NumPlayersPerPrompt => 2;
        protected override int NumPromptsPerUserPerRound => 2;
        protected override int NumRounds => 1;
    }

    [EndToEndGameTest("BodyBuilder_Struct3")]
    public class Struct_BB_Test3 : StructuredBodyBuilderTest
    {
        protected override int NumPlayers => 4;
        protected override int NumDrawings => 10;
        protected override int NumPlayersPerPrompt => 3;
        protected override int NumPromptsPerUserPerRound => 3;
        protected override int NumRounds => 2;
    }
}
