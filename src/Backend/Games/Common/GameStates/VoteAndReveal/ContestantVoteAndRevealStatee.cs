using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.ThreePartPeople.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.DataModels.Responses.Gameplay;
using Backend.Games.Common.DataModels;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class ContestantVoteAndRevealState<T> : VoteAndRevealState<T> where T: UserCreatedUnityObject
    {
        public override Func<User, List<T>, UserPrompt> VotingPromptGenerator { get; set; }
        public string VotingPromptTitle { get; set; } = "Pick the best submission!";
        public string VotingPromptDescription { get; set; }

        private Func<T, string> ContestantName { get; }
        private TimeSpan? VotingTime { get; }

        private UserPrompt DefaultVotingPromptGenerator(User user, List<T> choices)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.Voting,
                Title = this.VotingPromptTitle,
                PromptHeader = new PromptHeaderMetadata
                {
                    CurrentProgress = 1,
                    MaxProgress = 1,
                    ExpectedTimePerPrompt = VotingTime
                },
                Description = this.VotingPromptDescription,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt()
                    {
                        Answers =  choices.Select(person => this.ContestantName(person)).ToArray() ,
                    }
                },
                SubmitButton = true
            };
        }

        public ContestantVoteAndRevealState(
            Lobby lobby,
            List<T> people,
            Func<T, string> contestantName,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, people, votingUsers, votingTime)
        {
            VotingTime = votingTime;
            VotingPromptGenerator ??= DefaultVotingPromptGenerator;
            ContestantName = contestantName;
        }
    }
}
