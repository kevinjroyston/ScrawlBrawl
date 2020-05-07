using RoystonGame.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels
{
    public class Setup_Person
    {
        /// <summary>
        /// The user who came up with this challenge
        /// </summary>
        public User Owner { get; set; }

        public string Prompt { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();

        public class UserDrawing
        {
            public string Drawing { get; set; }
            public DrawingType Type { get; set; }
            public Guid Id { get; set; }
            public User Owner { get; set; }

        }
        public enum DrawingType
        {
            Head, Body, Legs, None
        }
        public Dictionary<DrawingType, UserDrawing> UserSubmittedDrawingsByDrawingType { get; set; } = new Dictionary<DrawingType, UserDrawing>();
        public Dictionary<User, UserDrawing> UserSubmittedDrawingsByUser { get; set; } = new Dictionary<User, UserDrawing>();
    }
}
