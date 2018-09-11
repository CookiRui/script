/*
    author:jlx
*/

using UnityEngine;
using DG.Tweening;

public class TweenRotation : TweenBase
{
    public bool local = true;
    public Vector3 from;
    public Vector3 to;

    protected override Tweener getTweener()
    {
        if (local)
        {
            transform.localEulerAngles = from;
            return transform.DOLocalRotate(to, param.time, RotateMode.FastBeyond360);
        }
        else
        {
            transform.eulerAngles = from;
            return transform.DORotate(to, param.time, RotateMode.FastBeyond360);
        }
    }
}
