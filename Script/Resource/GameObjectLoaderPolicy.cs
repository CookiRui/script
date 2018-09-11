using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameObjectLoaderPolicy
{
    void update(GameObjectLoader loader);
    void request(GameObjectLoader.IGameObjectResource res);
    void unload(GameObjectLoader.IGameObjectResource res);
    void clear();
}

public class GameObjectLoaderPolicy_Default : IGameObjectLoaderPolicy
{
    public void update(GameObjectLoader loader)
    {
        const float TIME = 10;
        timer += Time.deltaTime;
        if (timer >= TIME)
        {
            timer = 0;
            foreach (var res in loader.snapshot())
            {
                if (res.loaded && res.tag != null)
                {
                    res.update();
                    if (res.instanceCount == 0)
                    {
                        var last = (float)res.tag;
                        if (Time.time - last >= TIME)
                        {
                            loader.unloadObject(res);
                        }
                    }
                }
            }
        }
    }

    public void request(GameObjectLoader.IGameObjectResource res)
    {
        res.tag = Time.time;
    }

    public void unload(GameObjectLoader.IGameObjectResource res) { }

    public void clear() {}

    float timer = 0;
}

public class GameObjectLoaderWithPolicy<T> : GameObjectLoader where T : IGameObjectLoaderPolicy, new()
{
    protected override void Update()
    {
        base.Update();
        policy.update(this);
    }

    protected override void onResourceRequest(IGameObjectResource res)
    {
        policy.request(res);
    }

    protected override void onResourceClear()
    {
        policy.clear();
    }

    protected override void onResourceUnload(IGameObjectResource res)
    {
        policy.unload(res);
    }

    T policy = new T();
}

public class GameObjectLoaderWithPolicy : GameObjectLoaderWithPolicy<GameObjectLoaderPolicy_Default> {}