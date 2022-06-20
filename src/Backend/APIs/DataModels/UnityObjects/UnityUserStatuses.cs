﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityUserStatuses
    {
        [JsonProperty("a")]
        public IEnumerable<Guid> UsersAnsweringPrompts { get; } = new List<Guid>();
        public UnityUserStatuses(IEnumerable<Guid> usersAnsweringPrompts)
        {
            this.UsersAnsweringPrompts = usersAnsweringPrompts;
        }
    }
}
