using RoystonGame.TV.DataModels.Users;
using System;

namespace RoystonGame.TV.GameModes.Common.DataModels
{
    public class UserDrawing
    {
        public string Drawing { get; set; }
        public Guid Id { get; set; }
        public User Owner { get; set; }
    }
}
