﻿using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates
{
    public class DrawingVoteAndRevealGameState : GameState
    {
        public DrawingVoteAndRevealGameState(
            Lobby lobby,
            List<UserDrawing> drawingsToVoteOn,
            List<User> votingUsers,
            Action<User, UserFormSubmission> votingSubmitHandler,
            int idexOfCorrectChoice,
            List<string> imageTitles = null,
            TimeSpan? votingTime = null) : base(lobby)
        {
            if (imageTitles == null)
            {
                imageTitles = Enumerable.Range(1, drawingsToVoteOn.Count).Select((num) => num.ToString()).ToList();
            }
            if (imageTitles.Count != drawingsToVoteOn.Count)
            {
                //todo log error
            }

            SimplePromptUserState pickOriginal = new SimplePromptUserState(
                promptGenerator: (User user) =>
                {

                },
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    int userVotedFor = (int)submission.SubForms[0].RadioAnswer;
                    usersToVoteResults.TryAdd(user, (DateTime.UtcNow, userVotedFor));
                    return (true, string.Empty);
                },
                userTimeoutHandler
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
                        userVotedFor.AddScore(MimicConstants.PointsForVote);
                        if (userVotedFor == roundTracker.originalDrawer)
                        {
                            user.AddScore(CommonHelpers.PointsForSpeed(
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
