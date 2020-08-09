using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
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
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = prompt.UsersToUserHands.Keys.Select((user) => prompt.UsersToUserHands[user].Contestant.GetPersonImage(imageIdentifier: ""+ prompt.UsersToUserHands[user].VotesForContestant, header: user.DisplayName)).ToList() },
                Title = new StaticAccessor<string> { Value = "Voting results!" },
                Instructions = new StaticAccessor<string> { Value = prompt.Text}
            };
        }
    }
}
