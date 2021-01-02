using Assets.Scripts.Networking.DataModels.UnityObjects;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TypeEnums;

namespace Assets.Scripts.Views.ViewComponentHandlers
{
    public class SliderHandler : MonoBehaviour, HandlerInterface
    {
        public RectTransform SliderBody;
        public HandlerScope Scope => HandlerScope.UnityObject;
        private float bodyLeft;
        private float bodyRight;
        private float bodyTop;
        private float bodyBottom;

        public List<HandlerId> HandlerIds => new List<HandlerId>()
        { 
            HandlerType.SliderValueList.ToHandlerId(SliderType.MainSliderValue),
            HandlerType.SliderValueList.ToHandlerId(SliderType.GuessSliderValues)
        };

        public void Start()
        {
            if (SliderBody != null)
            {
                bodyLeft = SliderBody.rect.xMin;
                bodyRight = SliderBody.rect.xMax;
                bodyBottom = SliderBody.rect.yMin;
                bodyTop = SliderBody.rect.yMax;
            }
        }

        public void UpdateValue(List<SliderValueHolder> mainSliderHolders, List<SliderValueHolder> guessSliderHolders)
        {

        }

        private void ShowSliderHolder(SliderValueHolder sliderHolder, bool isMain = true)
        {
            /*GameObject toInstantiate;
            float y;
            if (isMain)
            {
                toInstantiate = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderValue];
                y = (bodyTop + bodyBottom) / 2;
            }
            else
            {
                toInstantiate = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderGuess];
                y = bodyBottom;
            }
            if (sliderHolder.SingleValue != null)
            {
                float x = Helpers.GetRelativePosition(sliderHolder.SingleValue, sliderHolder.min)
            } 
            else if (sliderHolder.ValueRange != null)
            {

            }*/
        }
        public void UpdateValue(List<dynamic> objects)
        {
            UpdateValue(objects[0], objects[1]);
        }
    }
}
