using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.Enums;
using FluentAssertions;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class VotingGameState<T>: GameState where T:class, IVotable
    {
        private Func<User, List<T>, UserPrompt> PromptGenerator { get; set; }
        private List<T> ObjectList { get; set; }
        private Dictionary<User, UserPrompt> PromptsPerUser { get; set; } = new Dictionary<User, UserPrompt>();
        private Dictionary<User, VoteInfo> UserVotes { get; set; } = new Dictionary<User, VoteInfo>();
        protected DateTime StartingTime { get; private set; }
        public VotingGameState(
            Lobby lobby,
            Func<User, List<T>, UserPrompt> votingUserPromptGenerator,
            List<T> votingObjects,
            Action<List<T>, Dictionary<User,VoteInfo>> votingExitListener,
            UnityView votingUnityView,
            List<User> votingUsers,
            TimeSpan? votingTime = null) : base(lobby)
        {
            this.PromptGenerator = votingUserPromptGenerator;
            this.ObjectList = votingObjects;
            this.Entrance.AddExitListener(() =>
            {
                StartingTime = DateTime.UtcNow;
            });
            SelectivePromptUserState votingUserState = new SelectivePromptUserState(
                usersToPrompt: votingUsers ?? lobby.GetAllUsers().ToList(),
                promptGenerator: this.InternalPromptGenerator,
                formSubmitHandler: this.FormSubmitHandler,
                userTimeoutHandler: this.FormTimeoutHandler,
                maxPromptDuration: votingTime,
                exit: new WaitForUsers_StateExit(lobby: lobby));

            this.Entrance.Transition(votingUserState);
            votingUserState.Transition(this.Exit);
            votingUserState.AddExitListener(()=>votingExitListener(this.ObjectList, this.UserVotes));

            this.UnityView = votingUnityView;
        }

        private UserPrompt InternalPromptGenerator(User user)
        {
            UserPrompt prompt = this.PromptGenerator(user, this.ObjectList);
            ValidateUserPrompt(prompt);
            PromptsPerUser[user] = prompt;
            return prompt;
        }

        private (bool, string) FormSubmitHandler(User user, UserFormSubmission submission)
        {
            SubPrompt subPrompt = this.PromptsPerUser[user].SubPrompts[0];
            T objectVotedFor = null;
            if (subPrompt.Answers != null)
            {
                objectVotedFor = this.ObjectList[submission.SubForms[0].RadioAnswer.Value];
            }
            else if (subPrompt?.Selector?.ImageList != null)
            {
                objectVotedFor = this.ObjectList[submission.SubForms[0].RadioAnswer.Value];
            }
            objectVotedFor.Should().NotBeNull(because: "Could not find an object voted for");
            VoteInfo vote = new VoteInfo
            {
                ObjectsVotedFor = new List<dynamic> { objectVotedFor },
                TimeTakenInMs = DateTime.UtcNow.Subtract(this.StartingTime).TotalMilliseconds,
                UserWhoVoted = user
            };
            objectVotedFor.VotesCastForThisObject.Add(vote);
            this.UserVotes[user] = vote;

            return (true, string.Empty);
        }
        private void ValidateUserPrompt(UserPrompt prompt)
        {
            // Syntax below is from Fluent assertions
            prompt.SubPrompts.Should().NotBeNull().And.HaveCount(1, because: "VotingGameState only equipped to handle 1 subprompt.");

            SubPrompt subPrompt = prompt.SubPrompts[0];
            if (subPrompt.Answers != null)
            {
                subPrompt.Answers.Should().HaveCount(this.ObjectList.Count, "Answers list length should match length of voting object list");
            }
            if (subPrompt?.Selector?.ImageList != null)
            {
                subPrompt.Selector.ImageList.Should().HaveCount(this.ObjectList.Count, "Answers list length should match length of voting object list");
            }
        }
        private UserTimeoutAction FormTimeoutHandler(User user, UserFormSubmission submission)
        {
            SubPrompt subPrompt = this.PromptsPerUser[user].SubPrompts[0];
            T objectVotedFor = null;
            if (subPrompt.Answers != null)
            {
                objectVotedFor = this.ObjectList[submission.SubForms[0].RadioAnswer.Value];
            }
            else if (subPrompt?.Selector?.ImageList != null)
            {
                objectVotedFor = this.ObjectList[submission.SubForms[0].RadioAnswer.Value];
            }

            // TODO: Support ability to not count a vote.
            if (objectVotedFor == null)
            {
                objectVotedFor = ObjectList.First();
            }
            VoteInfo vote = new VoteInfo
            {
                ObjectsVotedFor = new List<dynamic> { objectVotedFor },
                TimeTakenInMs = DateTime.UtcNow.Subtract(this.StartingTime).TotalMilliseconds,
                UserWhoVoted = user
            };
            objectVotedFor.VotesCastForThisObject.Add(vote);
            this.UserVotes[user] = vote;
            return UserTimeoutAction.None;
        }
    }
}
