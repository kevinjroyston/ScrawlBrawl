using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using static TypeEnums;
using System.Text.RegularExpressions;

public class GenericTextHandler : MonoBehaviour, HandlerInterface
{
    public Text TextComponent;

    public List<HandlerId> HandlerIds => HandlerType.Strings.ToHandlerIdList(this.textType);
    public HandlerScope Scope => textType.GetScope();
    public StringType textType;

    // Just a simple before/after to support custom "hideDuringDrumroll" tag, not trying to animate anything currently
    private string PreRevealText;
    private string PostRevealText;

    /// <summary>
    /// THIS IS A VERY NAIVE IMPLEMENTATION OF TAGS.
    /// 
    /// There cant be any nested tags inside of the custom tags listed here. It does parse out values though, so thats kind of neat.
    /// </summary>
    private Regex tagRegex = new Regex(@"<(?<tag>hideDuringDrumroll)=*(?<value>[^>]*)>(?<body>[^<]*)<\/\1>");

    public void Start()
    {
        // Set not to persist. Should get cleaned up when the view is destroyed. Might as well OneShot it too.
        EventSystem.Singleton.RegisterListener(
            listener: (gameEvent) => UpdateTextField(PostRevealText),
            gameEvent: new GameEvent { eventType = GameEvent.EventEnum.RevealImages }, // Bit janky, technically this event is not guaranteed to occur if no images are marked for reveal.
            oneShot: true
        );
    }

    public void UpdateValue(UnityField<string> field)
    {
        // In either case we remove the tag. But we start with ????s and move to the real text after reveal.
        PreRevealText = tagRegex.Replace(field?.Value ?? string.Empty, "?????");
        PostRevealText = tagRegex.Replace(field?.Value ?? string.Empty, "${body}");

        UpdateTextField(PreRevealText);
    }

    private void UpdateTextField(string text)
    {
        if (TextComponent != null)
        {
            TextComponent.text = text ?? string.Empty;
        }

        // SUPER hacky, this should be an interface or listener or something
        var maxSizeScript = gameObject.GetComponent<IdentifierMaxSize>();
        if (maxSizeScript != null)
        {
            maxSizeScript.OnGUI();
        }
    }

    public void UpdateValue(List<object> objects)
    {
        UpdateValue((UnityField<string>) objects[0]);
    }
}
