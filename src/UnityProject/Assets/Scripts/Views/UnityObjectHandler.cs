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
using static TypeEnums;

namespace Assets.Scripts.Views
{
    public class UnityObjectHandler : MonoBehaviour
    {
        private UnityObject UnityObject { get; set; }
        public UnityObjectType UnityObjectType => UnityObject.Type;
        private List<Component> Handlers { get; set; } = new List<Component>();

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
            foreach (Component handlerComponent in Handlers)
            {
                HandlerInterface handlerInterface = (HandlerInterface)handlerComponent;
                if (handlerInterface.Scope != HandlerScope.UnityObject)
                {
                    continue;
                }
                List<dynamic> values = new List<dynamic>();
                foreach (HandlerId handlerId in handlerInterface.HandlerIds)
                {
                    switch (handlerId.HandlerType)
                    {
                        case HandlerType.Sprite:
                            values.Add(
                                new SpriteHolder()
                                {
                                    Sprites = UnityObject.Sprites,
                                    SpriteGridWidth = UnityObject.SpriteGridWidth,
                                    SpriteGridHeight = UnityObject.SpriteGridHeight,
                                    BackgroundColor = UnityObject.BackgroundColor
                                });
                            break;
                        case HandlerType.IdList:
                            switch (handlerId.SubType)
                            {
                                case IdType.Object_UsersWhoVotedFor: values.Add(UnityObject.UsersWhoVotedFor); break;
                                default:
                                    throw new ArgumentException($"Unknown subtype id: '{HandlerType.IdList}-{handlerId.SubType}'");
                            }
                            break;
                        case HandlerType.Strings:
                            switch (handlerId.SubType)
                            {
                                case StringType.Object_Title: values.Add(UnityObject.Title); break;
                                case StringType.Object_Header: values.Add(UnityObject.Header); break;
                                case StringType.Object_Footer: values.Add(UnityObject.Footer); break;
                                case StringType.Object_ImageIdentifier: values.Add(UnityObject.ImageIdentifier); break;
                                default:
                                    throw new ArgumentException($"Unknown subtype id: '{HandlerType.Strings}-{handlerId.SubType}'");
                            }
                            break;
                        case HandlerType.Ints:
                            switch (handlerId.SubType)
                            {
                                case IntType.Object_VoteCount: values.Add(UnityObject.VoteCount); break;
                                default:
                                    throw new ArgumentException($"Unknown subtype id: '{HandlerType.Ints}-{handlerId.SubType}'");
                            }
                            break;
                        case HandlerType.ObjectOptions:
                            values.Add(UnityObject.Options);
                            break;
                        default:
                            throw new ArgumentException($"Unknown handler id: '{handlerId.SubType}'");
                    }
                    Helpers.SetActiveAndUpdate(handlerComponent, values);
                }
            }
        }

        private void UpdateHandlers()
        {
            Handlers = gameObject.GetComponentsInChildren(typeof(HandlerInterface)).ToList();
        }
    }

}
