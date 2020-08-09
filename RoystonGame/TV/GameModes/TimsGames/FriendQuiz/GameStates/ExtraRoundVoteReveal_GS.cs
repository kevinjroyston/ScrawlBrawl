using Microsoft.AspNetCore.Http;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.TimsGames.FriendQuiz.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz.GameStates
{
    public class ExtraRoundVoteReveal_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };
        public ExtraRoundVoteReveal_GS(Lobby lobby, Question question, User differentUser, List<User> randomizedUsers, TimeSpan? maxWaitTime = null)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            this.Entrance.Transition(this.Exit);

            List<UnityImage> unityImages = randomizedUsers.Select((User user) =>
            {
                int votesRecieved = 0;
                if (question.ExtraRoundUserToVotesRecieved.ContainsKey(user))
                {
                    votesRecieved = question.ExtraRoundUserToVotesRecieved[user];
                }
                if (user.Equals(differentUser))
                {
                    return new UnityImage()
                    {
                        Title = new StaticAccessor<string> { Value = "<color=green><b>" + user.DisplayName + "</b></color>" },
                        VoteCount = new StaticAccessor<int?> { Value = votesRecieved }
                    };
                }
                else
                {
                    return new UnityImage()
                    {
                        Title = new StaticAccessor<string> { Value = user.DisplayName },
                        VoteCount = new StaticAccessor<int?> { Value = votesRecieved }
                    };
                }
            }).ToList();
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                Title = new StaticAccessor<string> { Value = "Here are the results" },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages.AsReadOnly() },
                Options = new StaticAccessor<UnityViewOptions>
                {
                    Value = new UnityViewOptions()
                    {
                        PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Horizontal },
                        PrimaryAxisMaxCount = new StaticAccessor<int?> { Value = 5 }
                    }
                }
            };
        }
    }
}
