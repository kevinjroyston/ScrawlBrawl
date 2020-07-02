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
            List<UserWriting> writings = roundTracker.UsersToUserWriting.Values.ToList();
            
           
            List<UnityImage> displayTexts = writings.Select((UserWriting writing) =>
            {
                string formattedText;
                if (writing.Position == WritingDisplayPosition.Before)
                {
                    formattedText = "<color=green><b>" + writing.Text + "</b></color> \n" + oldText;
                }
                else if (writing.Position == WritingDisplayPosition.After)
                {
                    formattedText = oldText + "\n<color=green><b>" + writing.Text + "</b></color>";
                }
                else // position is none (only in setup)
                {
                    formattedText = writing.Text;
                }

                string userName = writing.Owner.DisplayName;
                if(writing == roundTracker.Winner)
                {
                    userName = "<color=green><b>" + userName + "</b></color>";
                }

                return new UnityImage()
                {
                    Header = new StaticAccessor<string> { Value = formattedText },
                    Title = new StaticAccessor<string> { Value = userName },
                    VoteCount = new StaticAccessor<int?> { Value = writing.VotesRecieved }
                };
            }).ToList();

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                Title = new StaticAccessor<string> { Value = "Here are the results" },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = displayTexts.AsReadOnly() },
                Instructions = new StaticAccessor<string> { Value = Invariant($"Which one was the best \"{prompt}\"?") },
                Options = new StaticAccessor<UnityViewOptions>
                {
                    Value = new UnityViewOptions()
                    {
                        PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Horizontal },
                        PrimaryAxisMaxCount = new StaticAccessor<int?> { Value = 4 }
                    }
                }
            };
        }
    }
}
