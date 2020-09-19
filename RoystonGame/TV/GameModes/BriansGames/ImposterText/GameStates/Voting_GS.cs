using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.ImposterText.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace RoystonGame.TV.GameModes.BriansGames.ImposterText.GameStates
{
    public class Voting_GS : GameState
    {
        public Voting_GS(Lobby lobby, Prompt prompt, List<User> randomizedUsersToShow, bool possibleNone, TimeSpan? votingTimeDuration)
           : base(
                 lobby: lobby,
                 exit: new WaitForUsers_StateExit(lobby))
        {

            ConcurrentDictionary<User, User> usersToVotes = new ConcurrentDictionary<User, User>();

            UserPrompt promptGenerator(User user)
            {
                List<string> detailedChoices = randomizedUsersToShow.Select((User randomUser) => (randomUser == user) ? Invariant($"{prompt.UsersToAnswers[randomUser]} - Your Answer") : prompt.UsersToAnswers[randomUser]).ToList();
                if (possibleNone)
                {
                    detailedChoices.Add("None of these");
                }     

                string description;
                if (prompt.UsersToAnswers.ContainsKey(user))
                {
                    description = Invariant($"'{prompt.Owner.DisplayName}' created this prompt. Your prompt was '{(prompt.Imposter == user ? prompt.FakePrompt : prompt.RealPrompt)}'");
                }
                else if (prompt.Owner == user)
                {
                    description = Invariant($"You created this prompt. Prompt: '{prompt.RealPrompt}', Imposter: '{prompt.FakePrompt}'");
                }
                else
                {
                    description = Invariant($"'{prompt.Owner.DisplayName}' created this prompt.\nYou didn't draw anything for this prompt.");
                }

                return new UserPrompt
                {
                    UserPromptId = UserPromptId.ImposterSnydrome_AnswerPrompt,
                    Title = "Find the imposter!",
                    Description = description,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = $"Which answer is the fake?",
                            Answers = detailedChoices.ToArray()
                        }
                    },
                    SubmitButton = true,
                };
            }

            UserState votingUserState = new SimplePromptUserState(
                promptGenerator: promptGenerator,
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    int choice = input.SubForms[0].RadioAnswer ?? 0;
                    if (possibleNone && choice == randomizedUsersToShow.Count ) // user selected None
                    {
                        if (!randomizedUsersToShow.Contains(prompt.Imposter))
                        {
                            usersToVotes.AddOrReplace(user, prompt.Imposter); // User was correct, set their answer as the imposter 
                        }
                        else
                        {
                            usersToVotes.AddOrReplace(user, prompt.Owner); // User was wrong, set their answer as the prompt creator which will always be wrong
                        }         
                    }
                    else
                    {
                        usersToVotes.AddOrReplace(user, randomizedUsersToShow[choice]);
                    }           
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(
                    lobby: this.Lobby,
                    usersToWaitFor: WaitForUsersType.All),
                maxPromptDuration: votingTimeDuration);

            this.Entrance.Transition(votingUserState);
            votingUserState.Transition(this.Exit);
            votingUserState.AddExitListener(() =>
            {
                List<User> correctUsers = new List<User>();
                foreach (User user in usersToVotes.Keys)
                {
                    prompt.UsersToNumVotesRecieved.AddOrIncrease(usersToVotes[user], 1, 1);
                    if (usersToVotes[user] == prompt.Imposter)
                    {
                        correctUsers.Add(user);
                    }
                    else
                    {
                        if (usersToVotes[user] != user)
                        {
                            usersToVotes[user].Score -= ImposterTextConstants.PointsToLooseForWrongVote; // user was voted for when they weren't the imposter so they lose points
                        }         
                    }
                }

                int totalPointsToAward = usersToVotes.Count * ImposterTextConstants.TotalPointsToAwardPerVote; // determine the total number of points to distribute
                foreach (User user in correctUsers)
                {
                    user.Score += totalPointsToAward / correctUsers.Count; //distribute those evenly to the correct users
                }

                // If EVERYBODY figures out the diff, the owner loses some points but not as many.
                if (correctUsers.Where(user => user != prompt.Owner).Count() == (this.Lobby.GetAllUsers().Count - 1))
                {
                    prompt.Owner.Score -= totalPointsToAward / 4;
                }

                // If the owner couldnt find the diff, they lose a bunch of points.
                if (!correctUsers.Contains(prompt.Owner))
                {
                    prompt.Owner.Score -= totalPointsToAward / 2;
                }
            });

            List<UnityImage> unityImages = randomizedUsersToShow.Select((User user) =>
            {
                return new UnityImage()
                {
                    Header = new StaticAccessor<string> { Value = prompt.UsersToAnswers[user] }
                };
            }).ToList();
            if (possibleNone)
            {
                unityImages.Add(new UnityImage()
                {
                    Header = new StaticAccessor<string> { Value = "None of these" }
                });
            }
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Who do you think answered a different question?" },
                Instructions = new StaticAccessor<string> { Value = possibleNone ? "Someone didn't finish so there may not be an imposter in this group": ""},
                Options = new StaticAccessor<UnityViewOptions> { Value = new UnityViewOptions()
                {
                    PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Vertical},
                    PrimaryAxisMaxCount = new StaticAccessor<int?> { Value = 4 },
                }
                }
            };
        }
    }
}
