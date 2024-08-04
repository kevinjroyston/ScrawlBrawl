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

    bool startHasBeenCalled = false;
    public void Start()
    {
        startHasBeenCalled = true;
        RegisterListener();
    }

    // Need to register the listener on every enable, because we have to remove it on every disable. But the enable happens during awake, so referencing singletons is a no go. Hence the ugly logic
    public void OnEnable()
    {
        if (!startHasBeenCalled)
        {
            // Rather than do any initialization here, we just wait for start to be called. Singletons will not have been set up until after Awake() phase finishes (OnEnable is part of that phase initially)
            return;
        }

        // The first register listener call cannot happen until start is called. But SUBSEQUENT calls to enable should register the listener.
        RegisterListener();
    }
    private void RegisterListener()
    {
        EventSystem.Singleton.RegisterListener(
            listener: ListenerCallback,
            gameEvent: new GameEvent { eventType = GameEvent.EventEnum.RevealImages }, // Bit janky, technically this event is not guaranteed to occur if no images are marked for reveal.
            persistant: true,
            oneShot: false);
    }
    public void OnDisable()
    {
        // Could not find a way to avoid calling this listener while disabled. So instead we remove and re-add it as the object disables/enables
        EventSystem.Singleton.RemoveListener(ListenerCallback);
    }

    private void ListenerCallback(GameEvent gameEvent) => UpdateTextField(PostRevealText);

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
