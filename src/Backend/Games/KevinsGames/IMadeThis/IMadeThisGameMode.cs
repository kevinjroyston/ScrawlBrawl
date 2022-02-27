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
using System.Collections.Concurrent;
using Common.DataModels.Interfaces;
using Common.Code.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Backend.Games.KevinsGames.IMadeThis
{
    public class IMadeThisGameMode : IGameMode
    {
        private const int MinPlayers = 4;
        public static GameModeMetadata GameModeMetadata { get; } =
            new GameModeMetadata
            {
                Title = "... I made this",
                GameId = GameModeId.IMadeThis,
                Description = "Make the best adjustments to your peer's artwork.",
                MinPlayers = MinPlayers,
                MaxPlayers = null,
                Attributes = new GameModeAttributes
                {
                   ProductionReady = false,
                },
                Options = new List<GameModeOptionResponse>
                {
                },
                GetGameDurationEstimates = GetGameDurationEstimates,
            };
        private static IReadOnlyDictionary<GameDuration, TimeSpan> GetGameDurationEstimates(int numPlayers, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            Dictionary<GameDuration, TimeSpan> estimates = new Dictionary<GameDuration, TimeSpan>();

            numPlayers = Math.Max(numPlayers, MinPlayers);
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = Math.Min(IMadeThisConstants.MaxNumRounds[duration], numPlayers);
                int numDrawingsPerUser = Math.Min(IMadeThisConstants.MaxDrawingsPerPlayer[duration], numRounds - 1);

                numRounds = Math.Min(numRounds, numPlayers* numDrawingsPerUser / IMadeThisConstants.MinNumPlayersPerRound);

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan writingTimer = IMadeThisConstants.WritingTimer[duration];
                TimeSpan drawingTimer = IMadeThisConstants.DrawingTimer[duration];
                TimeSpan votingTimer = IMadeThisConstants.VotingTimer[duration];

                estimate += writingTimer.MultipliedBy(NumPromptsPerUser);
                estimate += drawingTimer.MultipliedBy(numDrawingsPerUser);
                estimate += votingTimer.MultipliedBy(numRounds);
                estimates[duration] = estimate;
            }

            return estimates;
        }
        private SetupPrompts_Gs SetupPrompts { get; set; }
        private SetupInitialDrawing_Gs SetupInitialDrawing { get; set; }
        private SetupImproveInitialDrawing_Gs SetupContestDrawings { get; set; }
        private Random Rand { get; } = new Random();
        private Lobby Lobby { get; set; }
        private const int NumPromptsPerUser = 2;
        public IMadeThisGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, StandardGameModeOptions standardOptions)
        {
            this.Lobby = lobby;
            TimeSpan? writingTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? votingTimer = null;
            GameDuration duration = standardOptions.GameDuration;
            if (standardOptions.TimerEnabled)
            {
                writingTimer = IMadeThisConstants.WritingTimer[duration].MultipliedBy(NumPromptsPerUser);
                drawingTimer = IMadeThisConstants.DrawingTimer[duration];
                votingTimer = IMadeThisConstants.VotingTimer[duration];
            }
            IReadOnlyList<ChallengeTracker> challengeTrackers = new List<ChallengeTracker>();
            int numRounds = Math.Min(IMadeThisConstants.MaxNumRounds[duration], this.Lobby.GetAllUsers().Count);
            int numDrawingsPerUser = Math.Min(IMadeThisConstants.MaxDrawingsPerPlayer[duration], numRounds - 1);

            numRounds = Math.Min(numRounds, (this.Lobby.GetAllUsers().Count-1) * numDrawingsPerUser / IMadeThisConstants.MinNumPlayersPerRound);
            int playersPerPrompt = Math.Min(IMadeThisConstants.MaxNumPlayersPerRound, this.Lobby.GetAllUsers().Count);
            playersPerPrompt = Math.Min(playersPerPrompt, this.Lobby.GetAllUsers().Count * numDrawingsPerUser / numRounds + 1);

            ConcurrentBag<string> prompts = new ConcurrentBag<string>();
            SetupPrompts = new SetupPrompts_Gs(
                lobby: lobby,
                prompts: prompts,
                numExpectedPerUser: NumPromptsPerUser,
                setupDuration: writingTimer);
            this.Entrance.Transition(SetupPrompts);
            SetupPrompts.Transition(CreateSetupInitialDrawings);

            State CreateSetupInitialDrawings()
            {
                numRounds = Math.Min(numRounds, prompts.Count);
                if (numRounds == 1)
                {
                    throw new Exception("Not enough prompts to play the game");
                }

                // This will be trimmed down further after we let users draw initial drawings.
                challengeTrackers = prompts.OrderBy(_ => Rand.Next()).Take(this.Lobby.GetAllUsers().Count).Select(prompt => new ChallengeTracker()
                {
                    InitialPrompt = prompt
                }).ToList();

                var state = new SetupInitialDrawing_Gs(
                    lobby,
                    AssignInitialDrawings(challengeTrackers),
                    drawingTimer);

                state.Transition(CreateSetupSecondaryDrawings);
                return state;
            }

            State CreateSetupSecondaryDrawings()
            {
                // Trim down to num rounds
                challengeTrackers = challengeTrackers.Where(tracker => tracker.InitialDrawing != null).Take(numRounds).ToList();

                numRounds = Math.Min(numRounds, challengeTrackers.Count);
                if (numRounds == 0)
                {
                    throw new Exception("Not enough prompts to play the game");
                }

                var state = new SetupImproveInitialDrawing_Gs(
                    lobby,
                    AssignSecondaryDrawings(challengeTrackers, playersPerPrompt:playersPerPrompt, numDrawingsPerPlayer: numDrawingsPerUser, secondaryPrompts: prompts),
                    drawingTimer);

                state.Transition(CreateGamePlayLoop);
                return state;
            }

            StateChain CreateGamePlayLoop()
            {
                List<State> stateList = new List<State>();
                foreach (ChallengeTracker challengeTracker in challengeTrackers)
                {
                    stateList.Add(new IMadeThisVoteAndReveal_Gs(Lobby, challengeTracker, votingTimer));

                    if(challengeTracker == challengeTrackers.Last())
                    {
                        stateList.Add(new ScoreBoardGameState(lobby, "Final Scores"));
                    }
                    else
                    {
                        stateList.Add(new ScoreBoardGameState(lobby));
                    }
                }
                StateChain gamePlayChain = new StateChain(states: stateList);
                gamePlayChain.Transition(this.Exit);
                return gamePlayChain;
            }
        }

        /// <summary>
        /// Creates a mapping per user of which challenges they will be doing the initial drawing for
        /// </summary>
        /// <returns></returns>
        private Dictionary<User, List<ChallengeTracker>> AssignInitialDrawings(IReadOnlyList<ChallengeTracker> challenges)
        {
            IReadOnlyList<User> users = this.Lobby.GetAllUsers().OrderBy(_ => Rand.Next()).ToList();

            List<IGroup<User>> groups = MemberHelpers<User>.Assign(
                challenges.Cast<IConstraints<User>>().ToList(),
                users);

            // Will only iterate to the length of the smaller of the two lists
            var assignments = users.Zip(challenges);
            return assignments.ToDictionary(kvp => kvp.First, kvp => new List<ChallengeTracker>() { kvp.Second });
        }

        /// <summary>
        /// Creates a mapping per user of which challenges they will be doing the initial drawing for
        /// </summary>
        /// <returns></returns>
        private Dictionary<User, List<ChallengeTracker>> AssignSecondaryDrawings(IReadOnlyList<ChallengeTracker> challenges, int playersPerPrompt, int numDrawingsPerPlayer, ConcurrentBag<string> secondaryPrompts)
        {
            IReadOnlyList<User> users = this.Lobby.GetAllUsers().OrderBy(_ => Rand.Next()).ToList();

            // Get all the prompts that have been used
            List<string> usedPrompts = challenges.Select(challenge => challenge.InitialPrompt).ToList();
            HashSet<string> unusedPrompts = secondaryPrompts.ToHashSet();

            // Remove the used prompts
            unusedPrompts.ExceptWith(usedPrompts);

            List<string> newSecondaryPrompts = unusedPrompts.OrderBy(_ => Rand.Next()).ToList();
            if(newSecondaryPrompts.Count < challenges.Count)
            {
                newSecondaryPrompts.AddRange(usedPrompts.OrderBy(_ => Rand.Next()).ToList());
            }

            int secondaryPromptIndex = 0;
            foreach(ChallengeTracker challenge in challenges)
            {
                challenge.BannedMemberIds = new List<Guid> { challenge.InitialDrawing.Owner.Id }.ToImmutableHashSet();
                challenge.MaxMemberCount = playersPerPrompt;
                challenge.AllowDuplicateIds = false;

                if(string.Equals(newSecondaryPrompts[secondaryPromptIndex], challenge.InitialPrompt, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Skip over if we used this already, append to end
                    newSecondaryPrompts.Add(newSecondaryPrompts[secondaryPromptIndex]);
                    secondaryPromptIndex++;

                    // Technically still possible for duplicates, but should be exceeding rare and not the end of the world.
                }

                challenge.SecondaryPrompt = newSecondaryPrompts[secondaryPromptIndex];
                secondaryPromptIndex++;
            }

            // Now assign the users to each prompt
            List<IGroup<User>> groups = MemberHelpers<User>.Assign(
                challenges.Cast<IConstraints<User>>().ToList(),
                users,
                numDrawingsPerPlayer);

            var assignments = groups.Zip(challenges);

            // Add the user assignments in the trackers themselves
            foreach ((IGroup<User> groupedUsers, ChallengeTracker tracker) in assignments)
            {
                tracker.UsersToDrawings = new ConcurrentDictionary<User, UserDrawing>(
                    groupedUsers.Members.ToDictionary<User, User, UserDrawing>(
                        keySelector: (user) => user,
                        elementSelector: (user) => null));
            }

            // Creates a dictionary tracking what prompts each user got assigned to
            Dictionary<User, List<ChallengeTracker>> challengesForUser = new Dictionary<User, List<ChallengeTracker>>();
            foreach(User user in users)
            {
                challengesForUser[user] = new List<ChallengeTracker>();
                foreach(ChallengeTracker tracker in challenges)
                {
                    if (tracker.UsersToDrawings.ContainsKey(user))
                    {
                        challengesForUser[user].Add(tracker);
                    }
                }
            }

            return challengesForUser;
        }

    }
}
