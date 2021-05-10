using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;
using Common.DataModels.Requests.LobbyManagement;

namespace BackendAutomatedTestingClient.Games.FriendQuiz
{
    public abstract class StructuredFriendQuizTests : FriendQuizTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected abstract int NumQuestionsToAnswer { get; }
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = NumQuestionsToAnswer + "" }, //num prompts per user per round
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

                int numQuestionsToAnswer = this.NumQuestionsToAnswer;
                int effectiveNumPlayers = Math.Min(this.NumPlayers, 12);
                int numQuestionSetup = (int)(numQuestionsToAnswer * 2.5f / effectiveNumPlayers) + 1; // How many questions each user should contribute.

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.FriendQuiz_CreateQuestion),
                    },
                    repeatCounter: numQuestionSetup);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.FriendQuiz_AnswerQuestion),
                    },
                    repeatCounter: Math.Min(NumQuestionsToAnswer, numQuestionsToAnswer));

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.OneVsAll(UserPromptId.Waiting, NumPlayers, UserPromptId.FriendQuiz_Query),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns),
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
        protected override int NumQuestionsToAnswer => 10;
    }

    [EndToEndGameTest("FriendQuiz_Struct2")]
    public class Struct_FQ_Test2 : StructuredFriendQuizTests
    {
        protected override int NumPlayers => 3;
        protected override int NumQuestionsToAnswer => 5;
    }

    [EndToEndGameTest("FriendQuiz_Struct3")]
    public class Struct_FQ_Test3 : StructuredFriendQuizTests
    {
        protected override int NumPlayers => 10;
        protected override int NumQuestionsToAnswer => 8;
    }
}
