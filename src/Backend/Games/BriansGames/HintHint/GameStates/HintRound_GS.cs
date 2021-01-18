using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.BriansGames.HintHint.DataModels;
using Common.Code.Extensions;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using System.Threading.Tasks;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.Extensions;
using Backend.APIs.DataModels.Enums;

namespace Backend.Games.BriansGames.HintHint.GameStates
{
    public class HintRound_GS : GameState
    {
        public HintRound_GS(
            Lobby lobby,
            RealFakePair realFakePair,
            int maxHints,
            int maxGuesses,
            TimeSpan? guessingTimer = null) : base(lobby, guessingTimer)
        {
            ConcurrentBag<(string, User, DateTime)> hints = new ConcurrentBag<(string, User, DateTime)>();
            ConcurrentBag<(string, User, DateTime)> guesses = new ConcurrentBag<(string, User, DateTime)>();
            ConcurrentBag<User> usersWhoGuessedReal = new ConcurrentBag<User>();
            ConcurrentBag<User> usersWhoGuessedFake = new ConcurrentBag<User>();
            List<(string, User)> orderedHints = new List<(string, User)>();
            List<(string, User)> orderedGuesses = new List<(string, User)>();
            UnityTextStack hintTextStackObject = new UnityTextStack()
            {
                Title = new UnityField<string> { Value = "Hints" },
                FixedNumItems = HintConstants.NumHintGuessesToShow,
                TextStackList = new UnityField<IReadOnlyList<StackItemHolder>>()
                {
                    Value = new List<StackItemHolder>(),
                    Options = new Dictionary<UnityFieldOptions, object>()
                    {
                        { UnityFieldOptions.ScrollingTextStack, true}
                    },
                },
            };
            UnityTextStack guessTextStackObject = new UnityTextStack()
            {
                Title = new UnityField<string> { Value = "Guesses" },
                FixedNumItems = HintConstants.NumHintGuessesToShow,
                TextStackList = new UnityField<IReadOnlyList<StackItemHolder>>()
                {
                    Value = new List<StackItemHolder>(),
                    Options = new Dictionary<UnityFieldOptions, object>()
                    {
                        { UnityFieldOptions.ScrollingTextStack, true}
                    },
                },
            };
            bool realGuessed = false;
            bool fakeGuessed = false;

            ConcurrentDictionary<User, int> userToNumLeft = new ConcurrentDictionary<User, int>();
            foreach (User user in lobby.GetAllUsers())
            {
                if (realFakePair.RealHintGivers.Contains(user))
                {
                    userToNumLeft.AddOrReplace(user, maxHints);
                }
                else if (realFakePair.FakeHintGivers.Contains(user))
                {
                    userToNumLeft.AddOrReplace(user, maxHints);
                }
                else
                {
                    userToNumLeft.AddOrReplace(user, maxGuesses);
                }
            }

            StateChain hintGuessChain = new StateChain(stateGenerator: (int counter) =>
            {
                if (userToNumLeft.Values.Any(numLeft => numLeft > 0) && !realGuessed && !fakeGuessed)
                {
                    return new SelectivePromptUserState(
                        usersToPrompt: userToNumLeft.Keys.Where(user => userToNumLeft[user] > 0).ToList(),
                        promptGenerator: (User user) =>
                        {
                            if (realFakePair.RealHintGivers.Contains(user))
                            {
                                return RealHintPromptGenerator(user);
                            }
                            else if (realFakePair.FakeHintGivers.Contains(user))
                            {
                                return FakeHintPromptGenerator(user);
                            }
                            else
                            {
                                return GuessPromptGenerator(user);
                            }
                        },
                        formSubmitHandler: (User user, UserFormSubmission input) =>
                        {
                            if (realFakePair.RealHintGivers.Contains(user))
                            {
                                return RealHintFormSubmitHandler(user, input);
                            }
                            else if (realFakePair.FakeHintGivers.Contains(user))
                            {
                                return FakeHintFormSubmitHandler(user, input);
                            }
                            else
                            {
                                return GuessFormSubmitHandler(user, input);
                            }
                        },
                        userTimeoutHandler: (User user, UserFormSubmission input) =>
                        {
                            if (realFakePair.RealHintGivers.Contains(user))
                            {
                                return RealHintTimeourHandler(user, input);
                            }
                            else if (realFakePair.FakeHintGivers.Contains(user))
                            {
                                return FakeHintTimeourHandler(user, input);
                            }
                            else
                            {
                                return GuessTimeourHandler(user, input);
                            }
                        });
                }
                else
                {
                    return null;
                }
            });


            hintGuessChain.AddExitListener(() =>
            {
                orderedHints = hints.OrderBy(hintGroup => hintGroup.Item3).Select(hintGroup => (hintGroup.Item1, hintGroup.Item2)).ToList();

                realFakePair.LastHints = orderedHints.Select(hintPair => new StackItemHolder()
                {
                    Text = hintPair.Item1,
                    Owner = new UnityUser(hintPair.Item2),
                }).ToList();

                if (realGuessed)
                {
                    realFakePair.RoundEndCondition = EndCondition.GuessedReal;
                    realFakePair.RelaventUsersWhoGuessed = usersWhoGuessedReal.ToList();
                    foreach (User user in realFakePair.RealHintGivers)
                    {
                        user.ScoreHolder.AddScore(HintConstants.PointsForHintingReal, Score.Reason.Hint_HintedReal);
                    }
                    foreach (User user in usersWhoGuessedReal)
                    {
                        user.ScoreHolder.AddScore(HintConstants.PointsForGuessingReal, Score.Reason.Hint_GuessedReal);
                    }
                }
                else if (fakeGuessed)
                {
                    realFakePair.RoundEndCondition = EndCondition.GuessedFake;
                    realFakePair.RelaventUsersWhoGuessed = usersWhoGuessedFake.ToList();
                    foreach (User user in realFakePair.FakeHintGivers)
                    {
                        user.ScoreHolder.AddScore(HintConstants.PointsForHintingFake, Score.Reason.Hint_HintedFake);
                    }
                }
                else
                {
                    realFakePair.RoundEndCondition = EndCondition.Timeout;
                    foreach (User user in realFakePair.FakeHintGivers)
                    {
                        user.ScoreHolder.AddScore(HintConstants.PointsForFailingReal, Score.Reason.Hint_Distracted);
                    }
                }
            });

            this.Entrance.Transition(hintGuessChain);
            hintGuessChain.Transition(this.Exit);

            this.UnityView = new UnityView(lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = "Hint Time!" },
                Instructions = new UnityField<string> { Value = "Be careful, someone may be trying to hint you to the wrong word." },
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>> 
                { 
                    Value = new List<UnityObject>() { hintTextStackObject, guessTextStackObject}
                }
            };

            #region PromptGenerators
            UserPrompt RealHintPromptGenerator(User user)
            {
                string description = "You cannot say any of the following words:";
                description += $"\n {realFakePair.RealGoal}";
                foreach (string banned in realFakePair.BannedWords)
                {
                    description += $"\n {banned}";
                }
                return new UserPrompt()
                {
                    UserPromptId = UserPromptId.HintHint_Hint,
                    Title = $"You are giving hints for the word: {realFakePair.RealGoal}",
                    Description = description,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = true
                        }
                    },
                    SubmitButton = true
                };
            }
            UserPrompt FakeHintPromptGenerator(User user)
            {
                return new UserPrompt()
                {
                    UserPromptId = UserPromptId.HintHint_Hint,
                    Title = "You are the fake!",
                    Description = $"Try to get people to say {realFakePair.FakeGoal}, instead of {realFakePair.RealGoal}",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = true
                        }
                    },
                    SubmitButton = true
                };
            }
            UserPrompt GuessPromptGenerator(User user)
            {
                return new UserPrompt()
                {
                    UserPromptId = UserPromptId.HintHint_Guess,
                    Title = "What do you think they are hinting at?",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = true
                        }
                    },
                    SubmitButton = true
                };
            }
            #endregion

            #region FormAndTimeoutHandlers
            (bool, string) RealHintFormSubmitHandler(User user, UserFormSubmission input)
            {
                string hint = input?.SubForms?[0].ShortAnswer ?? string.Empty;
                if (realFakePair.BannedWords.Any(banned => hint.FuzzyEquals(banned))
                    || hint.FuzzyEquals(realFakePair.RealGoal)
                    || hint.FuzzyEquals(string.Empty)
                    || hint.Trim().Contains(" "))
                {
                    return (false, $"'{hint}' is not a valid hint");
                }

                userToNumLeft.AddOrIncrease(user, 0, -1);
                hints.Add((hint, user, DateTime.UtcNow));
                UpdateHints();

                return (true, string.Empty);
            }
            (bool, string) FakeHintFormSubmitHandler(User user, UserFormSubmission input)
            {
                string hint = input?.SubForms?[0].ShortAnswer ?? string.Empty;
                if ( hint.FuzzyEquals(realFakePair.FakeGoal)
                    || hint.FuzzyEquals(realFakePair.RealGoal)
                    || hint.FuzzyEquals(string.Empty))
                {
                    return (false, $"'{hint}' is not a valid hint");
                }

                userToNumLeft.AddOrIncrease(user, 0, -1);
                hints.Add((hint, user, DateTime.UtcNow));
                UpdateHints();

                return (true, string.Empty);
            }
            (bool, string) GuessFormSubmitHandler(User user, UserFormSubmission input)
            {
                string guess = input?.SubForms?[0].ShortAnswer ?? string.Empty;
                if (guess.FuzzyEquals(realFakePair.RealGoal) && !fakeGuessed)
                {
                    guess = "<color=green>" + guess + "</color>";
                    realGuessed = true;
                    usersWhoGuessedReal.Add(user);

                    HurryUsers();
                }
                else if (guess.FuzzyEquals(realFakePair.FakeGoal) && !realGuessed)
                {
                    guess = "<color=red>" + guess + "</color>";
                    fakeGuessed = true;
                    usersWhoGuessedFake.Add(user);

                    HurryUsers();
                }

                userToNumLeft.AddOrIncrease(user, 0, -1);
                guesses.Add((guess, user, DateTime.UtcNow));
                UpdateGuesses();

                return (true, string.Empty);
            }

            UserTimeoutAction RealHintTimeourHandler(User user, UserFormSubmission input)
            {
                return UserTimeoutAction.None;
            }
            UserTimeoutAction FakeHintTimeourHandler(User user, UserFormSubmission input)
            {
                return UserTimeoutAction.None;
            }
            UserTimeoutAction GuessTimeourHandler(User user, UserFormSubmission input)
            {
                string guess = input?.SubForms?[0].ShortAnswer ?? string.Empty;
                if (guess.FuzzyEquals(realFakePair.RealGoal) && !fakeGuessed)
                {
                    guess = "<color=green>" + guess + "</color>";
                    realGuessed = true;
                    usersWhoGuessedReal.Add(user);

                    guesses.Add((guess, user, DateTime.UtcNow));
                    UpdateGuesses();
                }

                return UserTimeoutAction.None;
            }
            #endregion


            void UpdateHints()
            {
                // Orders the hints of type (string, DateTime) based off of their time, and pulls out the ordered strings
                orderedHints = hints.OrderBy(hintGroup => hintGroup.Item3).Select(hintGroup => (hintGroup.Item1, hintGroup.Item2)).ToList();

                hintTextStackObject.TextStackList.Value = orderedHints.Select(hintPair => new StackItemHolder()
                {
                    Text = hintPair.Item1,
                    Owner = new UnityUser(hintPair.Item2),
                }).ToList();
            }
            void UpdateGuesses()
            {
                // Orders the guesses of type (string, DateTime) based off of their time, and pulls out the ordered strings
                orderedGuesses = guesses.OrderBy(guessGroup => guessGroup.Item3).Select(guessGroup => (guessGroup.Item1, guessGroup.Item2)).ToList();

                hintTextStackObject.TextStackList.Value = orderedGuesses.Select(guessPair => new StackItemHolder()
                {
                    Text = guessPair.Item1,
                    Owner = new UnityUser(guessPair.Item2),
                }).ToList();
            }
        }

    }
}
