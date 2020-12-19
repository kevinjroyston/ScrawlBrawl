using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Views
{
    public class UnityObjectHandler : MonoBehaviour
    {
        private UnityObject UnityObject { get; set; }
        private List<Sprite_HandlerInterface> SpriteHandlers { get; set; } = new List<Sprite_HandlerInterface>();
        private List<Strings_HandlerInterface> StringHandlers { get; set; } = new List<Strings_HandlerInterface>();
        private List<Ints_HandlerInterface> IntHandlers { get; set; } = new List<Ints_HandlerInterface>();
        private List<Options_HandlerInterface<UnityObjectOptions>> ObjectOptionHandlers { get; set; } = new List<Options_HandlerInterface<UnityObjectOptions>>();

        public void Initialize(UnityObject unityObject)
        {
            UnityObject = unityObject;
            UpdateHandlers();
            CallHandlers();
        }
        private void Start()
        {
            
        }


        private void CallHandlers()
        {
            foreach (Strings_HandlerInterface stringHandler in StringHandlers)
            {
                if (stringHandler.Type == StringType.Object_Title)
                {
                    stringHandler.UpdateValue(UnityObject.Title);
                }
                else if (stringHandler.Type == StringType.Object_Header)
                {
                    stringHandler.UpdateValue(UnityObject.Header);
                }
                else if (stringHandler.Type == StringType.Object_Footer)
                {
                    stringHandler.UpdateValue(UnityObject.Footer);
                }
                else if (stringHandler.Type == StringType.Object_ImageIdentifier)
                {
                    stringHandler.UpdateValue(UnityObject.ImageIdentifier);
                }
            }

            foreach (Sprite_HandlerInterface spriteHandler in SpriteHandlers)
            {
                spriteHandler.UpdateValue(new SpriteHolder()
                {
                    Sprites = UnityObject.Sprites,
                    SpriteGridWidth = UnityObject.SpriteGridWidth,
                    SpriteGridHeight = UnityObject.SpriteGridHeight,
                    BackgroundColor = UnityObject.BackgroundColor
                });
            }

            foreach (Ints_HandlerInterface intHandler in IntHandlers)
            {
                if (intHandler.Type == IntType.Object_VoteCount)
                {
                    intHandler.UpdateValue(UnityObject.VoteCount);
                }
            }

            foreach (Options_HandlerInterface<UnityObjectOptions> objectOptionsHandler in ObjectOptionHandlers)
            {
                objectOptionsHandler.UpdateValue(UnityObject.Options);
            }
        }

        private void UpdateHandlers()
        {
            StringHandlers = gameObject.GetComponentsInChildren<Strings_HandlerInterface>().ToList();
            SpriteHandlers = gameObject.GetComponentsInChildren<Sprite_HandlerInterface>().ToList();
            IntHandlers = gameObject.GetComponentsInChildren<Ints_HandlerInterface>().ToList();
            ObjectOptionHandlers = gameObject.GetComponentsInChildren<Options_HandlerInterface<UnityObjectOptions>>().ToList();
        }
    }
}
