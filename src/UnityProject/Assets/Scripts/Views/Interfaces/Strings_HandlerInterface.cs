using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Views.Interfaces
{
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
        public StringType Type { get; set; }
        public void UpdateValue(UnityField<string> value);
    }
}
