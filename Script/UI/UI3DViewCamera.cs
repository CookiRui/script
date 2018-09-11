using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI3DViewCamera : MonoBehaviour {

    public static UI3DViewCamera instance { get; private set; }

    //渲染3D场景的相机
    public Camera ui3DCamera;
    //rendertexture camea
    public Camera renderTextureCamera;

    //Camera Transform
    private Transform ui3DCameraTrans;
    private Transform renderTextureCameraTrans;

    private GameObject rootObject;

    private Transform rootTrans;

    private Coroutine loadCoroutine;

    private GameObject loadObject;

    private GameObject loadScene;

    private RenderTexture renderTexture;

    void Awake()
    {
        instance = this;
        rootObject = gameObject;
        rootTrans = transform;
        ui3DCameraTrans = ui3DCamera.transform;
        renderTextureCameraTrans = renderTextureCamera.transform;
        setActive(false);
    }


    #region UI相机控制
    //UI相机控制 
    public void setActive(bool active)
    {
        if (!active) {
            stopLoadCoroutine();
            if (loadObject)
            {
                DestroyImmediate(loadObject);
                loadObject = null;
            }

            if (loadScene)
            {
                DestroyImmediate(loadScene);
                loadScene = null;
            }
        }
        if(active!=rootObject.activeSelf) rootObject.SetActive(active);
    }


    public void setUI3dCamraPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        if (ui3DCameraTrans != null)
        {
            ui3DCameraTrans.SetPositionAndRotation(position,rotation);
        }
    }

    public void setRenderCamraPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        if (renderTextureCameraTrans != null)
        {
            renderTextureCameraTrans.SetPositionAndRotation(position, rotation);
        }
    }

    //设置3dUI层级
    private void setUI3DLayer(GameObject uiObj, string layername, bool useSelectTypes)
    {
        if (uiObj != null)
        {
            int layer = LayerMask.NameToLayer(layername);
            if (useSelectTypes)
            {
                //for (int index = 0, length = trans.childCount;index < length ; index++)
                //{
                //    trans.GetChild(index).gameObject.layer = layer;
                //}
                SkinnedMeshRenderer[] skinMeshs = uiObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer s in skinMeshs)
                {
                    s.gameObject.layer = layer;
                }
            }
            uiObj.layer = layer;
            if (uiObj.GetComponent<Collider>() == null)
            {
                BoxCollider box = uiObj.AddComponent<BoxCollider>();
                box.center = Vector3.up;
                box.size = Vector3.one * 2;
            }
        }
    }
    #endregion

    #region 加载接口
    public void loadModel(string loadpath, LuaFunction func)
    {
        ModelResourceLoader.inst.loadModel(loadpath, delegate(GameObject obj)
        {
            obj.transform.parent = rootTrans;
            loadScene = obj;
            func.Call(obj);
        });
    }


    /// <summary>
    /// 加载模型 回调函数
    /// </summary>

    public void loadModelAvatar(string avatorName, string meshIndex, string meshname, string layername, bool useSelectTypes, LuaFunction func)
    {
        loadCoroutine = StartCoroutine(waitLoadModelAvatar(avatorName, meshIndex, meshname, layername, useSelectTypes, func));
    }

    private void stopLoadCoroutine()
    {
        if (loadCoroutine != null)
        {
            StopCoroutine(loadCoroutine);
            loadCoroutine = null;
        }
    }

    IEnumerator waitLoadModelAvatar(string avatorName, string meshIndex, string meshname, string layername, bool useSelectTypes, LuaFunction func)
    {
        if (avatorName.isNullOrEmpty())
        {
            Debuger.LogError("load model avatar name is null");
            yield break;
        }

        System.Collections.Generic.Dictionary<string, string> avatar = new System.Collections.Generic.Dictionary<string, string>();
        avatar[meshIndex] = meshname;
        loadObject = ModelResourceLoader.inst.createAvatar(avatorName, avatar, null);
        if (loadObject == null) yield break;
        loadObject.transform.parent = rootTrans;
        //yield return new WaitForSeconds(0.7f);
        while (loadObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length==0) yield return null;
        setUI3DLayer(loadObject, layername, useSelectTypes);
        if (func != null) func.Call(loadObject);
    }

    #endregion

    #region rendertexture设置
    public void unloadRenderTexture()
    {
        if (renderTexture)
        {
            Resources.UnloadAsset(renderTexture);
            renderTexture = null;
        }
    }

    public void setRenderTexture(RawImage image)
    {
        if (image != null)
        {
            if (renderTextureCamera.targetTexture == null)
            {
                renderTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
                renderTextureCamera.targetTexture = renderTexture;
            }
            else renderTexture = renderTextureCamera.targetTexture;
            image.texture = renderTexture;
        }
    }
    #endregion

}
