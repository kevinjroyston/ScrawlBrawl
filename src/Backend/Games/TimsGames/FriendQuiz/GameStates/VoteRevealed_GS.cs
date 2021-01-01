using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.TimsGames.FriendQuiz.DataModels;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.TimsGames.FriendQuiz.GameStates
{
    public class VoteRevealed_GS : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.PartyLeader_SkipReveal,
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
            List<Legacy_UnityImage> displayTexts = questionsToShow.Select((Question question) =>
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

                return new Legacy_UnityImage()
                {
                    Title = new StaticAccessor<string> { Value = title },
                    Header = new StaticAccessor<string> { Value = formattedText }
                };
            }).ToList();

            this.Entrance.Transition(this.Exit);

            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                Title = new StaticAccessor<string> { Value = Invariant($"How do you think {userToShow.DisplayName} answered these questions?") },
                UnityImages = new StaticAccessor<IReadOnlyList<Legacy_UnityImage>> { Value = displayTexts },
                Options = new StaticAccessor<Legacy_UnityViewOptions>
                {
                    Value = new Legacy_UnityViewOptions()
                    {
                        PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Vertical },
                        PrimaryAxisMaxCount = new StaticAccessor<int?> { Value = 8 }
                    }
                }
            };
        }
        
    }
}
