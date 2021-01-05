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

        private float SliderBodyAspectRatio;
        private float SliderBodyAnchorMinX;
        private float SliderBodyAnchorMaxX;
        private float SliderBodyAnchorMinY;
        private float SliderBodyAnchorHeight;
        private float SliderMin;
        private float SliderMax;

        public List<HandlerId> HandlerIds => new List<HandlerId>()
        { 
            HandlerType.SliderBoundsTuple.ToHandlerId(),
            HandlerType.SliderValueList.ToHandlerId(SliderType.MainSliderValue),
            HandlerType.SliderValueList.ToHandlerId(SliderType.GuessSliderValues)
        };

        public void UpdateValue((float, float) sliderBounds, List<SliderValueHolder> mainSliderHolders, List<SliderValueHolder> guessSliderHolders)
        {
            SliderBodyAspectRatio = gameObject.GetComponent<FixedAspectRatio>().AspectRatio;
            SliderBodyAnchorMinX = SliderBody.anchorMin.x;
            SliderBodyAnchorMaxX = SliderBody.anchorMax.x;
            SliderBodyAnchorMinY = SliderBody.anchorMin.y;
            SliderBodyAnchorHeight = SliderBody.anchorMax.y - SliderBody.anchorMin.y;
            SliderMin = sliderBounds.Item1;
            SliderMax = sliderBounds.Item2;

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
            float anchorHeight;
            if (isMain)
            {
                toInstantiate = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderValue];
                y = 0.5f;
                anchorHeight = SliderBodyAnchorHeight * 1.05f;
            }
            else
            {
                toInstantiate = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderGuess];
                y = SliderBodyAnchorMinY;
                anchorHeight = SliderBodyAnchorHeight * 0.3f;
            }
            if (sliderHolder.SingleValue != null)
            {
                float x = Helpers.GetRelativePosition(
                    x1: (float)sliderHolder.SingleValue,
                    min1: SliderMin,
                    max1: SliderMax,
                    min2: 0f,
                    max2: 1f);

                CreateTick(toInstantiate, x, y, anchorHeight, sliderHolder.UserId.ToString());
            } 
            else if (sliderHolder.ValueRange != null)
            {
                float x1 = Helpers.GetRelativePosition(
                    x1: (((float, float)) sliderHolder.ValueRange).Item1,
                    min1: SliderMin,
                    max1: SliderMax,
                    min2: SliderBodyAnchorMinX,
                    max2: SliderBodyAnchorMaxX);

                float x2 = Helpers.GetRelativePosition(
                    x1: (((float, float))sliderHolder.ValueRange).Item2,
                    min1: SliderMin,
                    max1: SliderMax,
                    min2: SliderBodyAnchorMinX,
                    max2: SliderBodyAnchorMaxX);

                CreateRange(x1, x2, y, anchorHeight * 0.75f, sliderHolder.UserId.ToString());
                //CreateTick(toInstantiate, x1, y, anchorHeight);
                //CreateTick(toInstantiate, x2, y, anchorHeight);
            }
        }
        private void CreateTick(GameObject toInstantiate, float x, float y, float anchorHeight, string colorId)
        {
            float anchorWidth = anchorHeight / SliderBodyAspectRatio;

            GameObject createdTick = Instantiate(toInstantiate, transform);
            RectTransform createdRectTransform = createdTick.GetComponent<RectTransform>();

            createdRectTransform.anchorMin = new Vector2(x - anchorWidth / 2, y - anchorHeight / 2);
            createdRectTransform.anchorMax = new Vector2(x + anchorWidth / 2, y + anchorHeight / 2);
            createdRectTransform.sizeDelta = new Vector2(0, 0);

            createdTick.GetComponent<SliderColorizer>().Colorize(colorId);
        }
        private void CreateRange(float x1, float x2, float y, float anchorHeight, string colorId)
        {
            GameObject createdRange = Instantiate(PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderRange], transform);
            RectTransform createdRectTransform = createdRange.GetComponent<RectTransform>();

            createdRectTransform.anchorMin = new Vector2(x1, y - anchorHeight / 2); 
            createdRectTransform.anchorMax = new Vector2(x2, y + anchorHeight / 2);
            createdRectTransform.sizeDelta = new Vector2(0, 0);

            createdRange.GetComponent<SliderColorizer>().Colorize(colorId);
        }
        public void UpdateValue(List<dynamic> objects)
        {
            UpdateValue(objects[0], objects[1], objects[2]);
        }
    }
}
