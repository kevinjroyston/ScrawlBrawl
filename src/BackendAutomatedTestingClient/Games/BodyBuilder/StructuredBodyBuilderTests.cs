using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;
using Common.DataModels.Requests.LobbyManagement;

namespace BackendAutomatedTestingClient.Games
{
    public abstract class StructuredBodyBuilderTest : BodyBuilderTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected const int NumBodiesToChooseFrom = 3;
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                },
                StandardGameModeOptions = new StandardGameModeOptions
                {
                    GameDuration = GameDuration.Normal,
                    ShowTutorial = false,
                    TimerEnabled = false
                }
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>();

                int numRounds = 2;
                int numPromptsPerRound = Math.Min(this.NumPlayers, 5);
                int minDrawingsRequired = NumBodiesToChooseFrom * 3; // the amount to make one playerHand to give everyone

                int expectedPromptsPerUser = (int)Math.Ceiling(1.0 * numPromptsPerRound * numRounds / this.NumPlayers);
                int expectedDrawingsPerUser = Math.Max((minDrawingsRequired / this.NumPlayers + 1) * 2, 3);

                int numPromptsPerUserPerRound = Math.Max(1, numPromptsPerRound / 2);


                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_BodyPartDrawing),
                    },
                    repeatCounter: expectedDrawingsPerUser);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_BattlePrompts),
                    },
                    repeatCounter: expectedPromptsPerUser);

                var round = new List<GameStep>();
                round.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_ContestantCreation),
                    },
                    repeatCounter: numPromptsPerUserPerRound);

                round.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns),
                    },
                    repeatCounter: NumPlayers);
                round.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.Waiting));
                round.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns));

                toReturn.AppendRepetitiveGameSteps(round, numRounds);

                return toReturn;
            }
        }
    }

    [EndToEndGameTest("BodyBuilder_Struct1")]
    public class Struct_BB_Test1 : StructuredBodyBuilderTest
    {
        protected override int NumPlayers => 5;
    }

    [EndToEndGameTest("BodyBuilder_Struct2")]
    public class Struct_BB_Test2 : StructuredBodyBuilderTest
    {
        protected override int NumPlayers => 7;
    }
}
