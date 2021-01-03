using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.TimsGames.FriendQuiz.DataModels;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.States.UserStates;

namespace Backend.Games.TimsGames.FriendQuiz.GameStates
{
    public class ExtraRoundVoteReveal_GS : GameState
    {
        public ExtraRoundVoteReveal_GS(Lobby lobby, Question question, User differentUser, List<User> randomizedUsers, TimeSpan? maxWaitTime = null)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: Prompts.PartyLeaderSkipRevealButton(),
                      waitingPromptGenerator: Prompts.DisplayText()
                      )
                  )
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
