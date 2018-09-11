using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EffectResourceLoader : GameObjectLoaderWithPolicy
{
    public static EffectResourceLoader inst;

    void Awake()
    {
        inst = this;
    }

    protected override IEnumerable<string> getPreLoadAssetPath()
    {
        var path = getRealPath("effect/common/common.ab");
        //Debug.Log("**************getPreLoadAssetPath "+path);
        var ab = AssetBundle.LoadFromFile(path);
        return null;
    }

    public GameObject playEffect(string effectName, Transform parent)
    {
        return playEffect(effectName, parent, Vector3.zero, Vector3.zero, Vector3.one);
    }

    public GameObject playEffect(string effectName, Transform parent, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
    {
        if (string.IsNullOrEmpty(effectName))
            return null;

        GameObject go = null;
#if SCENEEDITOR_TOOL && UNITY_EDITOR
        string basePath = "Assets/Art/Effect/Prefabs/";
        var dir = Application.dataPath.Replace("Assets", "") + basePath;
        var files = Directory.GetFiles(dir, "*" +effectName+".prefab",SearchOption.AllDirectories);
        if (files == null || files.Length == 0)
        {
            Debug.LogError("playEffect gameobject " + effectName + "is null");
            return null;
        }

        string effectPath = files[0].Replace(Application.dataPath, "Assets");
        GameObject res = AssetDatabase.LoadAssetAtPath<GameObject>(effectPath);
        if (res == null)
        {
            Debug.LogError("playEffect gameobject " + effectName + "is null");
            return null;
        }
        go = GameObject.Instantiate(res);
#else
        string effectABName = Path.GetFileNameWithoutExtension(effectName);
        go = new GameObject(effectName);
        loadObject(getAssetBundlePath(effectABName), effectABName, go, effectGO =>
        {

            if (effectGO == null) return;
            if (go == null)
            {
                Destroy(effectGO);
                return;
            }

            var delayDestroy = effectGO.GetComponent<DelayDestroy>();
            if (delayDestroy == null) return;

            go.AddComponent<DelayDestroy>().destroyTime = delayDestroy.destroyTime;
            Destroy(delayDestroy);
        });
#endif

        if (parent != null)
        {
            go.transform.SetParent(parent);
        }

        go.transform.localPosition = localPosition;
        go.transform.localEulerAngles = localEulerAngles;
        go.transform.localScale = localScale;
        return go;
    }

    private string getAssetBundlePath(string effectName)
    {
        return "Effect/" + effectName + ".ab";
    }

    public void unLoadEffect(GameObject o)
    {
        GameObject.Destroy(o);
    }
}
