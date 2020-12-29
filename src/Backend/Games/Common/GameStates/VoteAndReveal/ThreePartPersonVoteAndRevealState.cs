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

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class ThreePartPersonVoteAndRevealState<T> : VoteAndRevealState<T> where T:Person
    {
        public override Func<User, List<T>, UserPrompt> VotingPromptGenerator { get; set; }
        public string VotingPromptTitle { get; set; } = "Pick the best submission!";
        public string VotingPromptDescription { get; set; }

        private UserPrompt DefaultVotingPromptGenerator(User user, List<T> choices)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.Voting,
                Title = this.VotingPromptTitle,
                Description = this.VotingPromptDescription,
                SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Answers =  choices.Select(person => person.Name).ToArray() ,
                }
            },
                SubmitButton = true
            };
        }

            public ThreePartPersonVoteAndRevealState(
            Lobby lobby,
            List<T> people,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, people, votingUsers, votingTime)
        {
            VotingPromptGenerator ??= DefaultVotingPromptGenerator;
        }
    }
}
