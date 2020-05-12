using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class Voting_GS : GameState
    {
        private static Func<User, UserPrompt> PickADrawing(List<string> choices) => (User user) =>
        {
            // TODO: Rank several drawings rather than pick one.
            // TODO: Option to randomize drawing pairings.
            return new UserPrompt
            {
                Title = "Vote for the best drawing!",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing is best",
                        Answers = choices.ToArray()
                    }
                },
                SubmitButton = true,
            };
        };
        private Lobby lobby { get; set; }
        private Random Rand { get; set; } = new Random();
        public Voting_GS(Lobby lobby, RoundTracker roundTracker, int numSubRounds,  Action<User, UserStateResult, UserFormSubmission> outlet = null, Func<StateInlet> delayedOutlet = null) : base(lobby, outlet, delayedOutlet)
        {        
            List<List<Person>> allRoundPeople = new List<List<Person>>();
            List<User> randomizedUsers = lobby.GetActiveUsers().OrderBy(_ => Rand.Next()).ToList();
            for (int i = 0; i < randomizedUsers.Count; i ++)
            {
                User user = randomizedUsers[i];
                List<string> usersPrompts = roundTracker.UsersToAssignedPrompts[user];
                foreach (string prompt in usersPrompts)
                {
                    for (int j = i+1; j<randomizedUsers.Count; j++)
                    {
                        User user2 = randomizedUsers[j];
                        if(roundTracker.UsersToAssignedPrompts[user2].Contains(prompt))
                        {
                            allRoundPeople.Add(roundTracker.PromptsToBuiltPeople[prompt]);
                        }
                    }
                }
            }
            allRoundPeople = allRoundPeople.OrderBy(_ => Rand.Next()).ToList();
            List<List<string>> allRoundChoices = allRoundPeople.Select(personList => personList.Select(person => person.Name).ToList()).ToList();

            for (int subRoundNum = 0; subRoundNum < numSubRounds; subRoundNum++)
            {
                SimplePromptUserState pickContestant = new SimplePromptUserState(
                    prompt: PickADrawing(choices: allRoundChoices[subRoundNum]),
                    formSubmitListener: (User user, UserFormSubmission submission) =>
                    {
                        user.Score += roundTracker.PointsForVote;
                        return (true, string.Empty);
                    }
                );
                this.Entrance = pickContestant;
                State waitForUsers = new WaitForAllPlayers(lobby: this.Lobby, outlet: this.Outlet);
                waitForUsers.AddStateEndingListener(() => this.UpdateScores());
                waitForUsers.SetOutlet(this.Outlet);
                this.UnityView = new UnityView
                {
                    ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                    UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = allRoundPeople[subRoundNum].Select((Person person) => person.GetPersonImage()).ToList()},
                    Title = new StaticAccessor<string> { Value = roundTracker.BuiltPeopleToPrompts[allRoundPeople[subRoundNum][0]]},
                };
            }
        }
        private void UpdateScores()
        {

        }
    }
}
