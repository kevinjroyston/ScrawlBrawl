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
                Title = "Voting Time!",
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
                foreach (User user in lobby.GetAllUsers())
                {
                    User userVotedFor = usersToVoteResults[user].Owner;
                    if(userVotedFor != user)
                    {
                        userVotedFor.Score += StoryTimeConstants.PointsForVote;
                    }
                    usersToVoteResults[user].VotesRecieved++;
                    if (roundTracker.Winner == null || usersToVoteResults[user].VotesRecieved > roundTracker.Winner.VotesRecieved)
                    {
                        roundTracker.Winner = usersToVoteResults[user];
                    }
                }
            });
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings }, //TODO display text on screen
                Title = new StaticAccessor<string> { Value = "Time To Vote!"},
            };

        }
    }
}
