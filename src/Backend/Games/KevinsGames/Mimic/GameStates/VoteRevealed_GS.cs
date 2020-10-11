using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.Mimic.DataModels;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.Games.KevinsGames.Mimic.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.PartyLeader_SkipReveal,
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
                List<User> relevantUsers = new List<User>();
                if (roundTracker.QuestionsToUsersWhoVotedFor.ContainsKey(i))
                {
                    relevantUsers = roundTracker.QuestionsToUsersWhoVotedFor[i];
                }
                unityImages.Add(roundTracker.UsersToUserDrawings[user].GetUnityImage(
                    header: user.DisplayName,
                    imageIdentifier: "" + (i + 1),
                    voteRevealOptions: new UnityImageVoteRevealOptions()
                    {
                        //ImageOwner = new StaticAccessor<User> { Value = user},
                        RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = relevantUsers},
                        RevealThisImage = new StaticAccessor<bool?> { Value = (user == roundTracker.originalDrawer)}
                    }));
            }
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.VoteRevealImageView },
                Title = new StaticAccessor<string> { Value = "Voting results!" },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages.AsReadOnly() },
                VoteRevealUsers = new StaticAccessor<IReadOnlyList<User>> { Value = lobby.GetAllUsers() },
            };
        }
    }
}
