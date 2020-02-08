using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameModes.KylesGames.QuestionQuest.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;

using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.KylesGames.QuestionQuest.GameStates
{
    public class RevealTheAchilles_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            RefreshTimeInMs = 1000,
            SubmitButton = true
        };

        public RevealTheAchilles_GS(Lobby lobby, ChallengeTracker challenge, Connector outlet = null, TimeSpan? maxWaitTime = null) : base(lobby, outlet)
        {
            UserState partyLeaderState = new SimplePromptUserState(PartyLeaderSkipButton, maxPromptDuration: maxWaitTime);
            WaitingUserState waitingState = new WaitingUserState(maxWaitTime: maxWaitTime);

            UserStateTransition waitForLeader = new WaitForPartyLeader(
                lobby: this.Lobby,
                outlet: this.Outlet,
                partyLeaderPrompt: partyLeaderState,
                waitingState: waitingState);

            this.Entrance = waitForLeader;

            var unityImages = new List<UnityImage>();
            unityImages.Add(new UnityImage
            {
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string> { challenge.Drawing } },
                //RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = new List<User> { owner } }
                Footer = new StaticAccessor<string> { Value = challenge.Answers[challenge.AchillesAnswer] },
                VoteCount = new DynamicAccessor<int?> { DynamicBacker = () => challenge.AnswersToUsersWhoSelected[challenge.AchillesAnswer]?.Count ?? 0 }

            });

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Reveal" },
            };
        }
    }
}
