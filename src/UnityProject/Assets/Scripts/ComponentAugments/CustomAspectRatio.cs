using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.ComponentAugments
{
    public interface CustomAspectRatio
    {
        void RegisterAspectRatioListener(Action<float> listener); 
    }
}
