/*
    author:jlx
*/

using UnityEngine;
using DG.Tweening;

public class TweenScale : TweenBase
{
    public Vector3 from;
    public Vector3 to = Vector3.one;
    protected override Tweener getTweener()
    {
        transform.localScale = from;
        return transform.DOScale(to, param.time);
    }
}
