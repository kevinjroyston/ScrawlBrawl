using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class StaticAccessor<T> : IAccessor<T>
    {
        public bool Refresh()
        {
            return false;
        }

        public T Value { get; set; }
    }
}
