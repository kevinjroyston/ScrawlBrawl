using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels
{
    public class UnityRPCDataHolder
    {
        public UnityView UnityView { get; set; }
        public UnityImageList UnityImageList { get; set; }
        public ConfigurationMetadata ConfigurationMetadata { get; set; }
    }
}
