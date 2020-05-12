using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.GameStates
{
    public class EndOfGameState : GameState
    {
        public static UserPrompt ContinuePrompt(User user) => new UserPrompt()
        {
            Title = "What shall we do next?",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Lets play some more!",
                    Answers = RestartTypes.Values.ToArray()
                },
            },
            SubmitButton = true,
        };

        private static Dictionary<EndOfGameRestartType, string> RestartTypes = new Dictionary<EndOfGameRestartType, string>()
        {
            { EndOfGameRestartType.SameGameAndPlayers, "Replay" },
            { EndOfGameRestartType.SamePlayers, "New game, Same players" },
            { EndOfGameRestartType.NewPlayers, "Change Players" },
        };

        public EndOfGameState(Lobby lobby, Action<EndOfGameRestartType> endOfGameRestartCallback, Connector outlet = null) : base(lobby, outlet)
        {
            UserState partyLeaderPrompt = new SimplePromptUserState(prompt: ContinuePrompt);
            State waitForLeader = new WaitForPartyLeader(
                lobby: this.Lobby,
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderPrompt,
                partyLeaderSubmission: (User user, UserStateResult result, UserFormSubmission userInput) =>
                {
                    int? selectedIndex = userInput.SubForms[0].RadioAnswer;
                    if (selectedIndex == null)
                    {
                        throw new Exception("Should have been caught in user input validation");
                    }
                    endOfGameRestartCallback(RestartTypes.Keys.ToList()[selectedIndex.Value]);
                });

            this.Entrance = waitForLeader;

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForPartyLeader },
                Instructions = new StaticAccessor<string> { Value = "Waiting for party leader . . ." },
            };
        }
    }
}
