using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.DataModels.UserCreatedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.DataModels
{
    public class UserCreatedObject
    {

        public Guid Id { get; set; }
        public User Owner { get; set; }
        public DateTime CreationTime { get; set; }

        public UserCreatedObject()
        {
            Id = Guid.NewGuid();
            CreationTime = DateTime.UtcNow;
        }
    }
}
