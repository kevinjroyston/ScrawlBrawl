using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates
{
    public class ContestantCreation_GS : GameState
    {
        private Random Rand { get; set; } = new Random();
        private List<PeopleUserDrawing> Drawings { get; set; }
        private RoundTracker roundTracker;
        private List<User> RandomizedUsers { get; set; }
        private int numDrawingsInUserHand { get; set; }
        private int numSubRounds { get; set; }
        private int subRoundCount { get; set; } = 0;

        public ContestantCreation_GS(Lobby lobby, int numDrawingsInUserHand, int numSubRounds, List<PeopleUserDrawing> drawings, RoundTracker roundTracker, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            this.RandomizedUsers = lobby.GetActiveUsers().OrderBy(_ => Rand.Next()).ToList();
            this.Drawings = drawings;
            this.roundTracker = roundTracker;
            this.numDrawingsInUserHand = numDrawingsInUserHand;
            this.numSubRounds = numSubRounds;
            AssignDrawingsAndPrompts();

            UserStateTransition waitForAllPlayers = new WaitForAllPlayers(lobby: lobby, outlet: this.Outlet);
            this.Entrance = MakePeopleUserStateChain(outlet: waitForAllPlayers.Inlet)[0];

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Make your contestants on your devices." },
            };
        }

        private List<UserState> MakePeopleUserStateChain(Connector outlet)
        {
            List<UserState> stateChain = new List<UserState>();
            while(subRoundCount<numSubRounds)
            {
                stateChain.Add(new SimplePromptUserState(
                    prompt: (User user) =>
                    {
                        return new UserPrompt
                        {
                            Title = Invariant($"Make the best character for this prompt: \"{roundTracker.UsersToAssignedPrompts[user][subRoundCount]}\""),
                            SubPrompts = new SubPrompt[]
                            {
                            new SubPrompt
                            {
                                Prompt = "Pick your Head",
                                Answers = roundTracker.UsersToPlayerHandsHeadsBySubRound[subRoundCount][user].Select( userDrawing => userDrawing.Drawing).ToArray()
                            },
                            new SubPrompt
                            {
                                Prompt = "Pick your Body",
                                Answers = roundTracker.UsersToPlayerHandsBodiesBySubRound[subRoundCount][user].Select( userDrawing => userDrawing.Drawing).ToArray()
                            },
                            new SubPrompt
                            {
                                Prompt = "Pick your Legs",
                                Answers = roundTracker.UsersToPlayerHandsLegsBySubRound[subRoundCount][user].Select( userDrawing => userDrawing.Drawing).ToArray()
                            },
                            new SubPrompt
                            {
                                Prompt = "Now give your character a name",
                                ShortAnswer = true
                            },
                            },
                            SubmitButton = true,
                        };
                    },
                    formSubmitListener: (User user, UserFormSubmission input) =>
                    {
                        if(!roundTracker.UsersToBuiltPeople.ContainsKey(user))
                        {
                            roundTracker.UsersToBuiltPeople.Add(user, new List<Person>());
                        }
                        Person builtPerson = new Gameplay_Person
                        {
                            BodyPartDrawings = new Dictionary<DrawingType, PeopleUserDrawing>{
                                {DrawingType.Head, roundTracker.UsersToPlayerHandsHeadsBySubRound[subRoundCount][user][(int)input.SubForms[0].RadioAnswer] },
                                {DrawingType.Body, roundTracker.UsersToPlayerHandsBodiesBySubRound[subRoundCount][user][(int)input.SubForms[1].RadioAnswer] },
                                {DrawingType.Legs, roundTracker.UsersToPlayerHandsLegsBySubRound[subRoundCount][user][(int)input.SubForms[2].RadioAnswer] }
                            },
                            Name = input.SubForms[3].ShortAnswer
                        };
                        roundTracker.UsersToBuiltPeople[user].Add(builtPerson);
                        string prompt = roundTracker.UsersToAssignedPrompts[user][subRoundCount];
                        if (!roundTracker.PromptsToBuiltPeople.ContainsKey(prompt))
                        {
                            roundTracker.PromptsToBuiltPeople.Add(prompt, new List<Person>());
                        }
                        roundTracker.PromptsToBuiltPeople[prompt].Add(new Gameplay_Person
                        {
                            BodyPartDrawings = new Dictionary<DrawingType, PeopleUserDrawing>{
                                {DrawingType.Head, roundTracker.UsersToPlayerHandsHeadsBySubRound[subRoundCount][user][(int)input.SubForms[0].RadioAnswer] },
                                {DrawingType.Body, roundTracker.UsersToPlayerHandsBodiesBySubRound[subRoundCount][user][(int)input.SubForms[1].RadioAnswer] },
                                {DrawingType.Legs, roundTracker.UsersToPlayerHandsLegsBySubRound[subRoundCount][user][(int)input.SubForms[2].RadioAnswer] }
                            },
                            Name = input.SubForms[3].ShortAnswer
                        });
                        roundTracker.BuiltPeopleToPrompts.Add(builtPerson, prompt);
                        return (true, String.Empty);
                    }
                    ));
                subRoundCount++;
            }
            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain;
        }
        /*private Func<User, UserPrompt> MakeAPerson()
        {
            return (User user) => {
                return new UserPrompt
                {
                    Title = Invariant($"Make the best character for this prompt: \"{roundTracker.UsersToAssignedPrompts[user][subRoundCount]}\""),
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = "Pick your Head",
                            Answers = roundTracker.UsersToPlayerHandsHeadsBySubRound[subRoundCount][user].Select( userDrawing => userDrawing.Drawing).ToArray()
                        },
                        new SubPrompt
                        {
                            Prompt = "Pick your Body",
                            Answers = roundTracker.UsersToPlayerHandsBodiesBySubRound[subRoundCount][user].Select( userDrawing => userDrawing.Drawing).ToArray()
                        },
                        new SubPrompt
                        {
                            Prompt = "Pick your Legs",
                            Answers = roundTracker.UsersToPlayerHandsLegsBySubRound[subRoundCount][user].Select( userDrawing => userDrawing.Drawing).ToArray()
                        },
                        new SubPrompt
                        {
                            Prompt = "Now give your character a name",
                            ShortAnswer = true
                        },
                    },
                    SubmitButton = true,
                };              
            };
        }*/

        private void AssignDrawingsAndPrompts()
        {
            roundTracker.ResetRoundVariables();
            for (int i = 0; i<numSubRounds; i++)
            {
                Dictionary<User, List<PeopleUserDrawing>> subRoundHandsHeads = new Dictionary<User, List<PeopleUserDrawing>>();
                Dictionary<User, List<PeopleUserDrawing>> subRoundHandsBodies = new Dictionary<User, List<PeopleUserDrawing>>();
                Dictionary<User, List<PeopleUserDrawing>> subRoundHandsLegs = new Dictionary<User, List<PeopleUserDrawing>>();
                List<PeopleUserDrawing> randomizedHeads = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Head).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> randomizedBodies = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Body).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> randomizedLegs = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Legs).OrderBy(_ => Rand.Next()).ToList();
                foreach (User user in RandomizedUsers)
                {
                    List<PeopleUserDrawing> userHandHeads = new List<PeopleUserDrawing>();
                    List<PeopleUserDrawing> userHandBodies = new List<PeopleUserDrawing>();
                    List<PeopleUserDrawing> userHandLegs = new List<PeopleUserDrawing>();
                    for (int j = 0; j < numDrawingsInUserHand; j++)
                    {
                        userHandHeads.Add(randomizedHeads[0]);
                        randomizedHeads.RemoveAt(0);

                        userHandBodies.Add(randomizedBodies[0]);
                        randomizedBodies.RemoveAt(0);

                        userHandLegs.Add(randomizedLegs[0]);
                        randomizedLegs.RemoveAt(0);
                    }
                    subRoundHandsHeads.Add(user, userHandHeads);
                    subRoundHandsBodies.Add(user, userHandBodies);
                    subRoundHandsLegs.Add(user, userHandLegs);
                }
                roundTracker.UsersToPlayerHandsHeadsBySubRound.Add(subRoundHandsHeads);
                roundTracker.UsersToPlayerHandsBodiesBySubRound.Add(subRoundHandsBodies);
                roundTracker.UsersToPlayerHandsLegsBySubRound.Add(subRoundHandsLegs);
            }

            /* gets a randomised list of prompts from UnusedUserPrompts the length required for all players to be prompted this round
             * then removes those prompts from UnusedUserPrompts
             * then doubles the list so that each prompt goes to 2 players
             */
            List<string> randomizedPrompts = roundTracker.UnusedUserPrompts.OrderBy(_ => Rand.Next()).ToList();
            randomizedPrompts.RemoveRange(0, randomizedPrompts.Count - 2 * numSubRounds * RandomizedUsers.Count);
            roundTracker.UnusedUserPrompts.RemoveAll((str) => randomizedPrompts.Contains(str));
            randomizedPrompts.AddRange(randomizedPrompts);
            randomizedPrompts = randomizedPrompts.OrderBy(_ => Rand.Next()).ToList();
            foreach (User user in RandomizedUsers)
            {
                List<string> usersPrompts = new List<string>();
                for (int i = 0; i<numSubRounds; i++)
                {
                    int j = 0;
                    while(usersPrompts.Contains(randomizedPrompts[j]))
                    {
                        j++;
                    }
                    usersPrompts.Add(randomizedPrompts[j]);
                    randomizedPrompts.RemoveAt(j);
                }
                roundTracker.UsersToAssignedPrompts.Add(user, usersPrompts);
            }
        }
    }
}
