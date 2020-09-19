using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
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
