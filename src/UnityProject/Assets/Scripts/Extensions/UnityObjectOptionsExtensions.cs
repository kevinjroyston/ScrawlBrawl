using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Extensions
{
    public static class UnityObjectOptionsExtensions
    {
        public static bool ShouldRevealThisObject(this Dictionary<UnityObjectOptions, object> options)
        {
            return (bool)(options?[UnityObjectOptions.RevealThisImage] ?? false);
        }
    }
}
