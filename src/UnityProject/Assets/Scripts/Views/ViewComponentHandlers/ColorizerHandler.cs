using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;

namespace Assets.Scripts.Views.ViewComponentHandlers
{
    public class ColorizerHandler : MonoBehaviour, HandlerInterface
    {
        public Image TargetImage;
        public List<HandlerId> HandlerIds => HandlerType.IdList.ToHandlerIdList(IdType.Object_OwnerIds);

        [HideInInspector]
        public Color AssignedColor;
        public HandlerScope Scope => HandlerScope.UnityObject;

        public void UpdateValue(List<Guid> field)
        {
            string str = "0";
            if (field?.Count > 0)
            {
                str = field[0].ToString();
            }
            AssignedColor = ColorizerManager.Singleton.GetColor(str);
            TargetImage.color = AssignedColor;
        }

        public void UpdateValue(List<dynamic> objects)
        {
            UpdateValue(objects[0]);
        }
    }
}
