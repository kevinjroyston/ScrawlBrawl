using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.ImposterText.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.ImposterText.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };

        public VoteRevealed_GS(Lobby lobby, Prompt prompt, List<User> randomizedUsersToShow, bool possibleNone, TimeSpan? voteRevealTimeDurration = null)
           : base(
                 lobby: lobby,
                 exit: new WaitForPartyLeader_StateExit(
                     lobby: lobby,
                     partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            this.Entrance.Transition(this.Exit);

            List<UnityImage> unityImages = randomizedUsersToShow.Select((User user) =>
            {
                int numVotesRecieved = 0;
                if (prompt.UsersToNumVotesRecieved.ContainsKey(user))
                {
                    numVotesRecieved = prompt.UsersToNumVotesRecieved[user];
                }
                if (user == prompt.ImposterCreator)
                {
                    return new UnityImage()
                    {
                        Title = new StaticAccessor<string> { Value = "<color=green>" + user.DisplayName + "</color>" },
                        Header = new StaticAccessor<string> { Value = "<color=green>" + prompt.UsersToAnswers[user] + "</color>" },
                        VoteCount = new StaticAccessor<int?> { Value = numVotesRecieved },
                    };
                }
                else
                {
                    return new UnityImage()
                    {
                        Title = new StaticAccessor<string> { Value = user.DisplayName },
                        Header = new StaticAccessor<string> { Value = prompt.UsersToAnswers[user] },
                        VoteCount = new StaticAccessor<int?> { Value = numVotesRecieved },
                    };
                }
                
            }).ToList();

            string imposterTitle = prompt.ImposterCreator.DisplayName;
            if (possibleNone)
            {
                if (!randomizedUsersToShow.Contains(prompt.ImposterCreator))
                {
                    imposterTitle = "Nobody";
                    int numVotesRecieved = 0;
                    if (prompt.UsersToNumVotesRecieved.ContainsKey(prompt.ImposterCreator)) // if none was correct choice all votes were given to imposter
                    {
                        numVotesRecieved = prompt.UsersToNumVotesRecieved[prompt.ImposterCreator];
                    }
                    unityImages.Add(new UnityImage()
                    {
                        Header = new StaticAccessor<string> { Value = "<color=green>None of these</color>" },
                        VoteCount = new StaticAccessor<int?> { Value = numVotesRecieved },
                    });
                }
                else
                {
                    int numVotesRecieved = 0;
                    if (prompt.UsersToNumVotesRecieved.ContainsKey(prompt.Owner))
                    {
                        numVotesRecieved = prompt.UsersToNumVotesRecieved[prompt.Owner]; // if none was not correct choice all votes were given to owner
                    }
                    unityImages.Add(new UnityImage()
                    {
                        Header = new StaticAccessor<string> { Value = "None of these" },
                        VoteCount = new StaticAccessor<int?> { Value = numVotesRecieved }
                    });
                }
            }
            
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = Invariant($"<color=green>{imposterTitle}</color> was the imposter!")  },
                Instructions = new StaticAccessor<string> { Value = Invariant($"Real: '{prompt.RealPrompt}', Imposter: '{prompt.FakePrompt}'") },
                Options = new StaticAccessor<UnityViewOptions>
                {
                    Value = new UnityViewOptions()
                    {
                        PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Vertical },
                    }
                }
            };
        }
    }
}
