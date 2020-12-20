using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Views.Interfaces
{
    [Serializable]
    public enum StringType
    {
        View_Title,
        View_Instructions,
        Object_Title,
        Object_Header,
        Object_Footer,
        Object_ImageIdentifier,
    }

    public interface Strings_HandlerInterface
    {
        StringType Type { get; set; }
        void UpdateValue(UnityField<string> value);
    }
}
