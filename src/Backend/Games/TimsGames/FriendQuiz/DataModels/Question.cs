using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.GameStates.QueryAndReveal;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Games.TimsGames.FriendQuiz.DataModels
{
    public class Question : UserCreatedObject, Common.GameStates.QueryAndReveal.IQueryable<(int, int)?>
    {
        public Question()
        {
            //empty
        }
        public Question(Question copy) : base()
        {
            this.Owner = copy.Owner;
            this.Tags = copy.Tags;
            this.Text = copy.Text;
            this.MainUser = copy.MainUser;
            this.MainAnswer = copy.MainAnswer;
            //this.UserAnswers = copy.UserAnswers; Do not copy this
            this.TickLabels = copy.TickLabels;
            this.MinBound = copy.MinBound;
            this.MaxBound = copy.MaxBound;
            this.Numeric = copy.Numeric;
        }

        public List<string> TickLabels { get; set; } = new List<string> { "Min", "Max" };

        public int MinBound { get; set; } = 0;
        public int MaxBound { get; set; } = 100;
        public bool Numeric { get; set; } = false;

        public List<int> TickValues { 
            get
            {
                return new List<int>() { MinBound, MaxBound };
            }
        }
        public bool Abstained { get; set; } = false;
        public string Text { get; set; }
        public User MainUser { get; set; }
        public int MainAnswer { get; set; }
        //public ConcurrentDictionary<User, (int, int)> UsersToGuesses { get; set; } = new ConcurrentDictionary<User, (int, int)>();
        public List<QueryInfo<(int, int)?>> UserAnswers { get; set; } = new List<QueryInfo<(int, int)?>>();

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
                SliderBounds = (MinBound, MaxBound),
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
