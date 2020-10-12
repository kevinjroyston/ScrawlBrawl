using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.Games.BriansGames.BattleReady.DataModels;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.BriansGames.BattleReady.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.PartyLeader_SkipReveal,
            Title = "Skip Reveal",
            SubmitButton = true
        };

        public VoteRevealed_GS(Lobby lobby, Prompt prompt, TimeSpan? maxWaitTime = null)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            this.Entrance.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = prompt.UsersToUserHands.Keys.Select((user) => prompt.UsersToUserHands[user].Contestant.GetUnityImage(imageIdentifier: ""+ prompt.UsersToUserHands[user].VotesForContestant, header: user.DisplayName)).ToList() },
                Title = new StaticAccessor<string> { Value = "Voting results!" },
                Instructions = new StaticAccessor<string> { Value = prompt.Text}
            };
        }
    }
}
