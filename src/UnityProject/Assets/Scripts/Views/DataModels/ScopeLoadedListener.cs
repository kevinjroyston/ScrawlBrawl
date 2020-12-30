using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TypeEnums;

namespace Assets.Scripts.Views.DataModels
{
    public interface ScopeLoadedListener
    {
        HandlerScope Scope { get; }
        void OnCompleteLoad(); 
    }
}
