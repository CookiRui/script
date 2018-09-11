using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class UI : MonoBehaviour
{
    public static UI instance { get; private set; }

    int UIINSTANCEMAX = 20;
    Dictionary<string, GameObject> uiInstanceDic = new Dictionary<string, GameObject>();


    Transform defaultParent;
    Transform tipParent;
    Transform topParent;

    void Awake()
    {
        instance = this;
    }

    public string showUI(string name)
    {
        return showUI(name, false);
    }
    public string showUI(string name, bool force = false)
    {
        if (name.isNullOrEmpty())
        {
            Debuger.LogError("showUI error, the name is null");
            return "";
        }

        GameObject go;
        string newName = name;
        if (uiInstanceDic.TryGetValue(name, out go))
        {
            //已经创建了该面板
            if (force)
            {
                //强制创建新面板(例如messagebox同时存在多个对象实例)
                for (int i = 0; i <= UIINSTANCEMAX; ++i)
                {
                    newName = name + "_" + i;
                    if (!uiInstanceDic.ContainsKey(newName))
                    {
                        break;
                    }

                    if (i == UIINSTANCEMAX)
                    {
                        Debuger.LogError("showUI failed, the number of ui instance is to large");
                        return "";
                    }
                }
            }
            else
            {
                //无需创建新面板
                go.SetActive(true);
                go.transform.SetAsLastSibling();
                return name;
            }
        }


        //创建
        createUI(newName, (newGo) =>
        {
            if (newGo != null)
            {
                if (uiInstanceDic.ContainsKey(newName)) return;

                uiInstanceDic.Add(newName, newGo);
            }
            else
            {
                Debuger.LogError("createUI failed");
            }
        });

        return newName;
    }


    public void setActive(string name, bool active)
    {
        if (name.isNullOrEmpty())
        {
            Debuger.LogError("showUI error, the name is null");
            return;
        }

        GameObject go;
        if (uiInstanceDic.TryGetValue(name, out go))
        {
            //无需创建新面板
            if (active != go.activeSelf)
            {
                go.SetActive(active);
            }
            if (active) go.transform.SetAsLastSibling();
            return;
        }

        createUI(name, (newGo) =>
        {
            if (newGo != null)
            {
                uiInstanceDic.Add(name, newGo);
                if (active != newGo.activeSelf)
                {
                    newGo.SetActive(active);
                }
            }
            else
            {
                Debuger.LogError("createUI failed");
            }
        });
    }

    public void setInverseActive(string name)
    {

        if (name.isNullOrEmpty())
        {
            Debuger.LogError("showUI error, the name is null");
            return;
        }
        GameObject go;
        if (uiInstanceDic.TryGetValue(name, out go))
        {
            //无需创建新面板
            go.SetActive(!go.activeSelf);
            if (go.activeSelf) go.transform.SetAsLastSibling();
            return;
        }
        createUI(name, (newGo) =>
        {
            if (newGo != null)
            {
                uiInstanceDic.Add(name, newGo);
                newGo.SetActive(true);
            }
            else
            {
                Debuger.LogError("createUI failed");
            }
        });
    }

    void createUI(string name, Action<GameObject> callBack)
    {
        if (name.isNullOrEmpty())
        {
            Debuger.LogError("name is null");
            return;
        }

        UIResourceLoader.inst.loadUI(name, (res) =>
        {
            if (res == null)
            {
                Debug.LogError("加载UI出错 " + name);
                return;
            }

            GameObject go = GameObject.Instantiate(res);
            go.name = name;
            Transform parent = getParent(go);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }
            //UI内3d对象 目前采用直接在canvas下添加一个相机，只渲染UI层 
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.localScale = Vector3.one;
            go.AddComponent<UIProxy>();
            callBack(go);
        });
    }

    public void closeUI(string name)
    {
        GameObject go;
        if (uiInstanceDic.TryGetValue(name, out go))
        {
            var tweens = go.GetComponents<TweenBase>().Where(a => !a.show);
            if (tweens.isNullOrEmpty())
            {
                destroyUI(name,go);
            }
            else
            {
                var index = tweens.Count();
                tweens.forEach(b =>
                {
                    b.play();
                    b.oncomplete = () =>
                    {
                        if (--index ==0)
                        {
                            destroyUI(name, go);
                        }
                    };
                });
            }
        }
        else
        {
            UIResourceLoader.inst.unloadUI(name);
        }
    }

    void destroyUI(string name, GameObject go)
    {
        if (go != null)
        {
            Destroy(go);
        }
        if (!name.isNullOrEmpty())
        {
            if (uiInstanceDic.ContainsKey(name))
            {
                uiInstanceDic.Remove(name);
            }
            UIResourceLoader.inst.unloadUI(name);
        }
    }

    public void switchUI(string from, string to)
    {
        showUI(to);
        closeUI(from);
    }


    Transform getParent(GameObject go)
    {
        if (!go)
        {
            Debuger.LogError("go is null");
            return null;
        }
        if (go.tag == "UIDefault")
        {
            if (!defaultParent)
            {
                defaultParent = createUIParent("Default", 0);
            }
            return defaultParent;
        }
        if (go.tag == "UITip")
        {
            if (!tipParent)
            {
                tipParent = createUIParent("Tip", 1);
            }
            return tipParent;
        }
        if (go.tag == "UITop")
        {
            if (!topParent)
            {
                topParent = createUIParent("Top", 2);
            }
            return topParent;
        }
        Debuger.LogError("panel hiberarchy set error：" + go.name);
        return null;
    }

    Transform createUIParent(string name, int siblingIndex)
    {
        if (name.isNullOrEmpty())
        {
            Debuger.LogError("name is null");
            return null;
        }
        var rectTrans = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rectTrans.SetParent(transform);
        rectTrans.gameObject.layer = gameObject.layer;
        rectTrans.SetSiblingIndex(siblingIndex);
        rectTrans.anchorMin = rectTrans.offsetMin = rectTrans.offsetMax = Vector2.zero;
        rectTrans.anchorMax = Vector2.one;
        rectTrans.localScale = Vector3.one;
        return rectTrans;
    }

    public bool isShowing(string name)
    {
        if (name == null)
        {
            Debuger.LogError("name is null");
            return false;
        }
        GameObject go;
        if (uiInstanceDic.TryGetValue(name, out go))
        {
            return go.activeSelf;
        }
        else
        {
            return false;
        }
    }
}
