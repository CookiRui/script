using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteStateRecorder : SpriteRecorderBase
{
    public List<RecorderSprite> sprites = new List<RecorderSprite>();
    Selectable selectable;

    protected override void restore()
    {
        selectable = GetComponent<Selectable>();
        if(!selectable)
        {
            Debug.LogError("selectable is null");
            return;
        }

        if(selectable.transition != Selectable.Transition.SpriteSwap)
        {
            Debug.LogError("Transition != SpriteSwap : " + selectable.transition);
            return;
        }

        if(sprites.isNullOrEmpty())
        {
            Debug.LogError("sprites is null");
            return;
        }

        sprites.ForEach(a =>
        {
            getSprite(a, (s, m) =>
            {
                if(selectable)
                {
                    var state = selectable.spriteState;
                    switch(a.state)
                    {
                        case SelectionState.Highlighted: state.highlightedSprite = s; break;
                        case SelectionState.Pressed: state.pressedSprite = s; break;
                        case SelectionState.Disabled: state.disabledSprite = s; break;
                    }
                    selectable.spriteState = state;
                }
            });
        });
    }
}
