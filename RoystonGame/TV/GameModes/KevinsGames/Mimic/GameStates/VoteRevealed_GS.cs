using RoystonGame.TV.ControlFlows.Exit;
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
