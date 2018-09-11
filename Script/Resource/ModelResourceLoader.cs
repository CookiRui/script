using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

class ModelResourceLoader : GameObjectLoaderWithPolicy
{
    public static ModelResourceLoader inst;

    private static readonly string rootBone = "Bip001";
    private static readonly string bindObjMark = "bindObjMark";
    private static readonly string skeletonName = "skeleton";

    void Awake()
    {
        inst = this;
    }

    protected override IEnumerable<string> getPreLoadAssetPath()
    {
        var path = getRealPath("model/common/common.ab");
        var ab = AssetBundle.LoadFromFile(path);
        if (ab == null)
        {
            Debuger.LogWarning("model common ab load failed");
            return null;
        }

        Debug.Log("==========ModelResourceLoader");

        return null;
    }

    public GameObject createAvatar(string avatarName, Dictionary<string, string> avatarInfo, Action<GameObject> callBack)
    {
        return loadAvatar(avatarName, avatarInfo, null, callBack);
    }

    public GameObject changeAvatar(string avatarName, Dictionary<string, string> avatarInfo, GameObject avatarObj, Action<GameObject> callBack)
    {
        return loadAvatar(avatarName, avatarInfo, avatarObj, callBack);
    }

    private GameObject loadAvatar(string avatarName, Dictionary<string, string> avatarInfo, GameObject avatarObj, Action<GameObject> callBack)
    {
#if SCENEEDITOR_TOOL && UNITY_EDITOR
        GameObject skeleton = null;
        GameObject modelObj = null;
        string basePath = "Assets/Art/Model/Avatar/" + avatarName + "/";
        if (avatarObj == null)
        {
            //初始化加载
            modelObj = new GameObject(avatarName);
            string skeletonPath = basePath + "skeleton.prefab";
            GameObject skeletonRes = AssetDatabase.LoadAssetAtPath<GameObject>(skeletonPath);
            skeleton = GameObject.Instantiate(skeletonRes);
            skeleton.name = skeletonName;
            skeleton.transform.SetParent(modelObj.transform, false);
        }
        else
        {
            //换装
            modelObj = avatarObj;
            skeleton = modelObj.transform.Find(skeletonName).gameObject;
        }

        foreach (var dict in avatarInfo)
        {
            if (isPendant(dict.Key))
            {
                //附属挂点
                string path = basePath + "pendant/" + dict.Value + ".prefab";
                destroyAvatarPendant(skeleton, dict.Key);
                GameObject res = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (res != null)
                {
                    GameObject o = GameObject.Instantiate(res);
                    if (o != null)
                    {
                        combineAvatarPendant(skeleton, dict.Key, o);
                    }
                }
            }
            else
            {
                //Avatar
                string path = basePath + "mesh/" + dict.Key + "/" + dict.Value + ".prefab";
                destroyAvatarPart(skeleton, dict.Key);
                GameObject res = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (res != null)
                {
                    GameObject o = GameObject.Instantiate(res);
                    if (o != null)
                    {
                        combineAvatarPart(skeleton, o.GetComponentInChildren<SkinnedMeshRenderer>(), dict.Key);
                        GameObject.Destroy(o);
                    }
                }
            }
        }

        if(callBack != null)
        {
            callBack(modelObj);
        }
        return modelObj;
#else
        if (avatarObj == null)
        {
            GameObject modelObj = new GameObject(avatarName);
            StartCoroutine(loadAvatarImp(avatarName, avatarInfo, null, (go) =>
            {
                go.transform.SetParent(modelObj.transform, false);
                if(callBack != null)
                {
                    callBack(modelObj);
                }
            }));

            return modelObj;
        }
        else
        {
            GameObject skeleton = avatarObj.transform.Find(skeletonName).gameObject; ;
            StartCoroutine(loadAvatarImp(avatarName, avatarInfo, skeleton, (go)=>
            {
                if(callBack != null)
                {
                    callBack(avatarObj);
                }
            }));
            return avatarObj;
        }
#endif
    }

    private IEnumerator loadAvatarImp(string avatarName, Dictionary<string, string> avatarInfo, GameObject avatarObj, Action<GameObject> callback)
    {
        string relativePath = "model/avatar/" + avatarName + ".ab";
        if (avatarObj == null)
        {
            var ab = getAssetBundleAsync(relativePath, null);
            yield return ab.assetBundle;
            loadSkeleton(relativePath, skeletonName, (skeleton) =>
            {
                handleAvatar(relativePath, avatarInfo, skeleton, callback);
            });
        }
        else
        {
            GameObject skeleton = avatarObj;
            handleAvatar(relativePath, avatarInfo, skeleton, callback);
        }
    }

    void handleAvatar(string assetBundlePath, Dictionary<string, string> avatarInfo, GameObject skeleton, Action<GameObject> callBack)
    {
        int total = getTotalAvatarPart(avatarInfo);
        int index = 0;
        foreach (var dict in avatarInfo)
        {
            if (isPendant(dict.Key))
            {
                destroyAvatarPendant(skeleton, dict.Key);
                if (!string.IsNullOrEmpty(dict.Value))
                {
                    loadObject(assetBundlePath, dict.Value, null, (go) =>
                    {
                        combineAvatarPendant(skeleton, dict.Key, go);
                        index++;
                        if(index == total)
                        {
                            callBack(skeleton);
                        }
                    });
                    //redirectAvatarPendant(skeleton, assetBundlePath, dict.Value, dict.Key);
                }
            }
            else
            {
                destroyAvatarPart(skeleton, dict.Key);
                if (!string.IsNullOrEmpty(dict.Value))
                {
                    loadAvatarPart(assetBundlePath, dict.Value, (go) =>
                    {
                        combineAvatarPart(skeleton, go.GetComponentInChildren<SkinnedMeshRenderer>(), dict.Key);
                        index++;
                        if(index == total)
                        {
                            callBack(skeleton);
                        }
                    });
                    //redirectAvatarPart(skeleton, assetBundlePath, dict.Value, dict.Key);
                }
            }
        }
    }

    int getTotalAvatarPart(Dictionary<string, string> avatarInfo)
    {
        int total = 0;
        foreach(var dict in avatarInfo)
        {
            if (!dict.Value.isNullOrEmpty())
                total++;
        }

        return total;
    }

    void redirectAvatarPart(GameObject skeleton, string assetBundle, string assetName, string partTag)
    {
        loadAvatarPart(assetBundle, assetName, (go) =>
        {
            combineAvatarPart(skeleton, go.GetComponentInChildren<SkinnedMeshRenderer>(), partTag);
        });
    }

    void redirectAvatarPendant(GameObject skeleton, string assetBundle, string assetName, string bindPointName)
    {
        loadObject(assetBundle, assetName, null, (go) =>
        {
            combineAvatarPendant(skeleton, bindPointName, go);
        });
    }

    GameObject getChildGameObj(GameObject o)
    {
        if (o == null)
            return null;

        SkinnedMeshRenderer smr = o.transform.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr == null)
            return null;

        return smr.gameObject;
    }

    GameObject specialInstantiate(IGameObjectResource res)
    {
        GameObject childObj = getChildGameObj(res.prefab);
        return Instantiate<GameObject>(childObj, Vector3.zero, new Quaternion());
    }

    void loadAvatarPart(string assetBundle, string assetName, Action<GameObject> callBack)
    {
        loadObject(assetBundle, assetName, specialInstantiate, (go) =>
        {
            callBack(go);
        });
    }

    void loadSkeleton(string assetBundle, string assetName, Action<GameObject> callBack)
    {
        loadObject(assetBundle, assetName, null, (go) =>
        {
            go.transform.position = Vector3.zero;
            go.transform.rotation = new Quaternion();
            go.name = skeletonName;
            callBack(go);
        });
    }

    void destroyAvatarPart(GameObject skeleton, string partTag)
    {
        for (int i = 0; i < skeleton.transform.childCount; i++)
        {
            Transform t = skeleton.transform.GetChild(i);
            if (t.name.Equals(partTag))
            {
                GameObject.Destroy(t.gameObject);
                break;
            }
        }
    }

    void combineAvatarPart(GameObject skeleton, SkinnedMeshRenderer smr, string partTag)
    {
        List<Transform> boneInSkeleton = new List<Transform>();
        List<string> nameList = getAvatarPartBone(smr);
        foreach (var name in nameList)
        {
            foreach (var bone in skeleton.transform.GetComponentsInChildren<Transform>(true))
            {
                if (name.Equals(bone.name))
                {
                    boneInSkeleton.Add(bone);
                    break;
                }
            }
        }

        smr.transform.SetParent(skeleton.transform, false);
        smr.bones = boneInSkeleton.ToArray();
        smr.rootBone = skeleton.transform.Find(rootBone);
        smr.name = partTag;
    }

    //挂载Avatar附属挂件
    void combineAvatarPendant(GameObject skeleton, string bindPointName, GameObject pendantObj)
    {
        Transform parent = getBindPoint(skeleton, bindPointName);
        if (parent == null)
            return;

        GameObject objMark = new GameObject(bindObjMark);
        pendantObj.transform.SetParent(objMark.transform, false);
        objMark.transform.SetParent(parent, false);
    }

    void destroyAvatarPendant(GameObject skeleton, string bindPointName)
    {
        Transform bindTran = getBindPoint(skeleton, bindPointName);
        for (int i = 0; i < bindTran.childCount; i++)
        {
            Transform t = bindTran.GetChild(i);
            if (t.name.Equals(bindObjMark))
            {
                t.parent = null;
                GameObject.Destroy(t.gameObject);
            }
        }
    }

    bool isPendant(string name)
    {
        foreach (var point in BindPoint.BindPoints)
        {
            if (name.Equals(point))
                return true;
        }

        return false;
    }

    Transform getBindPoint(GameObject skeleton, string bindName)
    {
        foreach (var bind in skeleton.transform.GetComponentsInChildren<Transform>(true))
        {
            if (bindName.Equals(bind.name))
            {
                return bind;
            }
        }

        return null;
    }

    List<string> getAvatarPartBone(SkinnedMeshRenderer smr)
    {
        List<string> nameList = new List<string>();
        foreach (var bone in smr.bones)
        {
            nameList.Add(bone.name);
        }

        return nameList;
    }

    public void loadAnimatorControllers(string avatarName, Action<List<RuntimeAnimatorController>> callBack)
    {
#if SCENEEDITOR_TOOL && UNITY_EDITOR
        string aniPath = "Assets/Art/Model/Avatar/" + avatarName + "/animation";
        string[] ctrlFiles = Directory.GetFiles(aniPath, "*.controller");
        List<RuntimeAnimatorController> ctrlList = new List<RuntimeAnimatorController>();
        for(int i = 0; i < ctrlFiles.Length; i++)
        {
            string fileName = aniPath + "/" + Path.GetFileName(ctrlFiles[i]);
            RuntimeAnimatorController c = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(fileName);
            if(c != null)
            {
                ctrlList.Add(c);
            }
        }

        callBack(ctrlList);
#else
        string relativePath = "model/avatar/" + avatarName + ".ab";
        getAssetBundleAsync(relativePath, (ab) =>
        {
            string[] assets = ab.GetAllAssetNames();
            List<string> assetList = new List<string>(assets);
            assetList.RemoveAll((str) =>
            {
                if (!Path.GetExtension(str).Equals(".controller"))
                    return true;

                return false;
            });

            List<RuntimeAnimatorController> aniCtrlList = new List<RuntimeAnimatorController>();
            foreach (var l in assetList)
            {
                RuntimeAnimatorController ctrl = ab.LoadAsset<RuntimeAnimatorController>(l);
                if (ctrl != null)
                {
                    aniCtrlList.Add(ctrl);
                }
            }

            callBack(aniCtrlList);
        });
#endif
    }

    public void loadAnimation(string avatarName, string aniName, Action<AnimationClip> callBack)
    {
#if SCENEEDITOR_TOOL && UNITY_EDITOR
        string aniPath = "Assets/Art/Model/Avatar/" + avatarName + "/animation/"+aniName+".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(aniPath);
        callBack(clip);
#else
        string relativePath = "model/avatar/" + avatarName + ".ab";
        getAssetBundleAsync(relativePath, (ab) =>
        {
            string fileName = "";
            string[] assets = ab.GetAllAssetNames();
            for (int i = 0; i < assets.Length; i++)
            {
                if (Path.GetFileNameWithoutExtension(assets[i]).ToLower().Equals(aniName.ToLower()))
                {
                    fileName = assets[i];
                    break;
                }
            }

            AnimationClip clip = null;
            if (!fileName.isNullOrEmpty())
            {
                clip = ab.LoadAsset<AnimationClip>(fileName);
            }
            callBack(clip);
        });
#endif
    }

    ///////////////////非avatar模型加载//////////////////////////////
    public GameObject loadModel(string modelName, Action<GameObject> callback = null)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;

        GameObject go = new GameObject(modelName);
#if SCENEEDITOR_TOOL && UNITY_EDITOR
        string basePath = "Assets/Art/Model/Model/";
        string effectPath = basePath + modelName + ".prefab";
        GameObject res = AssetDatabase.LoadAssetAtPath<GameObject>(effectPath);
        if (res == null)
        {
            Debug.LogError("playEffect gameobject " + modelName + "is null");
            return null;
        }
        GameObject o = GameObject.Instantiate(res);
        o.transform.SetParent(go.transform, false);
#else
        string modelABName = Path.GetFileNameWithoutExtension(modelName);
        loadObject(getAssetBundlePath(modelABName), modelABName, go, callback);
#endif
        return go;
    }

    private string getAssetBundlePath(string modelName)
    {
        return "Model/Model/" + modelName + ".ab";
    }
}
