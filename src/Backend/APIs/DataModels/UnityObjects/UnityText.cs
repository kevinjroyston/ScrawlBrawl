using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityText : UnityObject
    {
        public UnityText(Legacy_UnityImage legacy) : base(legacy)
        {
            this.Type = UnityObjectType.Text;
        }
        public UnityText()
        {
            this.Type = UnityObjectType.Text;
        }
    }
}
