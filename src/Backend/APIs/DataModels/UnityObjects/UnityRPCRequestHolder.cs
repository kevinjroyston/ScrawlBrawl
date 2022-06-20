using Common.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityRPCRequestHolder
    {
        public UnityView UnityView { get; set; }
        public UnityImageList UnityImageList { get; set; }
        public ConfigurationMetadata ConfigurationMetadata { get; set; }
        public UnityUserStatuses UnityUserStatus { get; set; }
    }
}
