using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public interface OptionsInterface<T> where T : Enum
    {
        Dictionary<T, object> Options { get; set; }
    }
}
