using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.TimsGames.FriendQuiz.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            Title = "Skip Reveal",
            SubmitButton = true
        };
        public VoteRevealed_GS(Lobby lobby, User userToShow, List<Question> questionsToShow, TimeSpan? maxWaitTime = null)
             : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: PartyLeaderSkipButton))
        {
            List<UnityImage> displayTexts = questionsToShow.Select((Question question) =>
            {
                string title = "<b>" + question.Text + "</b>";
                List<string> formattedTextList = new List<string>();
                List<string> answers = Question.AnswerTypeToStrings[question.AnswerType].Where((string ans) => ans != "Abstain").ToList();
                for (int i = 0; i < answers.Count; i++)
                {
                    if(question.UsersToAnswers[userToShow] == i+1)
                    {
                        formattedTextList.Add("<color=green>" + answers[i] + "</color>");
                    }
                    else
                    {
                        formattedTextList.Add(answers[i]);
                    }         
                }
                string formattedText = string.Join(" | ", formattedTextList);

                return new UnityImage()
                {
                    Title = new StaticAccessor<string> { Value = title },
                    Header = new StaticAccessor<string> { Value = formattedText }
                };
            }).ToList();

            this.Entrance.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                Title = new StaticAccessor<string> { Value = Invariant($"How do you think {userToShow.DisplayName} answered these questions?") },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = displayTexts },
                Options = new StaticAccessor<UnityViewOptions>
                {
                    Value = new UnityViewOptions()
                    {
                        PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Vertical },
                        PrimaryAxisMaxCount = new StaticAccessor<int?> { Value = 8 }
                    }
                }
            };
        }
        
    }
}
