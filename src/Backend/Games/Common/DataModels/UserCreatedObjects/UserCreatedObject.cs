using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels;
using System;

namespace Backend.Games.Common.DataModels
{
    public class UserCreatedObject : Identifiable
    {

        public Guid Id { get; set; }
        public User Owner { get; set; }
        public DateTime CreationTime { get; }

        public UserCreatedObject()
        {
            Id = Guid.NewGuid();
            CreationTime = DateTime.UtcNow;
        }
    }
}
