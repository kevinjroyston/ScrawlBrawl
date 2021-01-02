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

        public void UpdateValue((float, float) sliderBounds, List<SliderValueHolder> mainSliderHolders, List<SliderValueHolder> guessSliderHolders)
        {
            bodyLeft = -SliderBody.sizeDelta.x / 2;
            bodyRight = SliderBody.sizeDelta.x / 2;
            bodyBottom = -SliderBody.sizeDelta.y / 2;
            bodyTop = SliderBody.sizeDelta.y / 2;

            sliderMin = sliderBounds.Item1;
            sliderMax = sliderBounds.Item2;
            if (mainSliderHolders != null)
            {
                foreach (SliderValueHolder sliderHolder in mainSliderHolders)
                {
                    ShowSliderHolder(sliderHolder, true);
                }
            }
            if (guessSliderHolders != null)
            {
                foreach (SliderValueHolder sliderHolder in guessSliderHolders)
                {
                    ShowSliderHolder(sliderHolder, false);
                }
            }
        }

        private void ShowSliderHolder(SliderValueHolder sliderHolder, bool isMain = true)
        {
            GameObject toInstantiate;
            float y;
            if (isMain)
            {
                toInstantiate = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderValue];
                y = 0.5f;
            }
            else
            {
                toInstantiate = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderGuess];
                y = 0f;
            }
            if (sliderHolder.SingleValue != null)
            {
                float x = Helpers.GetRelativePosition(
                    x1: (float)sliderHolder.SingleValue,
                    min1: sliderMin,
                    max1: sliderMax,
                    min2: 0f,
                    max2: 1f);
                GameObject singleValueTick = Instantiate(toInstantiate, transform);
                singleValueTick.GetComponent<RectTransform>().anchorMin = new Vector2(x, y);
                singleValueTick.GetComponent<RectTransform>().anchorMax = new Vector2(x, y);
            } 
            else if (sliderHolder.ValueRange != null)
            {
                float x1 = Helpers.GetRelativePosition(
                    x1: (((float, float)) sliderHolder.ValueRange).Item1,
                    min1: sliderMin,
                    max1: sliderMax,
                    min2: 0,
                    max2: 1);

                float x2 = Helpers.GetRelativePosition(
                    x1: (((float, float))sliderHolder.ValueRange).Item2,
                    min1: sliderMin,
                    max1: sliderMax,
                    min2: 0,
                    max2: 1);

                GameObject valueRangeRect = Instantiate(PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderRange], transform);
                GameObject valueRangeTick1 = Instantiate(toInstantiate, transform);
                GameObject valueRangeTick2 = Instantiate(toInstantiate, transform);

                valueRangeRect.GetComponent<RectTransform>().anchorMin = new Vector2(x1, y - 0.1f);
                valueRangeRect.GetComponent<RectTransform>().anchorMax = new Vector2(x2, y + 0.1f);
                valueRangeRect.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

                valueRangeTick1.GetComponent<RectTransform>().anchorMin = new Vector2(x1, y);
                valueRangeTick1.GetComponent<RectTransform>().anchorMax = new Vector2(x1, y);
                valueRangeTick2.GetComponent<RectTransform>().anchorMin = new Vector2(x2, y);
                valueRangeTick2.GetComponent<RectTransform>().anchorMax = new Vector2(x2, y);

            }
        }
        public void UpdateValue(List<dynamic> objects)
        {
            UpdateValue(objects[0], objects[1], objects[2]);
        }
    }
}
