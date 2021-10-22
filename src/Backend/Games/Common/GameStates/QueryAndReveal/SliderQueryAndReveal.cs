using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.TimsGames.FriendQuiz.DataModels;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Common.DataModels.Responses.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.QueryAndReveal
{
    public class SliderQueryAndReveal : QueryAndRevealState<Question, (int, int)?>
    {
        public override Func<User, List<Question>, UserPrompt> QueryPromptGenerator { get; set; }
        public override Action<List<Question>> QueryExitListener { get; set; }
        public string QueryPromptTitle { get; set; } = "Answer these questions";
        public string QueryPromptDescription { get; set; }

        public SliderQueryAndReveal(
            Lobby lobby,
            List<Question> objectsToQuery,
            List<User> usersToQuery = null,
            TimeSpan? queryTime = null) : base(lobby, objectsToQuery, usersToQuery, queryTime)
        {
            QueryPromptGenerator ??= DefaultQueryPromptGenerator;
        }
        public override (int, int)? AnswerExtractor(UserSubForm subForm)
        {
            if (subForm?.Slider?.Count == 2)
            {
                return (subForm.Slider[0], subForm.Slider[1]);
            }
            return null;
        }

        public override UnityView QueryUnityViewGenerator()
        {
            UnityView unityView = base.QueryUnityViewGenerator();
            unityView.ScreenId = TVScreenId.WaitForUserInputs;
            unityView.UnityObjects = null;
            return unityView;
        }

        private UserPrompt DefaultQueryPromptGenerator(User user, List<Question> questions)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.FriendQuiz_Query,
                Title = this.QueryPromptTitle,
                PromptHeader = new PromptHeaderMetadata
                {
                    CurrentProgress = 1,
                    MaxProgress = 1,
                },
                Description = this.QueryPromptDescription,
                SubPrompts = questions.Select(question => new SubPrompt()
                {
                    Prompt = question.Text,
                    Slider = new SliderPromptMetadata()
                    {
                        Min = question.MinBound,
                        Max = question.MaxBound,
                        Range = true,
                        ShowTooltip = question.Numeric ? SliderTooltipType.Always : SliderTooltipType.Hide,
                        TicksLabels = question.TickLabels.ToArray(),
                        Ticks = question.TickValues.ToArray(),
                        Value = new int[] { (int)(question.MinBound + (question.MaxBound - question.MinBound) * 0.25), (int)(question.MinBound + (question.MaxBound - question.MinBound) * 0.75) },
                    }
                }).ToArray(),
                SubmitButton = true
            };     
        }
    }
}
