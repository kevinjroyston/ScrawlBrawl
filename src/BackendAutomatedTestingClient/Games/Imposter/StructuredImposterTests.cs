﻿using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;

namespace BackendAutomatedTestingClient.Games
{
    public abstract class StructuredImposterTest : ImposterTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = ""+5 } // game speed
                },
            };
        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>()
                {
                    TestCaseHelpers.AllPlayers(UserPromptId.ImposterSyndrome_CreatePrompt, NumPlayers),
                };

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.OneVsAll(
                            numPlayers:NumPlayers,
                            onePrompt:UserPromptId.SitTight,
                            allPrompt: UserPromptId.ImposterSyndrome_Draw),
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns)
                    },
                    repeatCounter: NumPlayers);

                return toReturn;
            }
        }
    }


    [EndToEndGameTest("ImposterSyndrome_Struct1")]
    public class Struct_Imp_Test1 : StructuredImposterTest
    {
        protected override int NumPlayers => 5;
    }

    [EndToEndGameTest("ImposterSyndrome_Struct2")]
    public class Struct_Imp_Test2 : StructuredImposterTest
    {
        protected override int NumPlayers => 4;
    }

    [EndToEndGameTest("ImposterSyndrome_Struct3")]
    public class Struct_Imp_Test3 : StructuredImposterTest
    {
        protected override int NumPlayers => 10;
    }
}
