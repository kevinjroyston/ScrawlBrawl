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

        private float sliderBodyAspectRatio;
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
            sliderBodyAspectRatio = 

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
                RectTransform singleValueRectTrans = singleValueTick.GetComponent<RectTransform>();
                singleValueRectTrans.anchorMin = new Vector2(x, y);
                singleValueRectTrans.anchorMax = new Vector2(x, y);
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
                RectTransform valueRangeRectTransform = valueRangeRect.GetComponent<RectTransform>();
                RectTransform valueRangeRectTransform1 = valueRangeTick1.GetComponent<RectTransform>();
                RectTransform valueRangeRectTransform2 = valueRangeTick2.GetComponent<RectTransform>();

                valueRangeRectTransform.anchorMin = new Vector2(x1, y - 0.1f);
                valueRangeRectTransform.anchorMax = new Vector2(x2, y + 0.1f);
                valueRangeRectTransform.sizeDelta = new Vector2(0, 0);

                valueRangeRectTransform1.anchorMin = new Vector2(x1, y);
                valueRangeRectTransform1.anchorMax = new Vector2(x1, y);
                valueRangeRectTransform2.anchorMin = new Vector2(x2, y);
                valueRangeRectTransform2.anchorMax = new Vector2(x2, y);

            }
        }
        private void CreateTick(GameObject toInstantiate, float x, float y)
        {
            GameObject createdTick = Instantiate(toInstantiate, transform);
            RectTransform createdRectTransform = createdTick.GetComponent<RectTransform>();
            createdRectTransform.anchorMin = new Vector2(x, y);
            createdRectTransform.anchorMax = new Vector2(x, y);
        }
        private float CalculateAspectRatio()
        {
            RectTransform holderRectTransform = gameObject.GetComponent<RectTransform>();
            float bodyAnchorWidth = SliderBody.anchorMax.x - SliderBody.anchorMin.x;
            float bodyAnchorHeight = SliderBody.anchorMax.y - SliderBody.anchorMin.y;

            float bodyRelativeWidth = holderRectTransform.rect.width * bodyAnchorWidth;
            float bodyRelativeHeight = holderRectTransform.rect.width * bodyAnchorHeight;

            SliderBody.rect.w
        }
        public void UpdateValue(List<dynamic> objects)
        {
            UpdateValue(objects[0], objects[1], objects[2]);
        }
    }
}
