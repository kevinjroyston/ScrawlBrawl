using Common.DataModels.Enums;

namespace Backend.APIs.DataModels.UnityObjects
{
    // TODO
    public class Legacy_UnityViewOptions
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
        public IAccessor<Legacy_UnityViewAnimationOptions<float?>> BlurAnimate { private get; set; }
        public Legacy_UnityViewAnimationOptions<float?> _BlurAnimate {get => BlurAnimate?.Value; }
    }
}
