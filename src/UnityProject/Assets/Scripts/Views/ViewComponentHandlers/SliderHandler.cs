using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.UnityObjects;
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
    public class SliderHandler : MonoBehaviour, HandlerInterface
    {
        public RectTransform SliderBody;
        public HandlerScope Scope => HandlerScope.UnityObject;

        private float SliderBodyAspectRatio;
        private float SliderBodyAnchorMinX;
        private float SliderBodyAnchorMaxX;
        private float SliderBodyAnchorMinY;
        private float SliderBodyAnchorMaxY;
        private float SliderBodyAnchorHeight;
        private float SliderMin;
        private float SliderMax;

        public List<HandlerId> HandlerIds => new List<HandlerId>()
        { 
            HandlerType.SliderData.ToHandlerId(),
        };

        public void UpdateValue(SliderDataHolder sliderData)
        {
            (float, float) sliderBounds = sliderData.SliderBounds;
            List<(float, string)> tickLabels = sliderData.TickLabels;
            List<SliderValueHolder> mainSliderHolders = sliderData.MainSliderHolders;
            List<SliderValueHolder> guessSliderHolders = sliderData.GuessSliderHolders;

            SliderBodyAspectRatio = gameObject.GetComponent<FixedAspectRatio>().AspectRatio;
            SliderBodyAnchorMinX = SliderBody.anchorMin.x;
            SliderBodyAnchorMaxX = SliderBody.anchorMax.x;
            SliderBodyAnchorMinY = SliderBody.anchorMin.y;
            SliderBodyAnchorMaxY = SliderBody.anchorMax.y;
            SliderBodyAnchorHeight = SliderBody.anchorMax.y - SliderBody.anchorMin.y;
            SliderMin = sliderBounds.Item1;
            SliderMax = sliderBounds.Item2;

            if (tickLabels != null)
            {
                foreach ((float location, string text) in tickLabels)
                {
                    CreateLabel(location, text);
                }
            }
            if (mainSliderHolders != null)
            {
                foreach (SliderValueHolder sliderHolder in mainSliderHolders)
                {
                    ShowSliderHolder(sliderHolder, mainSliderHolders.Count(), true);
                }
            }
            if (guessSliderHolders != null)
            {
                foreach (SliderValueHolder sliderHolder in guessSliderHolders)
                {
                    ShowSliderHolder(sliderHolder, guessSliderHolders.Count(), false);
                }
            }
        }

        private void ShowSliderHolder(SliderValueHolder sliderHolder, int numAnswers = 1, bool isMain = true)
        {
            GameObject toInstantiate;
            float y;
            float anchorHeight;
            if (isMain)
            {
                toInstantiate = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderValue];
                y = (SliderBodyAnchorMaxY + SliderBodyAnchorMinY) / 2;
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
                    min2: SliderBodyAnchorMinX,
                    max2: SliderBodyAnchorMaxX);

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

                CreateRange(x1, x2, y, anchorHeight * 0.9f, numAnswers);
                //CreateTick(toInstantiate, x1, y, anchorHeight);
                //CreateTick(toInstantiate, x2, y, anchorHeight);
            }
        }
        private void CreateLabel(float location, string text)
        {
            float x = Helpers.GetRelativePosition(
                    x1: location,
                    min1: SliderMin,
                    max1: SliderMax,
                    min2: SliderBodyAnchorMinX,
                    max2: SliderBodyAnchorMaxX);
            float y = (1.0f + SliderBodyAnchorMaxY) / 2;
            float anchorHeight = 1.0f - SliderBodyAnchorMaxY;
            float anchorWidth = anchorHeight / SliderBodyAspectRatio;

            GameObject createdLabel = Instantiate(PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderTickLabel], transform);
            RectTransform createdRectTransform = createdLabel.GetComponent<RectTransform>();

            createdRectTransform.anchorMin = new Vector2(x - anchorWidth / 2, y - anchorHeight / 2);
            createdRectTransform.anchorMax = new Vector2(x + anchorWidth / 2, y + anchorHeight / 2);
            createdRectTransform.sizeDelta = new Vector2(0, 0);

            createdLabel.GetComponentInChildren<Text>().text = text;
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
        private void CreateRange(float x1, float x2, float y, float anchorHeight, int numAnswers)
        {
            GameObject createdRange = Instantiate(PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.SliderRange], transform);
            RectTransform createdRectTransform = createdRange.GetComponent<RectTransform>();

            createdRectTransform.anchorMin = new Vector2(x1, y - anchorHeight / 2); 
            createdRectTransform.anchorMax = new Vector2(x2, y + anchorHeight / 2);
            createdRectTransform.sizeDelta = new Vector2(0, 0);

            Image rangeImage = createdRange.GetComponent<Image>();
            rangeImage.color = new Color(rangeImage.color.r, rangeImage.color.g, rangeImage.color.b, 1f / numAnswers);
            //createdRange.GetComponent<SliderColorizer>().Colorize(colorId);
        }
        public void UpdateValue(List<object> objects)
        {
            UpdateValue((SliderDataHolder) objects[0]);
        }
    }
}
