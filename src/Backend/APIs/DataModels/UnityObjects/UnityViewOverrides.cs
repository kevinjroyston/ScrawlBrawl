using Backend.APIs.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    /// <summary>
    /// struct containing fields used for overriding UnityView fields.
    /// </summary>
    public class UnityViewOverrides
    {
        public string Title { get; set; }
        public string Instructions { get; set; }

        public Dictionary<UnityViewOptions, object> Options { get; set; }
    }
}
