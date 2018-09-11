using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Cratos;

public class SceneResourceLoader : ResourceLoader
{
    public static SceneResourceLoader inst;

    private string curSceneName;

    void Awake()
    {
        inst = this;
    }

    protected override IEnumerable<string> getPreLoadAssetPath()
    {
        //Debug.Log("******SceneResourceLoader");
        return null;
    }

    public void unLoadSceneMap()
    {
        if (curSceneName.isNullOrEmpty())
            return;

        Scene scene = SceneManager.GetSceneByName(curSceneName);
        if (!scene.IsValid())
            return;

        StartCoroutine(unLoadSceneMapImp(scene));
    }

    IEnumerator unLoadSceneMapImp(Scene scene)
    {
        var asyn = SceneManager.UnloadSceneAsync(scene);
        while(!asyn.isDone)
        {
            yield return null;
        }
        removeAssetBundles();
    }

    public void loadScene(string sceneName, string weatherName)
    {
#if SCENEEDITOR_TOOL
        StartCoroutine(asynLoadScene(weatherName));
#else
        StartCoroutine(loadSceneImp(sceneName, weatherName));
#endif
    }

    IEnumerator loadSceneImp(string sceneName, string weatherName)
    {
        string basePath = "map/" + sceneName;
        string commonPath = basePath + "/common.ab";
        var commonAb = getAssetBundleAsync(commonPath, null);
        yield return commonAb.assetBundle;
        if (isShareLightMap(weatherName))
        {
            string shareName = getShareLightMapAb(weatherName);
            string sharePath = basePath + "/" + shareName + ".ab";
            getAssetBundleAsync(sharePath, (ab) =>
            {
                loadWeather(sceneName, weatherName);
            });
        }
        else
        {
            loadWeather(sceneName, weatherName);
        }
    }

    void loadWeather(string sceneName, string weatherName)
    {
        string basePath = "map/" + sceneName;
        string weatherPath = basePath + "/" + weatherName + ".ab";
        getAssetBundleAsync(weatherPath, (weatherAB) =>
        {
            //Debug.Log("load scene ab res succeed");
            string name = getWeatherPath(weatherAB);
            curSceneName = Path.GetFileNameWithoutExtension(name);
            StartCoroutine(asynLoadScene(curSceneName));
        });
    }

    IEnumerator asynLoadScene(string name)
    {
        //Debug.Log("asynLoadScene:" +name);
        var asyn = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        yield return asyn;

        //Debug.Log("LoadSceneAsync done" + name);

        Scene scene = SceneManager.GetSceneByName(name);
        if (scene.IsValid())
        {
            SceneManager.SetActiveScene(scene);
        }

        //Debug.Log("asynLoadScene is done");
        LogicEvent.fire2Rendering("onSceneLoaded");
    }

    string getWeatherPath(AssetBundle ab)
    {
        string[] paths = ab.GetAllScenePaths();
        return paths.Length > 0 ? paths[0] : "";
    }

    bool isShareLightMap(string weatherName)
    {
        string[] str = weatherName.Split('_');
        if(str.Length == 3)
        {
            return true;
        }

        return false;
    }

    string getShareLightMapAb(string weatherName)
    {
        string[] str = weatherName.Split('_');
        return str[0] + "_" + str[1];
    }
}
