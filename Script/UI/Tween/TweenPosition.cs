/*
    author:jlx
*/

using UnityEngine;
using DG.Tweening;

public class TweenPosition : TweenBase
{
    public bool local = true;
    public Vector2 from;
    public Vector2 to;

    protected override Tweener getTweener()
    {
        if (local)
        {
            transform.localPosition = from;
            return transform.DOLocalMove(to, param.time);
        }
        else
        {
            transform.position = from;
            return transform.DOMove(to, param.time);
        }
    }
}
