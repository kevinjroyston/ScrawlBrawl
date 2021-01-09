using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;

namespace BackendAutomatedTestingClient.Games.FriendQuiz
{
    public abstract class StructuredFriendQuizTests : FriendQuizTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected abstract int NumQuestionsSetup { get; }
        protected abstract int NumQuestionsToAnswer { get; }
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = NumQuestionsSetup + "" }, // num rounds
                    new GameModeOptionRequest(){ Value = NumQuestionsToAnswer + "" }, //num prompts per user per round
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
                            prompt: UserPromptId.FriendQuiz_CreateQuestion),
                    },
                    repeatCounter: NumQuestionsSetup);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.FriendQuiz_AnswerQuestion),
                    },
                    repeatCounter: Math.Min(NumQuestionsToAnswer, NumPlayers * NumQuestionsSetup));

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.FriendQuiz_Query),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.Waiting),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns)
                    },
                    repeatCounter: NumPlayers);


                return toReturn;
            }
        }
    }

    [EndToEndGameTest("FriendQuiz_Struct1")]
    public class Struct_FQ_Test1 : StructuredFriendQuizTests
    {
        protected override int NumPlayers => 5;
        protected override int NumQuestionsSetup => 2;
        protected override int NumQuestionsToAnswer => 10;
    }

    [EndToEndGameTest("FriendQuiz_Struct2")]
    public class Struct_FQ_Test2 : StructuredFriendQuizTests
    {
        protected override int NumPlayers => 3;
        protected override int NumQuestionsSetup => 1;
        protected override int NumQuestionsToAnswer => 5;
    }

    [EndToEndGameTest("FriendQuiz_Struct3")]
    public class Struct_FQ_Test3 : StructuredFriendQuizTests
    {
        protected override int NumPlayers => 10;
        protected override int NumQuestionsSetup => 5;
        protected override int NumQuestionsToAnswer => 8;
    }
}
