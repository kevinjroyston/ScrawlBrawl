using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels
{

    public class UnityField<T> : OptionsInterface<UnityFieldOptions>
    {
        public T Value { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public T StartValue { get; set; }
        public T EndValue { get; set; }

        public Dictionary<UnityFieldOptions, object> Options { get; set; }
    }
}
