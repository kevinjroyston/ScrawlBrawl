using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    /// <summary>
    /// Struct containing values used for overriding UnityObject fields.
    /// </summary>
    public struct UnityObjectOverrides
    {
        public string Title { get; set; }
        public string Header { get; set; }
    }
}
