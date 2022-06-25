using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.LateToArtClass.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using Common.Code.WordLists;
using System;
using System.Collections.Generic;
using static System.FormattableString;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Backend.GameInfrastructure.DataModels;
using Common.DataModels.Interfaces;
using Common.Code.Helpers;
using System.Linq;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Common.Code.Extensions;
using Backend.Games.Common.DataModels;
using Backend.GameInfrastructure.DataModels.Enums;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Common.DataModels.Responses.Gameplay;
using MiscUtil;
using Backend.Games.Common;

namespace Backend.Games.KevinsGames.LateToArtClass.GameStates
{
    public class Setup_GS : GameState
    {
        private object ChallengeListLock = new object();
        private UserState GetChallengesUserState()
        {
            return new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
                {
                    Title = "Game setup",
                    PromptHeader = new PromptHeaderMetadata
                    {
                        CurrentProgress = 1,
                        MaxProgress = 1,
                    },
                    Description = "In the box below, describe a drawing / art assignment for other players to draw",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            //Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            Prompt = Invariant($"A drawing prompt"),
                            ShortAnswer = true,
                        }
                    },
                    Suggestion = new SuggestionMetadata
                    {
                        SuggestionKey = "LateToArtClass"
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    lock (ChallengeListLock)
                    {
                        ArtClassesToPopulate.Add(new ArtClass
                        {
                            Teacher = user,
                            ArtAssignment = input.SubForms[0].ShortAnswer,
                            MaxMemberCount = this.MaxPlayersPerPrompt,
                            BannedMemberIds = new List<Guid> { user.Id }.ToImmutableHashSet(),
                            AllowDuplicateIds = false,
                        });
                    }
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby: this.Lobby),
                maxPromptDuration: WritingTimeDuration);
        }
        private Random Rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<State> GetDrawingsUserStateChain(User user)
        {
            List<State> stateChain = new List<State>();
            List<ArtClass> onTimeArtClasses = this.ArtClassesToPopulate.Where(artClass => artClass.UsersToDrawings.ContainsKey(user)).Where(artClass=>artClass.LateStudent != user).OrderBy(_ => Rand.Next()).ToList();
            List<ArtClass> lateArtClasses = this.ArtClassesToPopulate.Where(artClass => artClass.LateStudent == user).OrderBy(_ => Rand.Next()).ToList();

            int index = 0;

            // Draw all their on time ones first to buy time before having to lock on another players drawing.
            foreach (ArtClass artClass in onTimeArtClasses)
            {
                var lambdaSafeIndex = index;
                stateChain.Add(new SimplePromptUserState(
                    promptGenerator: (User user) => new UserPrompt()
                    {
                        Title = "Draw the art assignment below",
                        PromptHeader = new PromptHeaderMetadata
                        {
                            CurrentProgress = lambdaSafeIndex + 1,
                            MaxProgress = onTimeArtClasses.Count + lateArtClasses.Count,
                            ExpectedTimePerPrompt = this.PerDrawingTimeDuration
                        },
                        Description = "Careful, the late student might be copying off YOUR work. So make it unique! But make sure you still follow the prompt!",
                        SubPrompts = new SubPrompt[]
                         {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Your assignment:\"{artClass.ArtAssignment}\""),
                                Drawing = new DrawingPromptMetadata(){
                                    GalleryOptions = null
                                },
                            },
                         },
                        SubmitButton = true
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        artClass.UsersToDrawings.AddOrReplace(
                            user,
                            new UserDrawing()
                            {
                                Drawing = input.SubForms[0].DrawingObject,
                                Owner = user,
                                UnityImageRevealOverrides = new UnityObjectOverrides
                                {
                                    Title = user.DisplayName,
                                }
                            });

                        // Locking dance to notify the late student user state that it can proceed. Must be called once, not within a lock.
                        if (artClass.ArtClassLockWinner == null)
                        {
                            lock (artClass.ArtClassLock)
                            {
                                if (artClass.ArtClassLockWinner == null)
                                {
                                    artClass.ArtClassLockWinner = user;
                                }
                            }
                        }

                        if (artClass.ArtClassLockWinner == user)
                        {
                            // Tell the late student's waiting state that it no longer needs to wait. 
                            // Should be fine to call before the student gets there.
                            // DO NOT CALL MORE THAN ONCE, DO NOT CALL FROM WITHIN A LOCK.
                            artClass.HaveArtToCopyOffOfCallback();
                        }

                        return (true, string.Empty);
                    },
                    userTimeoutHandler: (User user, UserFormSubmission input)=>
                    {
                        return UserTimeoutAction.None;
                    }));
                index++;
            }

            // Ideally enough time has passed that there is SOMEONE to copy off of at this point.
            foreach (ArtClass artClass in lateArtClasses)
            {
                var lambdaSafeIndex = index;

                // This will only get seen if nobody triggers this exit by submitting a drawing
                var waitingForCopyStateExit = new WaitForTrigger_StateExit(
                    (User user) => new UserPrompt()
                    {
                        Title = "You were late to class, but there is nobody to copy off of yet! Please hold, sorry!",
                        PromptHeader = new PromptHeaderMetadata
                        {
                            CurrentProgress = lambdaSafeIndex + 1,
                            MaxProgress = onTimeArtClasses.Count + lateArtClasses.Count,
                            ExpectedTimePerPrompt = this.PerDrawingTimeDuration
                        }
                    });

                // Tell other artists to call this waiting state trigger
                artClass.HaveArtToCopyOffOfCallback = waitingForCopyStateExit.Trigger;
                var dummyWaitingState = new PromptlessUserState(waitingForCopyStateExit);
                dummyWaitingState.AddExitListener(() => {
                    // Select a random user to copy from, just before entering the actual state
                    artClass.CopiedFrom = artClass.UsersToDrawings.Where(kvp => kvp.Value?.Drawing!=null).OrderBy(_ => StaticRandom.Next()).FirstOrDefault().Key;
                });

                stateChain.Add(dummyWaitingState);
                stateChain.Add(new SimplePromptUserState(
                    promptGenerator: (User user) => new UserPrompt()
                    {
                        Title = "You were late to art class!",
                        PromptHeader = new PromptHeaderMetadata
                        {
                            CurrentProgress = lambdaSafeIndex + 1,
                            MaxProgress = onTimeArtClasses.Count + lateArtClasses.Count,
                            ExpectedTimePerPrompt = this.PerDrawingTimeDuration
                        },
                        Description = "Better copy off of somebody else. But change it a little so you dont get caught!",
                        SubPrompts = new SubPrompt[]
                         {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Your neighbors assignment:{CommonHelpers.HtmlImageWrapper(artClass.UsersToDrawings[artClass.CopiedFrom].Drawing.DrawingStr)}"),
                                Drawing = new DrawingPromptMetadata(){
                                    GalleryOptions = null
                                },
                            },
                         },
                        SubmitButton = true
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        artClass.UsersToDrawings.AddOrReplace(
                            user,
                            new UserDrawing()
                            {
                                Drawing = input.SubForms[0].DrawingObject,
                                Owner = user,
                                ShouldHighlightReveal = true,
                                UnityImageRevealOverrides = new UnityObjectOverrides
                                {
                                    Title = user.DisplayName,
                                }
                            });
                        return (true, string.Empty);
                    },
                    userTimeoutHandler: (User user, UserFormSubmission input) =>
                    {
                        return UserTimeoutAction.None;
                    }));
                index++;
            }

            return stateChain;
        }
        private List<ArtClass> ArtClassesToPopulate { get; }
        private TimeSpan? WritingTimeDuration { get; }
        private TimeSpan? PerDrawingTimeDuration { get; }
        private int NumRounds { get; }
        private int MaxPlayersPerPrompt { get; }
        private int NumDrawingsPerUser { get; }
        public Setup_GS(Lobby lobby, List<ArtClass> promptsToPopulate, TimeSpan? writingTimeDuration, TimeSpan? drawingTimeDuration, int numRounds, int maxPlayersPerPrompt, int numDrawingsPerUser)
        : base(
            lobby: lobby,
            exit: new WaitForUsers_StateExit(lobby))
        {
            this.ArtClassesToPopulate = promptsToPopulate;
            this.WritingTimeDuration = writingTimeDuration;
            this.PerDrawingTimeDuration = drawingTimeDuration; // TODO, this is incorrect in edge case where we have too many users and maxPlayersPerPrompt is exceeded.
            this.NumRounds = numRounds;
            this.MaxPlayersPerPrompt = maxPlayersPerPrompt;
            this.NumDrawingsPerUser = numDrawingsPerUser;
            State getChallenges = GetChallengesUserState();

            this.Entrance.Transition(getChallenges);
            getChallenges.AddExitListener(() => this.AssignPrompts());
            getChallenges.Transition(() =>
            {
                StateExit waitForDrawings = new WaitForUsers_StateExit(
                    lobby: this.Lobby,
                    waitingPromptGenerator: (User user) =>
                    {
                        return Prompts.DisplayWaitingText("Waiting for others to draw.")(user);
                    });
                var getDrawings = new MultiStateChain(GetDrawingsUserStateChain, exit: waitForDrawings, stateDuration: PerDrawingTimeDuration.MultipliedBy(NumDrawingsPerUser));
                getDrawings.Transition(this.Exit);
                getDrawings.AddEntranceListener(() =>
                {
                    // Need to set unity view dirty since the state timeout has changed technically
                    this.UnityViewDirty = true;
                });
                return getDrawings;
            });

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = "Complete all the prompts on your devices." },
            };
        }


        private void AssignPrompts()
        {
            IReadOnlyList<User> users = this.Lobby.GetAllUsers();
            List<ArtClass> randomizedOrderChallenges = this.ArtClassesToPopulate.OrderBy(_ => Rand.Next()).ToList();
            List<ArtClass> excessChallenges = randomizedOrderChallenges.Skip(this.NumRounds).ToList();
            randomizedOrderChallenges = randomizedOrderChallenges.Take(this.NumRounds).ToList();

            foreach (ArtClass extra in excessChallenges)
            {
                this.ArtClassesToPopulate.Remove(extra);
            }

            if (randomizedOrderChallenges.Count == 0)
            {
                throw new Exception("Can't play the game if there are no prompts");
            }

            List<IGroup<User>> groups = MemberHelpers<User>.Assign(
                randomizedOrderChallenges.Cast<IConstraints<User>>().ToList(),
                users,
                this.NumDrawingsPerUser);

            var assignments = groups.Zip(randomizedOrderChallenges);
            Dictionary<User, int> userToLateCount = new Dictionary<User, int>();
            foreach ((IGroup<User> groupedUsers, ArtClass tracker) in assignments)
            {
                tracker.UsersToDrawings = new ConcurrentDictionary<User, UserDrawing>(
                    groupedUsers.Members.ToDictionary<User, User, UserDrawing>(
                        keySelector: (user) => user,
                        elementSelector: (user) => null));

                foreach (User lateStudent in tracker.UsersToDrawings.Keys.OrderBy(_ => Rand.Next()).ToList())
                {
                    // Make a slight effort to avoid people being late more than once.
                    if (!userToLateCount.ContainsKey(lateStudent)
                        && lateStudent != tracker.Teacher)
                    {
                        userToLateCount[lateStudent] = 1;
                        tracker.LateStudent = lateStudent;
                        break;
                    }
                }
                if(tracker.LateStudent == null)
                {
                    // If we hit here it means every student has already been late once, oh well, pick any except teacher.
                    int breakout = 0;
                    do
                    {
                        tracker.LateStudent = tracker.UsersToDrawings.Keys.ElementAt(Rand.Next(tracker.UsersToDrawings.Keys.Count));
                    }
                    while (tracker.LateStudent == tracker.Teacher && breakout++ < 50);
                }                
            }
        }
    }
}
