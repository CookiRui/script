using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectLoader : ResourceLoader
{
    public GameObject loadObjectWithProxy(string assetBundle, string assetName)
    {
        var proxy = new GameObject("object");
        loadObject(assetBundle, assetName, proxy, go => {
            if (go != null)
            {
                go.transform.localPosition = Vector3.zero;
            }
            else
            {
                proxy.name += "_failed";
            }
        });
        return proxy;
    }

    public void loadObject(string assetBundle, string assetName, GameObject proxy, System.Action<GameObject> callback, bool worldPositionStays = false)
    {
        loadObject(assetBundle, assetName, _res => doInstantiate(_res, proxy, worldPositionStays), callback);
    }

    public void loadObject(string assetBundle, string assetName, System.Func<IGameObjectResource, GameObject> instantiate, System.Action<GameObject> callback)
    {
        if (instantiate == null)
        {
            instantiate = _res => doInstantiate(_res, null, false);
        }

        GameObjectResource res;
        string resName = string.Format("{0}|{1}", assetBundle, assetName);

        if (resources.TryGetValue(resName, out res))
        {
            if (res.prefab != null)
            {
                var go = instantiate.Invoke(res);
                if (go != null)
                {
                    res.instanceList.AddLast(go);
                }
                if (callback != null)
                {
                    callback.Invoke(go);
                }
            }
            else if (res.requests != null)
            {
                res.requests.Add(new GameObjectResource.Request(instantiate, callback));
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke(null);
                }
            }
        }
        else
        {
            res = new GameObjectResource(resName);
            resources.Add(resName, res);
            res.requests.Add(new GameObjectResource.Request(instantiate, callback));
            StartCoroutine(load(res, assetBundle, assetName));
        }
        onResourceRequest(res);
    }

    public void unloadObject(IGameObjectResource res)
    {
        var _res = res as GameObjectResource;
        if (_res != null && resources.Remove(res.name))
        {
            _res._prefab = null;
            _res.requests = null;
            onResourceUnload(res);
            //ResourceManager.inst.hintUnloadUnusedAssets();
        }
    }

    public void clearObjects()
    {
        foreach (var res in resources.Values)
        {
            res.requests = null;
        }
        resources.Clear();
        onResourceClear();
    }

    public IGameObjectResource[] snapshot()
    {
        var ret = new IGameObjectResource[resources.Count];
        resources.Values.forEach((res, i) => ret[i] = res);
        return ret;
    }

    public interface IGameObjectResource
    {
        string name { get; }
        bool loaded { get; }
        GameObject prefab { get; }
        int instanceCount { get; }
        object tag { get; set; }
        void update();
    }

    protected virtual GameObject doInstantiate(IGameObjectResource res, GameObject proxy, bool worldPositionStays)
    {
        Transform parent = null;
        if (!Object.ReferenceEquals(proxy, null))
        {
            if (proxy != null)
            {
                parent = proxy.transform;
            }
            else
            {
                return null;
            }
        }
        return Instantiate<GameObject>(res.prefab, parent, worldPositionStays);
    }

    protected virtual void onResourceRequest(IGameObjectResource res) { }
    protected virtual void onResourceUnload(IGameObjectResource res) { }
    protected virtual void onResourceClear() { }

    class GameObjectResource : IGameObjectResource
    { 
        public GameObjectResource(string name)
        {
            _name = name;
        }

        public string name { get { return _name; } }
        public bool loaded { get { return requests == null; } }
        public GameObject prefab { get { return _prefab; } }
        public int instanceCount { get { return instanceList.Count; } }
        public object tag { get { return _tag; } set { _tag = value; } }

        public void update()
        {
            for (var node = instanceList.First; node != null;)
            {
                if (node.Value == null)
                {
                    var t = node;
                    node = node.Next;
                    instanceList.Remove(t);
                }
                else
                {
                    node = node.Next;
                }
            }
        }

        string _name = null;
        object _tag = null;
        public GameObject _prefab = null;
        public LinkedList<GameObject> instanceList = new LinkedList<GameObject>();

        public struct Request
        {
            public System.Func<IGameObjectResource, GameObject> instantiate;
            public System.Action<GameObject> callback;
            public Request(System.Func<IGameObjectResource, GameObject> instantiate, System.Action<GameObject> callback = null) { this.instantiate = instantiate; this.callback = callback; }
        }

        public List<Request> requests = new List<Request>(1);
    }

    Dictionary<string, GameObjectResource> resources = new Dictionary<string, GameObjectResource>();

    IEnumerator load(GameObjectResource res, string assetBundle, string assetName)
    {

        do
        {
            var abreq = getAssetBundleAsync(assetBundle, null);
            yield return abreq;
            if (abreq.assetBundle == null)
            {
                break;
            }

            var areq = abreq.assetBundle.LoadAssetAsync<GameObject>(assetName);
            yield return areq;
            if (areq.asset == null)
            {
                break;
            }
            if ((res._prefab = areq.asset as GameObject) == null)
            {
                Resources.UnloadAsset(areq.asset);
                break;
            }

            if (res.requests == null)
            {
                Resources.UnloadAsset(res._prefab);
                res._prefab = null;
            }
            else
            {
                foreach (var req in res.requests)
                {
                    var go = req.instantiate(res);
                    if (go != null)
                    {
                        res.instanceList.AddLast(go);
                    }
                    if (req.callback != null)
                    {
                        req.callback.Invoke(go);
                    }
                }
                res.requests = null;
            }

            yield break;
        }
        while (false);

        if (res.requests != null)
        {
            foreach (var req in res.requests)
            {
                if (req.callback != null)
                {
                    req.callback.Invoke(null);
                }
            }
            res.requests = null;
        }
    }
}
