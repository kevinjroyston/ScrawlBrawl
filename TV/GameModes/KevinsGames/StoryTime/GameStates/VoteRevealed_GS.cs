using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.TV.GameModes.KevinsGames.StoryTime.DataModels;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.Web.DataModels.Enums;
using static System.FormattableString;
using static RoystonGame.TV.GameModes.KevinsGames.StoryTime.DataModels.RoundTracker;

namespace RoystonGame.TV.GameModes.KevinsGames.StoryTime.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };
        public VoteRevealed_GS(Lobby lobby, RoundTracker roundTracker, string oldText, string prompt, TimeSpan? maxWaitTime = null)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            List<User> randomizedUserChoices = roundTracker.UsersToDisplay;
            this.Entrance.Transition(this.Exit);
            UserWriting writing = roundTracker.Winner;
            string formattedString;
            if (writing.Position == WritingDisplayPosition.Before)
            {
                formattedString = "<p style=\"color:green\"><b>" + writing.Text + "</b></p> \n" + oldText;
            }
            else if (writing.Position == WritingDisplayPosition.After)
            {
                formattedString = oldText + "\n<p style=\"color:green\"><b>" + writing.Text + "</b></p>";
            }
            else // position is none (only in setup)
            {
                formattedString = writing.Text;
            }

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = Invariant($"{writing.Owner.DisplayName} created the best {prompt}")},
                Instructions = new StaticAccessor<string> { Value = formattedString}
            };
        }
    }
}
