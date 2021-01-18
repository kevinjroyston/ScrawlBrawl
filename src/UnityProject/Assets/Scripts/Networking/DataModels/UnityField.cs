using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TypeEnums;

namespace Assets.Scripts.Networking.DataModels
{

    public class UnityField<T> : FieldValueNullOrEmpty,OptionsInterface<UnityFieldOptions>
    {
        public T Value { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public T StartValue { get; set; }
        public T EndValue { get; set; }

        public Dictionary<UnityFieldOptions, object> Options { get; set; }

        public bool IsNullOrEmpty()
        {
            var timerHolder = Value as TimerHolder;
            if (timerHolder != null)
            {
                return timerHolder.ServerTime == null || timerHolder.StateEndTime == null;
            }
            return Value == null || Value.Equals(string.Empty);
        }
    }
}
