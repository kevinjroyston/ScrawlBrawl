using System;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityViewAnimationOptions<T>
    {
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.StartTime?.Refresh() ?? false;
            modified |= this.EndTime?.Refresh() ?? false;
            modified |= this.StartValue?.Refresh() ?? false;
            modified |= this.EndValue?.Refresh() ?? false;
            return modified;
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<DateTime?> StartTime { private get; set; }
        public DateTime? _StartTime { get => StartTime?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<DateTime?> EndTime { private get; set; }
        public DateTime? _EndTime { get => EndTime?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<float?> StartValue { private get; set; }
        public float? _StartValue { get => StartValue?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<T> EndValue { private get; set; }
        public T _EndValue { get => EndValue.Value; }
    }
}
