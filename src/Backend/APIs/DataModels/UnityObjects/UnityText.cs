using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityText : UnityObject
    {
        public UnityText()
        {
            this.Type = UnityObjectType.Text;
        }
        public UnityText(UnityObject other): base(other)
        {
            this.Type = UnityObjectType.Text;
        }
    }
}
