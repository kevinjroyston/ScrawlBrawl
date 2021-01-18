using Assets.Scripts.Extensions;
using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
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

            RevealImageAnimation revealAnimation = gameObject.GetComponent<RevealImageAnimation>();
            if (revealAnimation != null && UnityObject.Options.ShouldRevealThisObject())
            {
                revealAnimation.RegisterForRevealEvent();
            }
        }
        private void Start()
        {
            /*EventSystem.Singleton.RegisterListener(
              listener: (gameEvent) => SetActiveAllChildren(transform, false),
              gameEvent: new GameEvent { eventType = EventEnum.ExitingState },
              persistant: true);*/
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
                List<object> values = new List<object>();
                foreach (HandlerId handlerId in handlerInterface.HandlerIds)
                {
                    switch (handlerId.HandlerType)
                    {
                        case HandlerType.IdList:
                            switch (handlerId.SubType)
                            {
                                case IdType.Object_UsersWhoVotedFor: values.Add(UnityObject.UsersWhoVotedFor); break;
                                case IdType.Object_OwnerIds: values.Add(new List<Guid>() { UnityObject.OwnerUserId.GetValueOrDefault(UnityObject.UnityObjectId) }); break;
                                default: values.Add(CheckSubHandlers(handlerId)); break;
                            }
                            break;
                        case HandlerType.Strings:
                            switch (handlerId.SubType)
                            {
                                case StringType.Object_Title: values.Add(UnityObject.Title); break;
                                case StringType.Object_Header: values.Add(UnityObject.Header); break;
                                case StringType.Object_Footer: values.Add(UnityObject.Footer); break;
                                case StringType.Object_ImageIdentifier: values.Add(UnityObject.ImageIdentifier); break;
                                default: values.Add(CheckSubHandlers(handlerId)); break;
                            }
                            break;
                        case HandlerType.Ints:
                            switch (handlerId.SubType)
                            {
                                case IntType.Object_VoteCount: values.Add(UnityObject.VoteCount); break;
                                default: values.Add(CheckSubHandlers(handlerId)); break;
                            }
                            break;
                        case HandlerType.ObjectOptions:
                            values.Add(UnityObject.Options);
                            break;
                        default: 
                            values.Add(CheckSubHandlers(handlerId));
                            break;
                    }
                }
                Helpers.SetActiveAndUpdate(handlerComponent, values);
            }

            foreach (ScopeLoadedListener loadedListener in gameObject.GetComponentsInChildren<ScopeLoadedListener>())
            {
                if (loadedListener.Scope == HandlerScope.UnityObject)
                {
                    loadedListener.OnCompleteLoad();
                }
            }
        }

        private object CheckSubHandlers(HandlerId handlerId)
        {
            switch (UnityObject.Type)
            {
                case UnityObjectType.Image:
                    return UnityImageSubHandler(handlerId);
                case UnityObjectType.Text:
                    // No extra data in Text for it to be listening for, dont know what they were looking for
                    throw new ArgumentException($"Unknown handler id: '{handlerId.SubType}'");
                case UnityObjectType.Slider:
                    return UnitySliderSubHandler(handlerId);
                case UnityObjectType.TextStack:
                    return UnityTextStackSubHandler(handlerId);
                default:
                    throw new ArgumentException($"Unknown Object Type: : '{UnityObject.Type}'");
            }
        }
        private void UpdateHandlers()
        {
            Handlers = gameObject.GetComponentsInChildren(typeof(HandlerInterface)).ToList();
        }

        #region ObjectType SubHandlers
        private object UnityImageSubHandler(HandlerId handlerId)
        {
            UnityImage castedImage = (UnityImage)UnityObject;
            switch (handlerId.HandlerType)
            {
                case HandlerType.Sprite:
                    return new SpriteHolder()
                    {
                        Sprites = castedImage.Sprites,
                        SpriteGridWidth = castedImage.SpriteGridWidth,
                        SpriteGridHeight = castedImage.SpriteGridHeight,
                        BackgroundColor = castedImage.BackgroundColor
                    };
                default:
                    throw new ArgumentException($"Unknown handler id: '{handlerId.SubType}'");
            }
        }

        private object UnitySliderSubHandler(HandlerId handlerId)
        {
            UnitySlider castedSlider = (UnitySlider)UnityObject;
            switch (handlerId.HandlerType)
            {
                case HandlerType.SliderData:
                    return new SliderDataHolder()
                    {
                        SliderBounds = castedSlider.SliderBounds,
                        TickLabels = castedSlider.TickLabels,
                        MainSliderHolders = castedSlider.MainSliderValues,
                        GuessSliderHolders = castedSlider.GuessSliderValues,
                    };
                default:
                    throw new ArgumentException($"Unknown handler id: '{handlerId.HandlerType}'");

            }
        }

        private object UnityTextStackSubHandler(HandlerId handlerId)
        {
            UnityTextStack castedStack = (UnityTextStack)UnityObject;
            switch (handlerId.HandlerType)
            {
                case HandlerType.Ints:
                    switch (handlerId.SubType)
                    {
                        case IntType.Object_TextStackFixedNumItems:
                            return castedStack.FixedNumItems;
                        default:
                            throw new ArgumentException($"Unknown handler subtype: '{handlerId.SubType}'");
                    }
                case HandlerType.TextStackList:
                    return castedStack.TextStackList;
                default:
                    throw new ArgumentException($"Unknown handler id: '{handlerId.HandlerType}'");

            }
        }
        #endregion
    }

}
