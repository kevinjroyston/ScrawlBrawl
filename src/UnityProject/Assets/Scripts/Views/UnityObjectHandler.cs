using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static GameEvent;

namespace Assets.Scripts.Views
{
    public class UnityObjectHandler : MonoBehaviour
    {
        private UnityObject UnityObject { get; set; }
        public UnityObjectType UnityObjectType => UnityObject.Type;
        private List<Component> SpriteHandlers { get; set; } = new List<Component>();
        private List<Component> StringHandlers { get; set; } = new List<Component>();
        private List<Component> IntHandlers { get; set; } = new List<Component>();
        private List<Component> ObjectOptionHandlers { get; set; } = new List<Component>();

        /// <summary>
        /// first float is inner(image grid) aspect ratio, second float is outer(entire card w/o padding) aspect ratio for perfect fit UI
        /// </summary>
        private List<Action<float, float>> AspectRatioListeners = new List<Action<float, float>>();

        public void HandleUnityObject(UnityObject unityObject)
        {
            UnityObject = unityObject;
            UpdateHandlers();
            CallHandlers();


            if ((bool) (unityObject.Options?[UnityObjectOptions.RevealThisImage] ?? false)) // only shake the objects if one of them is going to be revealed
            {
                EventSystem.Singleton.RegisterListener(
                    listener: (GameEvent gameEvent) => EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ShakeRevealImages }, allowDuplicates: false),
                    gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.CallShakeRevealImages });
            }
            else
            {
                EventSystem.Singleton.RegisterListener(
                   listener: (GameEvent gameEvent) => EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ShowDeltaScores }, allowDuplicates: false),
                   gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.CallShakeRevealImages });
            }

               /* /// Aspect ratio shenanigans
                float innerAspectRatio = ((float)gridColCount) / ((float)gridRowCount) * aspectRatio;
                // Code doesnt work if there are no inner images, in this case just default to A.R. 2.0
                float outerAspectRatio = 2f;

                if (GetFlexibleHeightOrDefault(SpriteZoneHolder) > 0.01f)
                {
                    outerAspectRatio =
                         innerAspectRatio
                         / (GetFlexibleHeightOrDefault(Title)
                             + GetFlexibleHeightOrDefault(Header)
                             + GetFlexibleHeightOrDefault(ScoreHolder)
                             + GetFlexibleHeightOrDefault(Footer)
                             + GetFlexibleHeightOrDefault(DummyScore)
                             + GetFlexibleHeightOrDefault(SpriteZoneHolder))
                         * GetFlexibleHeightOrDefault(SpriteZoneHolder);
                }
                CallAspectRatioListeners(innerAspectRatio, outerAspectRatio);*/
        }
        private void Start()
        {
            /*EventSystem.Singleton.RegisterListener(
              listener: (gameEvent) => SetActiveAllChildren(transform, false),
              gameEvent: new GameEvent { eventType = EventEnum.ExitingState },
              persistant: true);*/
        }


        private float lastUsedInnerAspectRatio = 1f;
        private float lastUsedOuterAspectRatio = 1f;
        public void RegisterAspectRatioListener(Action<float, float> listener)
        {
            AspectRatioListeners.Add(listener);
            listener.Invoke(lastUsedInnerAspectRatio, lastUsedOuterAspectRatio);
        }

        public float minAspectRatio = .3f;
        public float maxAspectRatio = 3.3f;
        private void CallAspectRatioListeners(float innerValue, float outerValue)
        {
            if (innerValue < minAspectRatio)
            {
                innerValue = minAspectRatio;
            }
            else if (innerValue > maxAspectRatio)
            {
                innerValue = maxAspectRatio;
            }
            if (outerValue < minAspectRatio)
            {
                outerValue = minAspectRatio;
            }
            else if (outerValue > maxAspectRatio)
            {
                outerValue = maxAspectRatio;
            }
            lastUsedInnerAspectRatio = innerValue;
            lastUsedOuterAspectRatio = outerValue;
            foreach (var func in AspectRatioListeners)
            {
                // Tells the listeners what size this layout group would ideally like to be
                func.Invoke(innerValue, outerValue);
            }
        }
        private float GetFlexibleHeightOrDefault(Component comp, float defaultValue = 0f)
        {
            return GetFlexibleHeightOrDefault(comp?.gameObject, defaultValue);
        }
        private float GetFlexibleHeightOrDefault(GameObject obj, float defaultValue = 0f)
        {
            float? flexHeight = obj?.transform?.GetComponent<LayoutElement>()?.flexibleHeight;
            return ((obj?.activeInHierarchy == true) && flexHeight.HasValue) ? flexHeight.Value : defaultValue;
        }
        private void CallColorizers(string identifier)
        {
            Colorizer[] colorizers = GetComponentsInChildren<Colorizer>();
            if (colorizers != null)
            {
                foreach (Colorizer colorizer in colorizers)
                {
                    colorizer.RefreshColor(identifier);
                }
            }
        }


        private void CallHandlers()
        {
            foreach (Component stringHandler in StringHandlers)
            {
                UnityField<string> value = null;
                switch (((Strings_HandlerInterface)stringHandler).Type)
                {
                    case StringType.Object_Title: value = UnityObject.Title; break;
                    case StringType.Object_Header: value = UnityObject.Header; break;
                    case StringType.Object_Footer: value = UnityObject.Footer; break;
                    case StringType.Object_ImageIdentifier: value = UnityObject.ImageIdentifier; break;
                    default: break;
                }
                Helpers.SetActiveAndUpdate(stringHandler, value);
            }

            foreach (Component spriteHandler in SpriteHandlers)
            {
                Helpers.SetActiveAndUpdate(
                    spriteHandler, 
                    new SpriteHolder()
                    {
                        Sprites = UnityObject.Sprites,
                        SpriteGridWidth = UnityObject.SpriteGridWidth,
                        SpriteGridHeight = UnityObject.SpriteGridHeight,
                        BackgroundColor = UnityObject.BackgroundColor
                    });

            }

            foreach (Component intHandler in IntHandlers)
            {
                if (((Ints_HandlerInterface)intHandler).Type == IntType.Object_VoteCount)
                {
                    Helpers.SetActiveAndUpdate(intHandler, UnityObject.VoteCount);
                }
            }
                
            foreach (Component objectOptionsHandler in ObjectOptionHandlers)
            {
                Helpers.SetActiveAndUpdate(objectOptionsHandler, UnityObject.Options);
            }
        }

        private void UpdateHandlers()
        {
            StringHandlers = gameObject.GetComponentsInChildren(typeof(Strings_HandlerInterface)).ToList();
            SpriteHandlers = gameObject.GetComponentsInChildren(typeof(Sprite_HandlerInterface)).ToList();
            IntHandlers = gameObject.GetComponentsInChildren(typeof(Ints_HandlerInterface)).ToList();
            ObjectOptionHandlers = gameObject.GetComponentsInChildren(typeof(Options_HandlerInterface<UnityObjectOptions>)).ToList();
        }
    }

}
