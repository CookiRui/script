/*
    author:jlx
*/

using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System;

[Serializable]
public class TweenParam
{
    public bool autoPlay = true;
    public bool ignoreTimescale;
    public bool loop = false;
    public float time = 1;
    public float delay = 0;
    public Ease ease = Ease.Linear;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
}

public abstract class TweenBase : MonoBehaviour
{
    public bool show = true;
    public TweenParam param = new TweenParam();
    public UnityAction oncomplete;
    public bool remove;
    public bool panel;

    Tweener tweener;
    bool started = false;
    void OnEnable()
    {
        if (started)
        {
            autoPlay();
        }
    }

    void OnDisable()
    {
        stop();
    }

    void Start()
    {
        started = true;
        autoPlay();
    }

    void autoPlay()
    {
        if (show && param.autoPlay)
        {
            play();
        }
    }

    public void play()
    {
        stop();
        playTween();
    }

    void playTween()
    {
        tweener = getTweener();
        if (tweener == null) return;
        if (param.ease == Ease.Unset)
        {
            tweener.SetEase(param.curve);
        }
        else
        {
            tweener.SetEase(param.ease);
        }
        tweener.SetDelay(param.delay)
                .SetUpdate(param.ignoreTimescale)
                .SetLoops(param.loop?-1:0)       
                .OnComplete(complete)
                .Play();
    }

    void complete()
    {
        if (oncomplete == null)
            return;
        oncomplete();
    }

    public void stop()
    {
        if (tweener == null) return;
        tweener.Kill();
        tweener = null;
    }

    protected abstract Tweener getTweener();
}
