using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class BlurredImageVoteAndRevealState : VoteAndRevealState<UserDrawing>
    {
        private double BlurRevealDelay { get; set; }
        private double BlurRevealLength { get; set; }
        public override Func<User, List<UserDrawing>, UserPrompt> VotingPromptGenerator { get; set; } = (User user, List<UserDrawing> options) => new UserPrompt()
        {
            UserPromptId = UserPromptId.Voting,
            Title = "Find the original!", // TODO, abstract this.
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Answers = Enumerable.Range(1, options.Count).Select(num => num.ToString()).ToArray()
                }
            },
            SubmitButton = true
        };

        public BlurredImageVoteAndRevealState(
            Lobby lobby,
            List<UserDrawing> drawings,
            double blurRevealDelay,
            double blurRevealLength,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, drawings, votingUsers, votingTime)
        {
            this.BlurRevealDelay = blurRevealDelay;
            this.BlurRevealLength = blurRevealLength;
        }
        public override UnityView VotingUnityViewGenerator()
        {
            UnityView view = base.VotingUnityViewGenerator();
            view.Options = new StaticAccessor<UnityViewOptions>
            {
                Value = new UnityViewOptions()
                {
                    BlurAnimate = new StaticAccessor<UnityViewAnimationOptions<float?>>
                    {
                        Value = new UnityViewAnimationOptions<float?>()
                        {
                            StartValue = new StaticAccessor<float?> { Value = 1.0f },
                            EndValue = new StaticAccessor<float?> { Value = 0.0f },
                            StartTime = new StaticAccessor<DateTime?> { Value = DateTime.UtcNow.AddSeconds(BlurRevealDelay) },
                            EndTime = new StaticAccessor<DateTime?> { Value = DateTime.UtcNow.AddSeconds(BlurRevealDelay + BlurRevealLength) }
                        }
                    }
                }
            };
            return view;
        }
    }
}
