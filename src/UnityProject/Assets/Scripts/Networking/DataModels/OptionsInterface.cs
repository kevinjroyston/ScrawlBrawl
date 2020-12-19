using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels
{
    interface OptionsInterface<T> where T : Enum{
        Dictionary<T, object> Options { get; set; }
    }
}
