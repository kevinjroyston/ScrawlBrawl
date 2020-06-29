using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.GameStates
{
    public class Voting_GS : GameState
    {
        private Random Rand { get; set; } = new Random();
        private static Func<User, UserPrompt> PickADrawing(List<string> answerChoices) => (User user) =>
        {
            return new UserPrompt
            {
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
        public Voting_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? votingTime ) : base(lobby)
        {
            ConcurrentDictionary<User, User> usersToVoteResults = new ConcurrentDictionary<User, User>();
            List<User> randomizedUserChoices = roundTracker.UsersToDisplay;
            SelectivePromptUserState pickOriginal = new SelectivePromptUserState(
                usersToPrompt: lobby.GetAllUsers().Where((User user) => user != roundTracker.originalDrawer).ToList(),
                promptGenerator: PickADrawing(Enumerable.Range(1,randomizedUserChoices.Count).Select((int num)=> ""+num).ToList()),
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    User userVotedFor = randomizedUserChoices[(int)submission.SubForms[0].RadioAnswer];
                    usersToVoteResults.TryAdd(user, userVotedFor);
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby));
            this.Entrance.Transition(pickOriginal);
            pickOriginal.Transition(this.Exit);
            pickOriginal.AddExitListener(() =>
            {
                foreach (User user in lobby.GetAllUsers())
                {
                    if (usersToVoteResults.ContainsKey(user))
                    {
                        User userVotedFor = usersToVoteResults[user];
                        userVotedFor.Score += MimicConstants.PointsForVote;
                        if(userVotedFor == roundTracker.originalDrawer)
                        {
                            user.Score += MimicConstants.PointsForCorrectPick(lobby.GetAllUsers().Count);
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
                    roundTracker.UsersToUserDrawings[user].GetUnityImage(title:""+(randomizedUserChoices.IndexOf(user)+1))).ToList().AsReadOnly()
                }
            };
        }
    }
}
