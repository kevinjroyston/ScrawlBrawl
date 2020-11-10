using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels;
using Common.DataModels.Interfaces;
using System;
using System.Collections.Generic;

namespace Backend.Games.Common.DataModels
{
    public class UserCreatedObject : IMember
    {

        public Guid Id { get; set; }
        public User Owner { get; set; }
        public DateTime CreationTime { get; }

        public IReadOnlyList<Guid> Tags { get; protected set; }

        public Guid Source => Owner.Id;

        public UserCreatedObject()
        {
            Id = Guid.NewGuid();
            CreationTime = DateTime.UtcNow;
        }
    }
}
