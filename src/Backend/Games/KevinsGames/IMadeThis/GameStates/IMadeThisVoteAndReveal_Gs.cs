using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.IMadeThis.DataModels;
using Backend.Games.KevinsGames.IMadeThis.GameStates;
using Backend.Games.Common;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.GameStates;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Common.Code.Extensions;
using Common.DataModels.Enums;
using Common.DataModels.Requests.LobbyManagement;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Constants = Common.DataModels.Constants;
namespace Backend.Games.KevinsGames.IMadeThis.GameStates
{
    public class IMadeThisVoteAndReveal_Gs : DrawingVoteAndRevealState
    {
        private ChallengeTracker Tracker { get; }
        private static List<UserDrawing> GetDrawingsFromTracker(ChallengeTracker tracker)
        {
            Random Rand = new Random();

            var randomizedSubset = tracker.UsersToDrawings.Where(kvp => kvp.Value?.Drawing != null).ToList().OrderBy(_ => Rand.Next()).ToList();
            List<UserDrawing> drawings = new List<UserDrawing> ();
            drawings.AddRange(randomizedSubset.Select(kvp => kvp.Value));

            return drawings;
        }
        public IMadeThisVoteAndReveal_Gs(Lobby lobby, ChallengeTracker challengeTracker, TimeSpan? votingTime, UnityRoundDetails roundDetails) : base (lobby: lobby, roundDetails:roundDetails, drawings: GetDrawingsFromTracker(challengeTracker), votingTime: votingTime)
        {
            VotingPromptTitle = (user) => $"Who made the best adjustments?";
            VotingPromptDescription = (User user) => $"The prompt: '{challengeTracker.SecondaryPrompt}'";
            VotingViewOverrides = new UnityViewOverrides
            {
                Title = $"Prompt:'{challengeTracker.SecondaryPrompt}'",
            };
            RevealViewOverrides = new UnityViewOverrides
            {
                Title = $"Prompt:'{challengeTracker.SecondaryPrompt}'",
            };
            VoteCountManager = CountVotes(challengeTracker);

            Tracker = challengeTracker;

        }

        private Action<List<UserDrawing>, IDictionary<User, VoteInfo>> CountVotes(ChallengeTracker ChallengeTracker)
        {
            return (List<UserDrawing> choices, IDictionary<User, VoteInfo> votes) =>
            {
                foreach ((User user, VoteInfo vote) in votes)
                {
                    User voteFor = ((UserDrawing)vote.ObjectsVotedFor[0]).Owner;
                    if (voteFor != user)
                    {
                        voteFor.ScoreHolder.AddScore(IMadeThisConstants.PointsPerVote, Score.Reason.ReceivedVotes);
                    }
                }

                // This is probably just equal to num players -1.
                int totalOtherVotes = (choices.Sum((drawing) => drawing.VotesCastForThisObject.Count)) - 1;
                // Points for voting with crowd.
                foreach (UserDrawing drawing in choices)
                {
                    // Gives a percentage of voting with crowd points. Linear with the percentage of other players who agreed with you.
                    int scorePerPlayer = (int)(IMadeThisConstants.PointsForVotingForWinningDrawing * ((drawing.VotesCastForThisObject.Count - 1) / 1.0 / totalOtherVotes));
                    foreach (User userWhoVoted in drawing.VotesCastForThisObject.Select(vote => vote.UserWhoVoted))
                    {
                        userWhoVoted.ScoreHolder.AddScore(scorePerPlayer, Score.Reason.VotedWithCrowd);
                    }
                }
            };
        }

        public override UnityView VotingUnityViewGenerator()
        {
            var baseView = base.VotingUnityViewGenerator();
            var unityObjects = baseView.UnityObjects.Value.ToList();
            // Prepend the initial drawing to the view ONLY, wont be voted on
            unityObjects.Prepend(this.Tracker.InitialDrawing.VotingUnityObjectGenerator(null));
            baseView.UnityObjects = new UnityField<IReadOnlyList<UnityObject>> { Value = unityObjects };
            return baseView;
        }
        public override UnityView RevealUnityViewGenerator()
        {
            var baseView = base.RevealUnityViewGenerator();
            var unityObjects = baseView.UnityObjects.Value.ToList();
            // Prepend the initial drawing to the view ONLY, wont be voted on
            unityObjects.Prepend(this.Tracker.InitialDrawing.RevealUnityObjectGenerator(null));
            baseView.UnityObjects = new UnityField<IReadOnlyList<UnityObject>> { Value = unityObjects };
            return baseView;
        }
    }
}
