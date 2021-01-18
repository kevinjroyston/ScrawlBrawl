using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TypeEnums;

namespace Assets.Scripts.Views.DataModels
{
    public static class TypeEnumExtensions
    {
        public static HandlerId ToHandlerId(this HandlerType handlerType, object subType = null)
        {
            return new HandlerId(handlerType, subType);
        }
        public static List<HandlerId> ToHandlerIdList(this HandlerType handlerType, object subType = null)
        {
            return new List<HandlerId> { new HandlerId(handlerType, subType) };
        }

        public static HandlerScope GetScope<T>(this T stringType)
        {
            string stringStringType = stringType.ToString();
            if (stringStringType.StartsWith("View_"))
            {
                return HandlerScope.View;
            }
            else if (stringStringType.StartsWith("Object_"))
            {
                return HandlerScope.UnityObject;
            }
            else
            {
                throw new Exception($"Enum did not follow naming convention. '{stringType}'");
            }
        }
    }
}
