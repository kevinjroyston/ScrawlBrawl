using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;

namespace BackendAutomatedTestingClient.Games
{
    public abstract class StructuredTwoToneTest : TwoToneTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected abstract int TeamsPerPrompt { get; }
        protected abstract int ColorsPerTeam { get; }
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = ColorsPerTeam + "" }, // max num colors
                    new GameModeOptionRequest(){ Value = TeamsPerPrompt + "" }, // max num teams per prompt
                    new GameModeOptionRequest(){ Value = "true"}, // show other colors
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>()
                {
                    TestCaseHelpers.AllPlayers(UserPromptId.ChaoticCooperation_Setup, NumPlayers),
                };

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.ChaoticCooperation_Draw),
                    },
                    repeatCounter: TeamsPerPrompt * ColorsPerTeam);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers),
                    },
                    repeatCounter: NumPlayers);

                return toReturn;
            }
        }
    }

    [EndToEndGameTest("TwoTone_Struct1")]
    public class Struct_TwoTone_Test1 : StructuredTwoToneTest
    {
        protected override int NumPlayers => 5;

        protected override int TeamsPerPrompt => 2;

        protected override int ColorsPerTeam => 2;
    }

    [EndToEndGameTest("TwoTone_Struct2")]
    public class Struct_TwoTone_Test2 : StructuredTwoToneTest
    {
        protected override int NumPlayers => 12;

        protected override int TeamsPerPrompt => 4;

        protected override int ColorsPerTeam => 3;
    }

    [EndToEndGameTest("TwoTone_Struct3")]
    public class Struct_TwoTone_Test3 : StructuredTwoToneTest
    {
        protected override int NumPlayers => 7;

        protected override int TeamsPerPrompt => 3;

        protected override int ColorsPerTeam => 2;
    }
}
