using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    /// <summary>
    /// If a class implements this it will be called when determining if the object has changed.
    /// </summary>
    public interface IAccessorHashable
    {
        public int GetIAccessorHashCode();
    }
}
