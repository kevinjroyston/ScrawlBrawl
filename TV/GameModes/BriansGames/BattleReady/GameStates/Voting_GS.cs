using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class Voting_GS : GameState
    {
        private static Func<User, UserPrompt> PickADrawing(Prompt prompt, List<User> randomizedUsers) => (User user) =>
        {
            return new UserPrompt
            {
                Title = "Vote for the best drawing!",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing is best",
                        Answers = randomizedUsers.Select((user)=> prompt.UsersToUserHands[user].Contestant.Name).ToArray()
                    },
                    new SubPrompt
                    {
                        Prompt = $"How good was the prompt (Bad) 1 - 5 (Good)",
                        Dropdown = new string[] {"1","2","3","4","5"}
                    }
                },
                SubmitButton = true,
            };
        };
        Random Rand { get; set; } = new Random();
        public Voting_GS(Lobby lobby, Prompt prompt) : base(lobby)
        {
            List<User>randomizedUsers = prompt.UsersToUserHands.Keys.OrderBy(_ => Rand.Next()).ToList();
            ConcurrentDictionary<User, (User, int)> usersToVoteResults = new ConcurrentDictionary<User, (User, int)>();
            SimplePromptUserState pickContestant = new SimplePromptUserState(
                promptGenerator: PickADrawing(prompt, randomizedUsers),
                formSubmitListener: (User user, UserFormSubmission submission) =>
                {
                    User userVotedFor = randomizedUsers[(int)submission.SubForms[0].RadioAnswer];
                    int promptRanking = (int)submission.SubForms[1].DropdownChoice;
                    usersToVoteResults.TryAdd(user, (userVotedFor, promptRanking));
                    return (true, string.Empty);
                },
                exit: new WaitForAllUsers_StateExit(lobby));
            this.Entrance.Transition(pickContestant);
            pickContestant.AddExitListener(() =>
            {
                foreach (User user in lobby.GetAllUsers())
                {
                    User userVotedFor = usersToVoteResults[user].Item1;
                    int promptRanking = usersToVoteResults[user].Item2;
                    userVotedFor.Score += BattleReadyConstants.PointsForVote;
                    prompt.UsersToUserHands[userVotedFor].VotesForContestant++;
                    prompt.Owner.Score += (promptRanking - 2) * BattleReadyConstants.PointMultiplierForPromptRating;
                    if (prompt.Winner == null || prompt.UsersToUserHands[userVotedFor].VotesForContestant > prompt.UsersToUserHands[prompt.Winner].VotesForContestant)
                    {
                        prompt.Winner = userVotedFor;
                    }
                }
            });
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = prompt.UsersToUserHands.Values.Select((userHand) => userHand.Contestant.GetPersonImage()).ToList() },
                Title = new StaticAccessor<string> { Value = prompt.Text },
            };
          
        }
    }
}
