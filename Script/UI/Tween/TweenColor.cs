/*
    author:jlx
*/

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TweenColor : TweenBase
{
    /*
    Material
    Camera
    Light
    LineRenderer
    SpriteRenderer
    Graphic
    Image
    Outline
        */
    public Color from = Color.white;
    public Color to = Color.white;
    protected override Tweener getTweener()
    {
        var graphic = GetComponent<Graphic>();
        if (!graphic) return null;
        graphic.color = from;
        return graphic.DOColor(to, param.time);
    }
}
