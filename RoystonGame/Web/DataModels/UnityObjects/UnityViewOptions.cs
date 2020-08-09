using RoystonGame.Web.DataModels.Enums;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    // TODO
    public class UnityViewOptions
    {
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.PrimaryAxisMaxCount?.Refresh() ?? false;
            modified |= this.PrimaryAxis?.Refresh() ?? false;
            modified |= this.BlurAnimate?.Refresh() ?? false;
            return modified;
        }

        // Currently only supported for text view
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<int?> PrimaryAxisMaxCount { private get; set; }
        public int? _PrimaryAxisMaxCount { get => PrimaryAxisMaxCount?.Value; }

        // Currently only supported for text view
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<Axis?> PrimaryAxis { private get; set; }
        public Axis? _PrimaryAxis { get => PrimaryAxis?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityViewAnimationOptions<float?>> BlurAnimate { private get; set; }
        public UnityViewAnimationOptions<float?> _BlurAnimate {get => BlurAnimate?.Value; }
    }
}
