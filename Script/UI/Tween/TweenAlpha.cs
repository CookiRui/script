/*
    author:jlx
*/

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TweenAlpha : TweenBase
{
    public float from = 0;
    public float to = 1;

    CanvasGroup canvasGroup;
    Graphic graphic;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup)
        {
            graphic = GetComponent<Graphic>();
            if (!graphic)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    protected override Tweener getTweener()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = from;
            return canvasGroup.DOFade(to, param.time);
        }
        if (graphic)
        {
            var color = graphic.color;
            color.a = from;
            graphic.color = color;
            return graphic.DOFade(to, param.time);
        }
        return null;
    }
}
