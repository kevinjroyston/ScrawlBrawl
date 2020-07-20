﻿using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };
        public VoteRevealed_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? maxWaitTime = null)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            List<User> randomizedUserChoices = roundTracker.UsersToDisplay;
            this.Entrance.Transition(this.Exit);
            List<UnityImage> unityImages = new List<UnityImage>();
            for (int i = 0; i< randomizedUserChoices.Count; i++)
            {
                User user = randomizedUserChoices[i];
                int voteCount = 0;
                roundTracker.UserToNumVotesRecieved.TryGetValue(user, out voteCount);
                if (user == roundTracker.originalDrawer)
                {
                    if (roundTracker.QuestionsToUsersWhoVotedFor.ContainsKey(i))
                    {
                        unityImages.Add(roundTracker.UsersToUserDrawings[user].GetUnityImage(
                        header: user.DisplayName,
                        voteCount: voteCount,
                        backgroundColor: Color.LightGreen,
                        imageIdentifier: "" + (randomizedUserChoices.IndexOf(user) + 1),
                        relevantUsers: roundTracker.QuestionsToUsersWhoVotedFor[i]));
                    }
                    else
                    {
                        unityImages.Add(roundTracker.UsersToUserDrawings[user].GetUnityImage(
                        header: user.DisplayName,
                        voteCount: voteCount,
                        backgroundColor: Color.LightGreen,
                        imageIdentifier: "" + (randomizedUserChoices.IndexOf(user) + 1)));
                    }
                }
                else
                {
                    if (roundTracker.QuestionsToUsersWhoVotedFor.ContainsKey(i))
                    {
                        unityImages.Add(roundTracker.UsersToUserDrawings[user].GetUnityImage(
                        header: user.DisplayName,
                        voteCount: voteCount,
                        imageIdentifier: "" + (randomizedUserChoices.IndexOf(user) + 1),
                        relevantUsers: roundTracker.QuestionsToUsersWhoVotedFor[i]));
                    }
                    else
                    {
                        unityImages.Add(roundTracker.UsersToUserDrawings[user].GetUnityImage(
                        header: user.DisplayName,
                        voteCount: voteCount,
                        imageIdentifier: "" + (randomizedUserChoices.IndexOf(user) + 1)));
                    }
                }
            }
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = "Voting results!" },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages.AsReadOnly()},      
            };
        }
    }
}
