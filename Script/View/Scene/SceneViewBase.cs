using UnityEngine;
using System.Collections.Generic;
using FixMath.NET;
using Cratos;

abstract class SceneViewBase : MonoBehaviour
{
    public virtual void onCreated()
    {

    }

    public virtual void onDestroyed()
    {
        if (!gameObject) return;
        Destroy(gameObject);
    }

    public virtual void onEnter()
    {

    }

    public virtual void onExit()
    {
    }
}

class SceneViews : Singleton<SceneViews>
{
    public override void onInit()
    {
        base.onInit();

        LogicEvent.add("onFBGameCreated", this, "onFBGameCreated");
        LogicEvent.add("onFBGameDestroyed", this, "onFBGameDestroyed");
        LogicEvent.add("onFBReplayCreated", this, "onFBReplayCreated");
        LogicEvent.add("onFBReplayDestroyed", this, "onFBReplayDestroyed");

    }
    public override void onUninit()
    {
        base.onUninit();

        LogicEvent.remove(this);
    }


    void onFBGameCreated()
    {
        setNextScene<FBSceneView>();
    }
    void onFBGameDestroyed()
    {
        removeScene<FBSceneView>();
    }
    void onFBReplayCreated()
    {
        setNextScene<FBReplayView>();
    }
    void onFBReplayDestroyed()
    {
        removeScene<FBReplayView>();
    }

    List<SceneViewBase> scenes = new List<SceneViewBase>();

    SceneViewBase currentScene = null;

    public SceneViewBase current
    {
        get { return currentScene; }
    }

    public FBSceneViewBase getCurFBScene()
    {
        if (currentScene == null) return null;
        if (!(currentScene is FBSceneViewBase)) return null;

        return currentScene as FBSceneViewBase;
    }

    T setNextScene<T>()
        where T : SceneViewBase, new()
    {
        T scene = getScene<T>();
        if (scene == null)
        {
            scene = createScene<T>();
        }
        else if (scene == currentScene)
        {
            return scene;
        }

        if (currentScene != null)
            currentScene.onExit();
        currentScene = scene;
        currentScene.onEnter();

        return scene;
    }

    T getScene<T>()
        where T : SceneViewBase
    {
        for (int i = 0; i < scenes.Count; ++i)
        {
            if (scenes[i].GetType() == typeof(T))
                return scenes[i] as T;
        }

        return null;
    }

    T createScene<T>()
        where T : SceneViewBase
    {
        if (getScene<T>() != null)
            return null;
        GameObject ga = new GameObject(typeof(T).ToString());
        T sc = ga.AddComponent<T>();
        scenes.Add(sc);
        sc.onCreated();
        return sc;
    }

    void removeScene<T>()
        where T : SceneViewBase
    {
        T sc = getScene<T>();
        if (sc == null)
            return;
        sc.onDestroyed();
        scenes.Remove(sc);
        if (currentScene == sc)
            currentScene = null;
    }
}