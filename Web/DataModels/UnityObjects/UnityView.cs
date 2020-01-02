using RoystonGame.TV;
using RoystonGame.TV.DataModels;
using RoystonGame.Web.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class UnityView
    {
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.Options?.Refresh() ?? false;
            modified |= this.ScreenId?.Refresh() ?? false;
            modified |= this.Users?.Refresh() ?? false;
            modified |= this.Title?.Refresh() ?? false;
            modified |= this.Instructions?.Refresh() ?? false;
            modified |= this.UnityImages?.Any(image => image?.Refresh() ?? false) ?? false;
            return modified;
        }
        public Guid Id { get; } = Guid.NewGuid();
        public UnityViewOptions Options { get; set; }
        public List<UnityImage> UnityImages { get; set; } = new List<UnityImage>();


        [JsonIgnore]
        public IAccessor<TVScreenId> ScreenId { private get; set; }
        public TVScreenId? _ScreenId { get => ScreenId?.Value; }


        [JsonIgnore]
        public IAccessor<IReadOnlyList<User>> Users { private get; set; } = new DynamicAccessor<IReadOnlyList<User>> { DynamicBacker = () => GameManager.GetActiveUsers() };
        public IReadOnlyList<User> _Users { get => Users?.Value; }

        [JsonIgnore]
        public IAccessor<string> Title { private get; set; }
        public string _Title { get => Title?.Value; }


        [JsonIgnore]
        public IAccessor<string> Instructions { private get; set; }
        public string _Instructions { get => Instructions?.Value; }
    }
}
