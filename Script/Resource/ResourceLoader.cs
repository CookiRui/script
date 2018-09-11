using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceLoader : MonoBehaviour
{
    public abstract class BundleRequest : CustomYieldInstruction
    {
        public abstract AssetBundle assetBundle { get; }
    }

    protected virtual void Update()
    {

    }

    protected virtual void OnDestroy()
    {
        unLoadPreLoadResource();
    }

    /// 预加载相关代码
    /// //////////////////////////////////////////////////////////////////////////
    protected List<AssetBundle> preloadAssetBundles = new List<AssetBundle>();
    public void loadPreLoadResource()
    {
        if (preloadAssetBundles.Count != 0)
            return;

        var paths = getPreLoadAssetPath();
        if (paths.isNullOrEmpty())
            return;

        paths.forEach(a => loadPreLoadAssetBundle(getRealPath(a)));
    }

    public T getAssetFromPreloadAssetBundle<T>(int index, string assetName) where T : UnityEngine.Object
    {
        if (index < 0 || index >= preloadAssetBundles.Count)
            return null;

        AssetBundle ab = preloadAssetBundles[index];
        return ab.LoadAsset<T>(assetName);
    }

    public T getAssetFromPreloadAssetBundle<T>(string abName, string assetName) where T : UnityEngine.Object
    {
        if (abName.isNullOrEmpty()) return null;
        if (assetName.isNullOrEmpty()) return null;
        if (preloadAssetBundles.isNullOrEmpty()) return null;

        var ab = preloadAssetBundles.FirstOrDefault(a => a.name == abName);
        //Debuger.Log("getAssetFromPreloadAssetBundle abName:" + abName + " assetName:" + assetName);
        return ab ? ab.LoadAsset<T>(assetName) : null;
    }

    void unLoadPreLoadResource()
    {
        foreach (var ab in preloadAssetBundles)
        {
            ab.Unload(true);
        }

        preloadAssetBundles.Clear();
    }

    //不同的类型要自定义的预加载资源
    protected virtual IEnumerable<string> getPreLoadAssetPath() { return null; }
    /// //////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////
    //提供简单的assetbundle的加载与缓存机制

    protected Dictionary<string, AssetBundleInfo> assetBundles = new Dictionary<string, AssetBundleInfo>();

    public BundleRequest getAssetBundleAsync(string relativePath, Action<AssetBundle> callback)
    {
        AssetBundleInfo info = null;
        string name = relativePath.ToLower();
        if (assetBundles.TryGetValue(name, out info))
        {
            if (callback != null)
            {
                if (info.loading != null)
                {
                    info.loading.callback += callback;
                }
                else
                {
                    callback(info.assetBundle);
                }
            }
        }
        else
        {
            info = createAssetBundleInfo(name);
            if (callback != null)
            {
                info.loading.callback += callback;
            }
            assetBundles.Add(name, info);
            StartCoroutine(loadAssetBundle(relativePath, info));
        }
        return info;
    }

    public BundleRequest findAssetBundle(string relativePath)
    {
        AssetBundleInfo info = null;
        string name = relativePath.ToLower();
        if (assetBundles.TryGetValue(name, out info))
        {
            return info;
        }
        return null;
    }

    IEnumerator loadAssetBundle(string relativePath, AssetBundleInfo info)
    {
        //Debug.Log("*********load ab resource " + relativePath);
        var loading = info.loading;
        yield return loading.request = AssetBundle.LoadFromFileAsync(getRealPath(relativePath));
        if (info.loading == null)
        {
            if (loading.request.assetBundle != null)
            {
                loading.request.assetBundle.Unload(true);
            }
            loading.finish(null);
            yield break;
        }

        if (loading.request.assetBundle == null)
        {
            Debug.LogError("无效的assetbundle " + relativePath);
            assetBundles.Remove(info.name);
            info.loading.finish(null);
            yield break;
        }

        //Debug.Log("======load ab res succeed " + relativePath);
        info._assetBundle = loading.request.assetBundle;
        info.loading = null;
        loading.finish(info.assetBundle);
    }

    public void removeAssetBundle(string relativePath)
    {
        AssetBundleInfo info;

        if (assetBundles.TryGetValue(relativePath.ToLower(), out info))
        {
            removeAssetBundle(info);
        }
    }

    public void removeAssetBundles()
    {
        foreach (var info in assetBundles.Values)
        {
            if (info.loading != null)
            {
                info.loading = null;
            }
            else
            {
                Debug.Assert(info.assetBundle != null, "这不可能！");
                info.assetBundle.Unload(true);
            }
        }
        assetBundles.Clear();
    }

    protected void removeAssetBundle(AssetBundleInfo info)
    {
        if (info.loading != null)
        {
            info.loading = null;
        }
        else
        {
            Debug.Assert(info.assetBundle != null, "这不可能！");
            info.assetBundle.Unload(true);
        }
        assetBundles.Remove(info.name);
    }

    /// //////////////////////////////////////////////////////////////////////////
    void loadPreLoadAssetBundle(string path)
    {
        AssetBundle ab = AssetBundle.LoadFromFile(path);
        if (ab)
        {
            preloadAssetBundles.Add(ab);
        }
    }

    //根据不同平台，不同位置，把相对的路径转成可以读取到的路径
    public static string getRealPath(string relativePath)
    {
        relativePath = relativePath.ToLower();
        var path = getFolder() + "/" + relativePath;
        return Application.streamingAssetsPath + "/" + path;
    }

    static string getFolder()
    {
#if UNITY_STANDALONE_OSX
        return "mac";
#elif UNITY_EDITOR || UNITY_STANDALONE
        return "win";
#elif UNITY_ANDROID
    return "android";
#elif UNITY_IOS
    return "ios";
#else
    Debug.LogError("不支持该平台！");
    return null;
#endif
    }

    protected virtual AssetBundleInfo createAssetBundleInfo(string name)
    {
        return new AssetBundleInfo(name);
    }


    protected class AssetBundleInfo : BundleRequest
    {
        public AssetBundleInfo(string name)
        {
            this.name = name;
            loading = new AssetBundleLoading();
        }

        public string name;
        public AssetBundle _assetBundle;
        public AssetBundleLoading loading;

        public override AssetBundle assetBundle
        {
            get
            {
                return _assetBundle;
            }
        }

        public override bool keepWaiting
        {
            get
            {
                return loading != null;
            }
        }
    }

    protected class AssetBundleLoading
    {
        public AssetBundleCreateRequest request;
        public event System.Action<AssetBundle> callback;

        public void finish(AssetBundle ab)
        {
            if (callback != null)
            {
                callback(ab);
            }
        }
    }
}
