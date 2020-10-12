using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.BattleReady.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.Games.BriansGames.BattleReady.GameStates
{
    public class Voting_GS : GameState
    {
        private Random Rand { get; set; } = new Random();

        private static Func<User, UserPrompt> PickADrawing(Prompt prompt, IReadOnlyList<User> randomizedUsers) => (User user) =>
        {
            return new UserPrompt
            {
                UserPromptId = UserPromptId.Voting,
                Title = "Vote for the best drawing!",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing is best",
                        Answers = randomizedUsers.Select((user)=> prompt.UsersToUserHands[user].Contestant.Name).ToArray()
                    },
                },
                SubmitButton = true,
            };
        };

        public Voting_GS(Lobby lobby, Prompt prompt) : base(lobby)
        {
            IReadOnlyList<User>randomizedUsers = prompt.UsersToUserHands.Keys.OrderBy(_ => Rand.Next()).ToList();
            ConcurrentDictionary<User, User> usersToVoteResults = new ConcurrentDictionary<User, User>();
            SimplePromptUserState pickContestant = new SimplePromptUserState(
                promptGenerator: PickADrawing(prompt, randomizedUsers),
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    User userVotedFor = randomizedUsers[(int)submission.SubForms[0].RadioAnswer];
                    usersToVoteResults.TryAdd(user, userVotedFor);
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby));
            this.Entrance.Transition(pickContestant);
            pickContestant.Transition(this.Exit);
            pickContestant.AddExitListener(() =>
            {
                foreach (User user in lobby.GetAllUsers())
                {
                    User userVotedFor = usersToVoteResults[user];
                    userVotedFor.AddScore(BattleReadyConstants.PointsForVote);
                    prompt.UsersToUserHands[userVotedFor].VotesForContestant++;
                    if (prompt.Winner == null || prompt.UsersToUserHands[userVotedFor].VotesForContestant > prompt.UsersToUserHands[prompt.Winner].VotesForContestant)
                    {
                        prompt.Winner = userVotedFor;
                    }
                }
            });
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = prompt.UsersToUserHands.Values.Select((userHand) => userHand.Contestant.GetUnityImage()).ToList() },
                Title = new StaticAccessor<string> { Value = prompt.Text },
            };
          
        }
    }
}
