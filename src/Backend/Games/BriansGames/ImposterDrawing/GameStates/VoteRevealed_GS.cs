using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.ImposterDrawing.DataModels;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.FormattableString;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Constants = Common.DataModels.Constants;

namespace Backend.Games.BriansGames.ImposterDrawing.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.PartyLeader_SkipReveal,
            Title = "Skip Reveal",
            SubmitButton = true
        };

        public VoteRevealed_GS(Lobby lobby, Prompt prompt, List<User> randomizedUsersToShow, bool possibleNone, TimeSpan? voteRevealTimeDuration = null)
           : base(
                 lobby: lobby,
                 exit: new WaitForPartyLeader_StateExit(
                     lobby: lobby,
                     partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            this.Entrance.Transition(this.Exit);

            int counter = 0;
            List<UnityImage> unityImages = randomizedUsersToShow.Select((User user) =>
            {
                int numVotesRecieved = 0;
                counter++;
                if (prompt.UsersToNumVotesRecieved.ContainsKey(user))
                {
                    numVotesRecieved = prompt.UsersToNumVotesRecieved[user];
                }
                if (user == prompt.Imposter)
                {
                    return prompt.UsersToDrawings[user].GetUnityImage(
                        backgroundColor: Color.LightGreen,
                        title: "<color=green>" + user.DisplayName + "</color>",
                        voteCount: numVotesRecieved,
                        imageIdentifier: "" + counter);
                }
                else
                {
                    return prompt.UsersToDrawings[user].GetUnityImage(
                        title: user.DisplayName,
                        voteCount: numVotesRecieved,
                        imageIdentifier: "" + counter);
                }

            }).ToList();

            string imposterTitle = prompt.Imposter.DisplayName;
            if (possibleNone)
            {
                if (!randomizedUsersToShow.Contains(prompt.Imposter))
                {
                    imposterTitle = "Nobody";
                    int numVotesRecieved = 0;
                    if (prompt.UsersToNumVotesRecieved.ContainsKey(prompt.Imposter)) // if none was correct choice all votes were given to imposter
                    {
                        numVotesRecieved = prompt.UsersToNumVotesRecieved[prompt.Imposter];
                    }
                    unityImages.Add(new UnityImage()
                    {
                        BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 250, 128, 114 } },
                        Header = new StaticAccessor<string> { Value = "<color=green>None of these</color>" },
                        Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string>() { Constants.Drawings.NoneUnityImage } },
                        VoteCount = new StaticAccessor<int?> { Value = numVotesRecieved },
                    });
                }
                else
                {
                    int numVotesRecieved = 0;
                    if (prompt.UsersToNumVotesRecieved.ContainsKey(prompt.Owner))
                    {
                        numVotesRecieved = prompt.UsersToNumVotesRecieved[prompt.Owner]; // if none was not correct choice all votes were given to owner
                    }
                    unityImages.Add(new UnityImage()
                    {
                        Header = new StaticAccessor<string> { Value = "None of these" },
                        Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string>() { Constants.Drawings.NoneUnityImage } },
                        VoteCount = new StaticAccessor<int?> { Value = numVotesRecieved }
                    });
                }
            }

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = Invariant($"<color=green>{imposterTitle}</color> was the imposter!") },
                Instructions = new StaticAccessor<string> { Value = Invariant($"Real: '{prompt.RealPrompt}', Imposter: '{prompt.FakePrompt}'") },
            };
        }
    }
}
