using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cratos;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager inst;
    UIResourceLoader uiResources;
    EffectResourceLoader effectResources;
    ConfigResourceLoader configResources;
    DataTableResourceLoader dataTableResources;
    ModelResourceLoader modelResources;
    SceneResourceLoader sceneResources;

    long totalAllocatedMemory = 0;

    public string BasePathURL
    {
        get
        {
            return
    #if UNITY_ANDROID  && !UNITY_EDITOR
                Application.dataPath + "!assets/";
    #elif UNITY_IPHONE && !UNITY_EDITOR
                Application.dataPath + "/Raw/";
    #elif UNITY_STANDALONE_WIN || UNITY_EDITOR
     Application.dataPath + "/StreamingAssets/";
    #else  
                string.Empty;  
    #endif
        }
    }

    public string WriteablePath
    {
        get
        {
            return Application.persistentDataPath + "/";
        }
    }

    public enum PathType
    {
        PT_Writable,             //可写目录
        PT_Resources,           //Resources目录
        PT_StreamingAssets,
    };

    void Awake()
    {
        inst = this;
        addResourceLoaders();
        initPreLoadResource();
    }

    IEnumerator Start()
    {
        StartCoroutine(unloadUnusedAssetsRoutine());

        for (;;)
        {
            totalAllocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            yield return new WaitForSeconds(1);
        }
    }

    //打开预加载且全局不卸载的AssetBundle
    //如技能图标，物品图标，场景通用，角色通用等等
    void initPreLoadResource()
    {
        uiResources.loadPreLoadResource();
        effectResources.loadPreLoadResource();
        configResources.loadPreLoadResource();
        dataTableResources.loadPreLoadResource();
        modelResources.loadPreLoadResource();
        sceneResources.loadPreLoadResource();
    }

    void addResourceLoaders()
    {
        uiResources = gameObject.AddComponent<UIResourceLoader>();
        effectResources = gameObject.AddComponent<EffectResourceLoader>();
        configResources = gameObject.AddComponent<ConfigResourceLoader>();
        dataTableResources = gameObject.AddComponent<DataTableResourceLoader>();
        modelResources = gameObject.AddComponent<ModelResourceLoader>();
        sceneResources = gameObject.AddComponent<SceneResourceLoader>();
    }

    IEnumerator unloadUnusedAssetsRoutine()
    {
        const float TIME = 10;
        const uint MEMORY_SIZE = 200 * 1024 * 1024;
        for (;;)
        {
            yield return new WaitForSeconds(TIME);
            if (totalAllocatedMemory >= MEMORY_SIZE)
            {
                Resources.UnloadUnusedAssets();
            }
        }
    }

    public string getPathByType(PathType pathType)
    {
        string tempPath = "";
        if (pathType == PathType.PT_Writable)
        {
            tempPath = WriteablePath;
        }
        else if (pathType == PathType.PT_Resources)
        {
            tempPath = Application.dataPath + "/Resources/";
        }
        else if (pathType == PathType.PT_StreamingAssets)
        {
            tempPath = Application.dataPath + "/StreamingAssets/";
        }

        return tempPath;
    }

    public void saveToFile(BytesStream stream, string filename, PathType pathType = PathType.PT_Writable)
    {
        string tempPath = getPathByType(pathType) + filename;

        if (Directory.Exists(Path.GetDirectoryName(tempPath)))
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
        }

        //临时写文件
        FileStream fs = new FileStream(tempPath, FileMode.CreateNew);
        fs.Write(stream.Buf, 0, stream.Used);
        fs.Close();
    }    
}
