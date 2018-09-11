/*
    author:jlx
    from:http://answers.unity3d.com/questions/217351/animations-ignore-timescale.html
 */

using UnityEngine;
using System.Collections;
using System;

public static class AnimationExtensions
{
    public static IEnumerator play(this Animation animation, string clipName, bool useTimeScale = false, Action onComplete = null)
    {
        if (animation == null)
        {
            Debug.LogError("animation is null");
            yield break;
        }
        if (string.IsNullOrEmpty(clipName))
        {
            Debug.LogError("clipName is null");
            yield break;
        }

        if (!useTimeScale)
        {
            var curState = animation[clipName];
            var isPlaying = true;
            var progressTime = 0f;
            var timeAtLastFrame = Time.realtimeSinceStartup;
            var timeAtCurrentFrame = 0f;
            var deltaTime = 0f;

            animation.Play(clipName);
            while (isPlaying)
            {
                timeAtCurrentFrame = Time.realtimeSinceStartup;
                deltaTime = timeAtCurrentFrame - timeAtLastFrame;
                timeAtLastFrame = timeAtCurrentFrame;

                progressTime += deltaTime;
                curState.normalizedTime = progressTime / curState.length;
                animation.Sample();

                if (progressTime >= curState.length)
                {
                    if (curState.wrapMode != WrapMode.Loop)
                    {
                        isPlaying = false;
                    }
                    else
                    {
                        progressTime = 0f;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            yield return null;
            if (onComplete != null)
            {
                onComplete();
            }
        }
        else
        {
            animation.Play(clipName);
        }
    }

    public static IEnumerator playReverse(this Animation animation, AnimationClip clip, Action onComplete = null)
    {
        if (animation == null)
        {
            Debug.LogError("animation is null");
            yield break;
        }
        if (clip == null)
        {
            Debug.LogError("clip is null");
            yield break;
        }

        AnimationState curState = animation[clip.name];
        animation.clip = clip;

        var isPlaying = true;
        var progressTime = 0f;
        var timeAtLastFrame = Time.realtimeSinceStartup;
        var timeAtCurrentFrame = 0f;
        var deltaTime = 0f;

        while (isPlaying)
        {
            timeAtCurrentFrame = Time.realtimeSinceStartup;
            deltaTime = timeAtCurrentFrame - timeAtLastFrame;
            timeAtLastFrame = timeAtCurrentFrame;

            progressTime += deltaTime;
            animation.Play();
            curState.normalizedTime = 1 - (progressTime / curState.length);
            animation.Sample();
            animation.Stop();

            if (progressTime >= clip.length)
            {
                curState.normalizedTime = 0;
                isPlaying = false;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
        if (onComplete != null)
        {
            onComplete();
        }
    }
}
