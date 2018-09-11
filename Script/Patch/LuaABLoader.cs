using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LuaInterface;

public class LuaABLoader : MonoBehaviour {

    int bundleCount = int.MaxValue;
    string updatePath = Application.persistentDataPath + "/";
    void Awake()
    {

    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void startLoad()
    {
        StartCoroutine(loadBundles());
    }

    public IEnumerator loadBundles()
    {
        string boundlesDir = LuaConst.osDir + "/" + LuaConst.osDir;

        string streamingPath = "";

        //检测更新目录
        string updateBoundlesPath = updatePath + "Patch/StreamingAssets/" + boundlesDir;
        if (File.Exists(updateBoundlesPath))
        {
            streamingPath = updatePath + "Patch/StreamingAssets";
        }
        else
        {
            streamingPath = Application.streamingAssetsPath.Replace('\\', '/');
        }
        //此函数的主要作用是读取到lua所有的ab包列表，因此如果更新目录下存在boundlesDir,说明修改过,或许有新增ab包,此时使用更新目录下的就可以了
        //关键的是下面读取真正的ab包的时候，如果更新目录下没有的话，需要再到基础目录下去搜索

#if UNITY_5
#if UNITY_ANDROID && !UNITY_EDITOR
        string main = streamingPath + "/" + boundlesDir;
#else
        string main = "file:///" + streamingPath + "/" + boundlesDir;
#endif
        Debug.Log("begin load www:" + main);

        WWW www = new WWW(main);
        yield return www;

        AssetBundleManifest manifest = (AssetBundleManifest)www.assetBundle.LoadAsset("AssetBundleManifest");
        List<string> list = new List<string>(manifest.GetAllAssetBundles());
#else
        //此处应该配表获取,unity5无需一个一个去配置啦
        //List<string> list = new List<string>() { "lua.unity3d", "lua_cjson.unity3d", "lua_system.unity3d", "lua_unityengine.unity3d", "lua_protobuf.unity3d", "lua_misc.unity3d", "lua_socket.unity3d", "lua_system_reflection.unity3d" };
#endif
        bundleCount = list.Count;

        for (int i = 0; i < list.Count; i++)
        {
            string str = list[i];

            string fileDir = LuaConst.osDir + "/" + str;

            string updateFileDir = updatePath + "Patch/StreamingAssets/" + fileDir;
            if (File.Exists(updateFileDir))
            {
                streamingPath = updatePath + "Patch/StreamingAssets";
            }
            else
            {
                streamingPath = Application.streamingAssetsPath.Replace('\\', '/');
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            string path = streamingPath + "/" + fileDir;
#else
            string path = "file:///" + streamingPath + "/" + fileDir;
#endif
            string name = Path.GetFileNameWithoutExtension(str);

            Debug.Log("Load lua assetbundle " + path + " name:" + name);

            StartCoroutine(coLoadBundle(name, path));
        }

        yield return StartCoroutine(LoadFinished());
    }

    IEnumerator coLoadBundle(string name, string path)
    {
        using (WWW www = new WWW(path))
        {
            if (www == null)
            {
                Debug.Log(name + " bundle not exists");
                yield break;
            }

            yield return www;

            if (www.error != null)
            {
                Debug.Log(string.Format("Read {0} failed: {1}", path, www.error));
                yield break;
            }

            --bundleCount;
            LuaLoader ll = gameObject.GetComponent<LuaLoader>();
            if (ll != null)
            {
                LuaFileUtils.Instance.AddSearchBundle(name, www.assetBundle);
            }

            www.Dispose();
        }
    }

    IEnumerator LoadFinished()
    {
        while (bundleCount > 0)
        {
            yield return null;
        }

        onBundleLoad();
    }

    void onBundleLoad()
    {
        LuaLoader ll = gameObject.GetComponent<LuaLoader>();
       if (ll != null)
       {
           ll.startLua();
       }
    }

}
