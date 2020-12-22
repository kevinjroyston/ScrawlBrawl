using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TypeEnums;

namespace Assets.Scripts.Views.Interfaces
{
    public interface HandlerInterface
    {
        HandlerScope Scope { get; }
        List<HandlerId> HandlerIds { get; }
        void UpdateValue(List<dynamic> objects);
    }
}
