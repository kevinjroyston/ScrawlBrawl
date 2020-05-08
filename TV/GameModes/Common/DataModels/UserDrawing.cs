using RoystonGame.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.DataModels
{
    public class UserDrawing
    {
        public string Drawing { get; set; }
        public Guid Id { get; set; }
        public User Owner { get; set; }
    }
}
