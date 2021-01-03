

namespace Common.DataModels.Enums
{
    public enum UserPromptId
    {
        Unknown = 0,
        JoinLobby = 1,
        Waiting = 2,
        Voting = 3,
        SitTight = 4,
        RevealScoreBreakdowns = 5,

        PartyLeader_DefaultPrompt = 10,
        PartyLeader_GameEnd = 11,
        PartyLeader_SkipReveal = 12,
        PartyLeader_SkipScoreboard = 13,

        BattleReady_ContestantCreation = 20,
        BattleReady_BodyPartDrawing = 21,
        BattleReady_BattlePrompts = 22,
        BattleReady_ExtraBodyPartDrawing = 23,
        BattleReady_ExtraBattlePrompts = 24,

        BodyBuilder_TradeBodyPart = 30,
        BodyBuilder_FinishedPerson = 31,
        BodyBuilder_CreatePrompts = 32,
        BodyBuilder_DrawBodyPart = 33,

        ImposterSyndrome_Draw = 40,
        ImposterSyndrome_AnswerPrompt = 41,
        ImposterSyndrome_CreatePrompt = 42,

        ChaoticCooperation_Setup = 50,
        ChaoticCooperation_Draw = 51,

        Mimic_DrawAnything = 60,
        Mimic_RecreateDrawing = 61,

        StoryTime_StartPromptChain = 70,
        StoryTime_ContinuePromptChain = 71,

        FriendQuiz_CreateQuestion = 80,
        FriendQuiz_AnswerQuestion = 81,
        FriendQuiz_ExtraRoundVoting = 82,
    }
}
