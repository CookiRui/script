using UnityEngine;

public interface AnimationEventHandler
{
    void OnAnimationEnd(string strParam);

    void OnAnimationStart(string strParam);

    void OnAnimation(string animation, string strParam);
}

class AnimationEvent : MonoBehaviour
{
    public AnimationEventHandler EventHandler = null;

    public void OnAnimationEnd(string strParam)
    {
        if (EventHandler != null)
            EventHandler.OnAnimationEnd(strParam);
    }

    public void OnAnimationStart(string strParam)
    {
        if (EventHandler != null)
            EventHandler.OnAnimationStart(strParam);
    }

    public void OnAnimation(string strParam)
    {
        if (EventHandler != null)
            EventHandler.OnAnimation("", strParam);

    }
};

