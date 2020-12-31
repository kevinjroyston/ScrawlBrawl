using Assets.Scripts.Extensions;
using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
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
                                case IdType.Object_OwnerIds: values.Add(new List<Guid>() { UnityObject.ImageOwnerId.GetValueOrDefault(UnityObject.UnityObjectId) }); break;
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

        private void UpdateHandlers()
        {
            Handlers = gameObject.GetComponentsInChildren(typeof(HandlerInterface)).ToList();
        }
    }

}
