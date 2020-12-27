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
    public class DrawingVoteAndRevealState : VoteAndRevealState<UserDrawing>
    {
        public override Func<User, List<UserDrawing>, UserPrompt> VotingPromptGenerator { get ; set; } = (User user, List<UserDrawing> choices) => new UserPrompt()
        {
            UserPromptId = UserPromptId.Voting,
            Title = null, // TODO: abstract this.
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Selector = new SelectorPromptMetadata(){ ImageList = choices.Select(userDrawing => userDrawing.Drawing).ToArray() },
                }
            },
            SubmitButton = true
        };

        public DrawingVoteAndRevealState(
            Lobby lobby,
            List<UserDrawing> drawings,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, drawings, votingUsers, votingTime)
        {
        }
    }
}
