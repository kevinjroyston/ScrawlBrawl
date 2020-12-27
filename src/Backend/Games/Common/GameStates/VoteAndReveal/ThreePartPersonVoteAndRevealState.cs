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
        public override Func<User, List<T>, UserPrompt> VotingPromptGenerator { get ; set; } = (User user, List<T> choices)=> new UserPrompt()
        {
            UserPromptId = UserPromptId.Voting,
            Title = "Pick the best contestant for the prompt!", //TODO: abstract
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Answers =  choices.Select(person => person.Name).ToArray() ,
                }
            },
            SubmitButton = true
        };

        public ThreePartPersonVoteAndRevealState(
            Lobby lobby,
            List<T> people,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, people, votingUsers, votingTime)
        {
        }
    }
}
