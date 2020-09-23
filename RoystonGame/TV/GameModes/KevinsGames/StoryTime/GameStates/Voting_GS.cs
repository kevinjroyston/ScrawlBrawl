using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KevinsGames.StoryTime.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.KevinsGames.StoryTime.DataModels.RoundTracker;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.KevinsGames.StoryTime.GameStates
{
    public class Voting_GS : GameState
    {
        private Random Rand { get; set; } = new Random();
        private static Func<User, UserPrompt> PickASubmission(List<string> submissions, string prompt) => (User user) =>
        {
            return new UserPrompt
            {
                UserPromptId = UserPromptId.Voting,
                Title = Constants.UIStrings.VotingUnityTitle,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = Invariant($"Which one is the best \"{prompt}\"?"),
                        Answers = submissions.ToArray()
                    },
                },
                SubmitButton = true,
            };
        };

        public Voting_GS(Lobby lobby, string oldText, string prompt, RoundTracker roundTracker) : base(lobby)
        {
            ConcurrentDictionary<User, UserWriting> usersToVoteResults = new ConcurrentDictionary<User, UserWriting>();
            List<UserWriting> writings = roundTracker.UsersToUserWriting.Values.ToList();
            List<string> formattedSubmissions = writings.Select((UserWriting writing) =>
            {
                if(writing.Position == WritingDisplayPosition.Before)
                {
                    return "<p style=\"color:green\"><b>" + writing.Text + "</b></p> \n" + oldText;
                }
                else if(writing.Position == WritingDisplayPosition.After)
                {
                    return oldText + "\n<p style=\"color:green\"><b>" + writing.Text + "</b></p>";
                }
                else // position is none (only in setup)
                {
                    return writing.Text;
                }
            }).ToList();
            SimplePromptUserState pickWriting = new SimplePromptUserState(
                promptGenerator: PickASubmission(formattedSubmissions, prompt),
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    UserWriting writingVotedFor = writings[(int)submission.SubForms[0].RadioAnswer];
                    usersToVoteResults.TryAdd(user, writingVotedFor);
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby));
            this.Entrance.Transition(pickWriting);
            pickWriting.Transition(this.Exit);
            pickWriting.AddExitListener(() =>
            {
                List<UserWriting> winners = new List<UserWriting>();
                foreach (User user in lobby.GetAllUsers())
                {
                    User userVotedFor = usersToVoteResults[user].Owner;
                    if(userVotedFor != user)
                    {
                        userVotedFor.AddScore( StoryTimeConstants.PointsForVote);
                    }
                    usersToVoteResults[user].VotesRecieved++;

                    if (winners.Count == 0)
                    {
                        winners = new List<UserWriting>() { usersToVoteResults[user] };
                    }
                    else
                    {
                        if (usersToVoteResults[user].VotesRecieved > winners.First().VotesRecieved)
                        {
                            winners = new List<UserWriting>() { usersToVoteResults[user] };
                        }
                        else if (usersToVoteResults[user].VotesRecieved == winners.First().VotesRecieved)
                        {
                            winners.Add(usersToVoteResults[user]);
                        }
                    }
                }
                roundTracker.Winner = winners[Rand.Next(0, winners.Count)]; //randomly pick one of the winners to be the one that is kept
            });
            List<UnityImage> displayTexts = writings.Select((UserWriting writing)=>
            {
                string formattedText;
                if (writing.Position == WritingDisplayPosition.Before)
                {
                    formattedText = "<color=green><b>" + writing.Text + "</b></color> \n" + oldText;
                }
                else if (writing.Position == WritingDisplayPosition.After)
                {
                    formattedText = oldText + "\n<color=green><b>" + writing.Text + "</b></color>";
                }
                else // position is none (only in setup)
                {
                    formattedText = writing.Text;
                }
                return new UnityImage()
                {
                    Header = new StaticAccessor<string> { Value = formattedText }
                };
            }).ToList();

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView }, 
                Title = new StaticAccessor<string> { Value = "Time To Vote!"},
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = displayTexts.AsReadOnly()},
                Instructions = new StaticAccessor<string> { Value = Invariant($"Which one is the best \"{prompt}\"?")},
                Options = new StaticAccessor<UnityViewOptions> { Value = new UnityViewOptions() 
                { 
                    PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Horizontal },
                    PrimaryAxisMaxCount = new StaticAccessor<int?> { Value = 4}
                }}
            };

        }
    }
}
