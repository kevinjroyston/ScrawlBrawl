using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.GameStates.QueryAndReveal;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Games.TimsGames.FriendQuiz.DataModels
{
    public class Question : UserCreatedObject, Common.GameStates.QueryAndReveal.IQueryable<(int, int)>
    {
        public Question()
        {
            //empty
        }
        public Question(Question copy) : base()
        {
            this.AnswerType = copy.AnswerType;
            this.Owner = copy.Owner;
            this.Tags = copy.Tags;
            this.Text = copy.Text;
            this.MainUser = copy.MainUser;
            this.MainAnswer = copy.MainAnswer;
            this.UserAnswers = copy.UserAnswers;
    }
        public enum AnswerTypes
        {
            YesNo,
            AlwaysOftenSometimesNever,
            AgreeDisagree,
            //Custom,
        };

        public static Dictionary<AnswerTypes, string> AnswerTypeToTypeName { get; } = new Dictionary<AnswerTypes, string>()
        {
            {AnswerTypes.YesNo, "No <-> Yes" },
            {AnswerTypes.AlwaysOftenSometimesNever, "Never <-> Always" },
            {AnswerTypes.AgreeDisagree, "Disagree <-> Agree" },
            //{AnswerTypes.Custom, "Custom, Select this and enter bellow" },
        };

        private static Dictionary<AnswerTypes, List<string>> AnswerTypeToSliderTicks { get; } = new Dictionary<AnswerTypes, List<string>>()
        {
            {AnswerTypes.YesNo, new List<string>(){"No", "Yes"} },
            {AnswerTypes.AlwaysOftenSometimesNever, new List<string>(){ "Never", "Sometimes", "Often", "Always" } },
            {AnswerTypes.AgreeDisagree, new List<string>(){"Strongly Disagree", "Disagree", "Agree", "Strongly Agree" } },
        };

        public AnswerTypes AnswerType { get; set; }
        public List<string> TickLabels { 
            get 
            {
                /*if (AnswerType == AnswerTypes.Custom)
                {
                    return CustomTickLabelsOverride;
                }
                else
                {*/
                    return AnswerTypeToSliderTicks[AnswerType];
                //}
            }
            /*set
            {
                if (AnswerType == AnswerTypes.Custom)
                {
                    CustomTickLabelsOverride = value;
                }
            }*/
        }
        //private List<string> CustomTickLabelsOverride { get; set; } = new List<string>();
        public List<int> TickValues { 
            get
            {
                List<int> tickValues = new List<int>();
                for (double d = 0; d <= 1; d += 1.0 / (TickLabels.Count - 1))
                {
                    tickValues.Add((int)(d * FriendQuizConstants.SliderTickRange));
                }
                return tickValues;
            }
        }
        public bool Abstained { get; set; } = false;
        public string Text { get; set; }
        public User MainUser { get; set; }
        public int MainAnswer { get; set; }
        //public ConcurrentDictionary<User, (int, int)> UsersToGuesses { get; set; } = new ConcurrentDictionary<User, (int, int)>();
        public List<QueryInfo<(int, int)>> UserAnswers { get; set; } = new List<QueryInfo<(int, int)>>();

        public UnityObject QueryUnityObjectGenerator(int numericId)
        {
            return null;
        }

        public UnityObject RevealUnityObjectGenerator(int numericId)
        {
            return new UnitySlider()
            {
                Title = new UnityField<string>()
                {
                    Value = Text, 
                },
                SliderBounds = (0, FriendQuizConstants.SliderTickRange),
                TickLabels = TickValues.Select(val => 1f * val).Zip(TickLabels).ToList(),
                MainSliderValues = new List<SliderValueHolder>()
                {
                    new SliderValueHolder()
                    {
                        UserId = MainUser.Id,
                        SingleValue = MainAnswer,
                    },
                },
                GuessSliderValues = UserAnswers.Select(answer => new SliderValueHolder()
                {
                    UserId = answer.UserQueried.Id,
                    ValueRange = answer.Answer,
                }).ToList(),

            };
        }

    }
}
