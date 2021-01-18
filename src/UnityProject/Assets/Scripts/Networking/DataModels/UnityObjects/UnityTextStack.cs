using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels.UnityObjects
{
    public class StackItemHolder
    {
        public string Text { get; set; }
        public UnityUser Owner { get; set; } = null;
    }
    public class UnityTextStack : UnityObject
    {
        public int? FixedNumItems { get; set; } = null;
        public UnityField<IReadOnlyList<StackItemHolder>> TextStackList { get; set; }
    }
}
