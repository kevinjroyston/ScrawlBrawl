using RoystonGame.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels.Setup_Person;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels
{
    public class Gameplay_Person
    {
        /// <summary>
        /// The user who came up with this challenge
        /// </summary>
        public User Owner { get; set; }
        public Dictionary<DrawingType, UserDrawing> BodyPartDrawings { get; set; } = new Dictionary<DrawingType, UserDrawing>();

        public bool doneWithRound { get; set; }

        public IReadOnlyList<string> GetOrderedDrawings()
        {
            return new List<string> { BodyPartDrawings[DrawingType.Head].Drawing, BodyPartDrawings[DrawingType.Body].Drawing, BodyPartDrawings[DrawingType.Legs].Drawing}.AsReadOnly();
        }
    }
}
