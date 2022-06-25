using Backend.GameInfrastructure.DataModels.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityUserStatuses
    {
        [JsonProperty("a")]
        public IEnumerable<Guid> UsersAnsweringPromptsGuids { get; } = new List<Guid>();

        [JsonIgnore]
        public IEnumerable<User> UsersAnsweringPrompts { get; }
        public UnityUserStatuses(IEnumerable<User> usersAnsweringPrompts)
        {
            this.UsersAnsweringPrompts = usersAnsweringPrompts;
            this.UsersAnsweringPromptsGuids = usersAnsweringPrompts.Select(user => user.Id).ToList();
        }
    }
}
