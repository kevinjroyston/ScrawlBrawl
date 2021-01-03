using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Networking.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels
{
    public class UnityView : OptionsInterface<UnityViewOptions>
    {
        public UnityField<IReadOnlyList<object>> UnityObjects { get; set; }
        public TVScreenId ScreenId { get; set; }
        public Guid Id { get; set; }
        public List<UnityUser> Users { get; set; }
        public UnityField<string> Title { get; set; }
        public UnityField<string> Instructions { get; set; }
        public DateTime? ServerTime { get; set; }
        public DateTime? StateEndTime { get; set; }
        public bool IsRevealing { get; set; }
        public Dictionary<UnityViewOptions, object> Options { get; set; }
    }
}
