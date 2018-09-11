using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageRecorder : SpriteRecorderBase
{
    public RecorderSprite sprite;

    protected override void restore()
    {
        var image = GetComponent<Image>();
        if(!image)
        {
            Debug.LogError("image is null");
            return;
        }

        if(!sprite)
        {
            Debug.LogError("sprite is null");
            return;
        }

        getSprite(sprite, (s, m) => { if (image) image.sprite = s; });
    }
}
