using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal
{
    public class VoteRevealGameState : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };

        public VoteRevealGameState(
            Lobby lobby,
            UnityView voteRevealUnityView,
            Func<User, UserPrompt> waitingPromptGenerator = null) : base(
                lobby: lobby,
                exit: new WaitForPartyLeader_StateExit(
                    lobby: lobby,
                    partyLeaderPromptGenerator: PartyLeaderSkipButton,
                    waitingPromptGenerator: waitingPromptGenerator))
        {
            this.Entrance.Transition(this.Exit);
            this.UnityView = voteRevealUnityView;
        }
    }
}
