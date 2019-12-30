using Microsoft.Xna.Framework;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels.Setup_Person;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates
{
    
    public class Gameplay_GS : GameState
    {
        private Random rand { get; } = new Random();
        private static Func<User, UserPrompt> PickADrawing(Gameplay_Person gameplay_Person, List<string> choices) => (User user) => {
            
            return new UserPrompt
            {
                Title = "This is your current person",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = "Which body part do you want to trade?",
                        Answers = detailedChoices.ToArray()
                    }

                },
                SubmitButton = true,
            };
        };

        // private List<Setup_Person> Setup_PeopleList { get; set; }
        private List<Gameplay_Person> UnassignedPeople { get; set; } = new List<Gameplay_Person>;
        Dictionary<User, Gameplay_Person> AssignedPeople { get; set; } = new Dictionary<User, Gameplay_Person>;
        public Gameplay_GS(List<Setup_Person> setup_PeopleList, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(outlet)
        {
            // this.Setup_PeopleList = setup_PeopleList;
            this.AssignPeople(setup_PeopleList);

        }
        private void AssignPeople(List<Setup_Person> setup_People)
        {
            List<UserDrawing> heads;
            List<UserDrawing> bodies;
            List<UserDrawing> legs;
            heads = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Head]).OrderBy(_ => rand.Next()).ToList();  
            bodies = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Body]).OrderBy(_ => rand.Next()).ToList();
            legs = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Legs]).OrderBy(_ => rand.Next()).ToList();
            foreach (User user in GameManager.GetActiveUsers())
            {
                Gameplay_Person temp = new Gameplay_Person
                {
                    Owner = user,
                };
                temp.BodyPartDrawings.Add(DrawingType.Head, heads.First());
                temp.BodyPartDrawings.Add(DrawingType.Body, bodies.First());
                temp.BodyPartDrawings.Add(DrawingType.Legs, legs.First());
                heads.RemoveAt(0);
                bodies.RemoveAt(0);
                legs.RemoveAt(0);

                AssignedPeople.Add(user, temp);
            }
            if(heads.Count!= GameManager.GetActiveUsers().Count)
            {
                throw new Exception("Something Went Wrong While Setting Up Game");
            }
            while (!heads.Any())
            {
                Gameplay_Person temp = new Gameplay_Person
                {
                    Owner = null,
                };
                temp.BodyPartDrawings.Add(DrawingType.Head, heads.First());
                temp.BodyPartDrawings.Add(DrawingType.Body, bodies.First());
                temp.BodyPartDrawings.Add(DrawingType.Legs, legs.First());
                heads.RemoveAt(0);
                bodies.RemoveAt(0);
                legs.RemoveAt(0);

                UnassignedPeople.Add(temp);
            }

        }


    }
}
