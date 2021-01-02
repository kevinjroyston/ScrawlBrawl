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
        private float sliderMin;
        private float sliderMax;

        public List<HandlerId> HandlerIds => new List<HandlerId>()
        { 
            HandlerType.SliderBoundsTuple.ToHandlerId(),
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

        public void UpdateValue((float, float) sliderBounds, List<SliderValueHolder> mainSliderHolders, List<SliderValueHolder> guessSliderHolders)
        {
            sliderMin = sliderBounds.Item1;
            sliderMax = sliderBounds.Item2;
            foreach (SliderValueHolder sliderHolder in mainSliderHolders)
            {
                ShowSliderHolder(sliderHolder, true);
            }
            foreach (SliderValueHolder sliderHolder in guessSliderHolders)
            {
                ShowSliderHolder(sliderHolder, false);
            }
        }

        private void ShowSliderHolder(SliderValueHolder sliderHolder, bool isMain = true)
        {
            GameObject toInstantiate;
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
                float x = Helpers.GetRelativePosition(
                    x1: (float)sliderHolder.SingleValue,
                    min1: sliderMin,
                    max1: sliderMax,
                    min2: bodyLeft,
                    max2: bodyRight);
                GameObject singleValueTick = Instantiate(toInstantiate, transform);
                singleValueTick.transform.position = new Vector3(x, y, 0);

            } 
            else if (sliderHolder.ValueRange != null)
            {
                float x1 = Helpers.GetRelativePosition(
                    x1: (((float, float)) sliderHolder.ValueRange).Item1,
                    min1: sliderMin,
                    max1: sliderMax,
                    min2: bodyLeft,
                    max2: bodyRight);

                float x2 = Helpers.GetRelativePosition(
                    x1: (((float, float))sliderHolder.ValueRange).Item2,
                    min1: sliderMin,
                    max1: sliderMax,
                    min2: bodyLeft,
                    max2: bodyRight);

                GameObject valueRangeRect = Instantiate(PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderRange], transform);
                GameObject valueRangeTick1 = Instantiate(toInstantiate, transform);
                GameObject valueRangeTick2 = Instantiate(toInstantiate, transform);

                valueRangeRect.transform.position = new Vector3((x1 + x2) / 2, y, 0);
                valueRangeRect.transform.localScale = new Vector3(x2 - x1, 1, 1);

                valueRangeTick1.transform.position = new Vector3(x1, y, 0);
                valueRangeTick2.transform.position = new Vector3(x2, y, 0);
            }
        }
        public void UpdateValue(List<dynamic> objects)
        {
            UpdateValue(objects[0], objects[1], objects[2]);
        }
    }
}
