using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class SpriteRecorderBase : MonoBehaviour
{
    void Awake()
    {
        restore();
    }

    protected abstract void restore();

    protected void getSprite(RecorderSprite sprite, UnityAction<Sprite, Material> callBack)
    {
        if(!sprite)
        {
            Debug.LogError("sprite is null");
            return;
        }

        if(callBack.isNull())
        {
            Debug.LogError("callback is null");
            return;
        }

        UIResourceLoader.inst.loadSprite(sprite, callBack);
    }
}
