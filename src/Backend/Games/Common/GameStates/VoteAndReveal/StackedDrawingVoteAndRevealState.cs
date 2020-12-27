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
using Backend.Games.Common.DataModels.UserCreatedObjects.UserCreatedUnityObjects;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class StackedDrawingVoteAndRevealState<T> : VoteAndRevealState<UserDrawingStack<T>> where T:UserDrawing
    {
        public Func<User, int, string> PromptAnswerAddOnGenerator { get; set; } = (User user, int answer) => "";
        public override Func<User, List<UserDrawingStack<T>>, UserPrompt> VotingPromptGenerator { get; set; }

        private UserPrompt DefaultVotingPromptGenerator(User user, List<UserDrawingStack<T>> choices) {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.Voting,
                Title = "Pick the best submission!",
                SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Answers = Enumerable.Range(1, choices.Count).Select(num => num.ToString() + PromptAnswerAddOnGenerator(user, num -1)).ToArray(),
                }
            },
                SubmitButton = true
            };
        }

        public StackedDrawingVoteAndRevealState(
            Lobby lobby,
            List<UserDrawingStack<T>> stackedDrawings,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, stackedDrawings, votingUsers, votingTime)
        {
            VotingPromptGenerator ??= DefaultVotingPromptGenerator;
        }
    }
}
