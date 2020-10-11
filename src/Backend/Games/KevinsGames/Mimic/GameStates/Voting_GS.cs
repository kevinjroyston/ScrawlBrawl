using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.Common;
using Backend.Games.KevinsGames.Mimic.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.Games.KevinsGames.Mimic.GameStates
{
    public class Voting_GS : GameState
    {
        private Random Rand { get; set; } = new Random();
        private DateTime startingTime;
        private static Func<User, UserPrompt> PickADrawing(List<string> answerChoices) => (User user) =>
        {
            return new UserPrompt
            {
                UserPromptId = UserPromptId.Voting,
                Title = "Let's test that memory!",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing was the original",
                        Answers = answerChoices.ToArray()
                    },
                },
                SubmitButton = true,
            };
        };
        public Voting_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? votingTime) : base(lobby)
        {
            ConcurrentDictionary<User, (DateTime, int)> usersToVoteResults = new ConcurrentDictionary<User, (DateTime, int)>();
            List<User> randomizedUserChoices = roundTracker.UsersToDisplay;
            SimplePromptUserState pickOriginal = new SimplePromptUserState(
                promptGenerator: PickADrawing(Enumerable.Range(1,randomizedUserChoices.Count).Select((int num)=> ""+num).ToList()),
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    int userVotedFor = (int)submission.SubForms[0].RadioAnswer;
                    usersToVoteResults.TryAdd(user, (DateTime.UtcNow, userVotedFor));
                    return (true, string.Empty);
                },
                maxPromptDuration: votingTime,
                exit: new WaitForUsers_StateExit(lobby));
            this.Entrance.Transition(pickOriginal);
            this.Entrance.AddExitListener(() =>
            {
                startingTime = DateTime.UtcNow;
            });
            pickOriginal.Transition(this.Exit);
            pickOriginal.AddExitListener(() =>
            {
                foreach (User user in lobby.GetAllUsers())
                {
                    if (usersToVoteResults.ContainsKey(user))
                    {
                        User userVotedFor = randomizedUserChoices[usersToVoteResults[user].Item2];
                        roundTracker.QuestionsToUsersWhoVotedFor.AddOrUpdate(
                            key: usersToVoteResults[user].Item2,
                            addValue: new List<User>() { user },
                            updateValueFactory: (int key, List<User> oldList) =>
                            {
                                oldList.Add(user);
                                return oldList;
                            });
                        DateTime timeSubmitted = usersToVoteResults[user].Item1;
                        userVotedFor.AddScore( MimicConstants.PointsForVote);
                        if(userVotedFor == roundTracker.originalDrawer)
                        {
                            user.AddScore( CommonHelpers.PointsForSpeed(
                                maxPoints: MimicConstants.PointsForCorrectPick(lobby.GetAllUsers().Count),
                                minPoints: MimicConstants.PointsForCorrectPick(lobby.GetAllUsers().Count) / 10,
                                startTime: MimicConstants.BlurDelay,
                                endTime: MimicConstants.BlurDelay + MimicConstants.BlurLength,
                                secondsTaken: timeSubmitted.Subtract(startingTime).TotalSeconds));                         
                        }
                        roundTracker.UserToNumVotesRecieved.AddOrUpdate(userVotedFor, 1, (User user, int numVotes) => numVotes + 1);
                    }
                }
            });

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = "Find the original drawing" },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>>
                {
                    Value = randomizedUserChoices.Select((User user) =>
                    roundTracker.UsersToUserDrawings[user].GetUnityImage(imageIdentifier: "" + (randomizedUserChoices.IndexOf(user) + 1))).ToList().AsReadOnly(),
                },
                Options = new StaticAccessor<UnityViewOptions>
                {
                    Value = new UnityViewOptions()
                    {
                        BlurAnimate = new StaticAccessor<UnityViewAnimationOptions<float?>>
                        {
                            Value = new UnityViewAnimationOptions<float?>()
                            {
                                StartValue = new StaticAccessor<float?> { Value = 1.0f },
                                EndValue = new StaticAccessor<float?> { Value = 0.0f },
                                StartTime = new StaticAccessor<DateTime?> { Value = DateTime.UtcNow.AddSeconds(MimicConstants.BlurDelay) },
                                EndTime = new StaticAccessor<DateTime?> { Value = DateTime.UtcNow.AddSeconds(MimicConstants.BlurDelay + MimicConstants.BlurLength) }
                            }
                        }
                    }
                }
            };
        }
    }
}
