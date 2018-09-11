using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using System.Linq;

public class UIResourceLoader : ResourceLoader
{
    public static UIResourceLoader inst;

    private readonly int version = 1;

    void Awake()
    {
        inst = this;
    }

#if !SCENEEDITOR_TOOL
    //可以使用UIEditor每次打包公有资源时先生成一个配置文件
    //此处可以通过解析配置文件动态的产生预加载列表
    protected override IEnumerable<string> getPreLoadAssetPath()
    {
        Debug.Log("UIResourceLoader  getPreLoadAssetPath ");
        var path = getRealPath("ui/common/commonconfig.ab");

        //jlx 20170315-log:移动平台不能用这个方法
        //if (!File.Exists(path))
        //{
        //    Debug.Log("UIResourceLoader  getPreLoadAssetPath path 不存在：" + path);
        //    return null;
        //}

        var ab = AssetBundle.LoadFromFile(path);
        if (!ab)
        {
            Debug.Log("UIResourceLoader  getPreLoadAssetPath ab 为空：" + path);
            return null;
        }

        using (var ms = new MemoryStream(ab.LoadAsset<TextAsset>("config.bytes").bytes))
        {
            using (var br = new BinaryReader(ms))
            {
                if (br.ReadInt32() != MagicNumberUtil.generateUICommonConfig())
                {
                    Debug.Log("UI commonconfig.ab MagicNumber 不匹配");
                    return null;
                }

                if (br.ReadInt32() != version)
                {
                    Debug.Log("UI commonconfig.ab 版本 不匹配");
                    return null;
                }
                var count = br.ReadInt32();
                Debug.Log("++++++++++++++++++UIResourceLoader  getPreLoadAssetPath count :" + count);

                if (count <= 0)
                    return null;

                var paths = new string[count];
                for (int i = 0; i < count; i++)
                {
                    paths[i] = "ui/" + br.ReadString();
                    Debug.Log(paths[i]);
                }
                return paths;
            }
        }
    }

    public Sprite getSkillIcon(string iconName)
    {
        return getAssetFromPreloadAssetBundle<Sprite>("common/icon/skill.ab", iconName);
    }

    public Sprite getItemIcon(string iconName)
    {
        return getAssetFromPreloadAssetBundle<Sprite>("common/icon/item.ab", iconName);
    }

    public Sprite getEmotionIcon(string iconName)
    {
        return getAssetFromPreloadAssetBundle<Sprite>("common/icon/emotion.ab", iconName);
    }

    public Sprite getHeadIcon(string iconName)
    {
        return getAssetFromPreloadAssetBundle<Sprite>("common/icon/head.ab", iconName);
    }

    //一个UI对应一个AssetBundle，所以关闭UI的时候直接关闭AssetBundle
    //这里的ui name就指原始的ui名称，如loginpanel
    public void loadUI(string uiName, Action<GameObject> callback)
    {
        getAssetBundleAsync(getUIAssetBundlePath(uiName), (ab) =>
        {

            if (ab == null)
            {
                Debug.LogWarning("加载assetbundle 出错 " + uiName);
                return;
            }

            StartCoroutine(loadAssetAsync(ab, uiName, callback));
        });
    }

    public void unloadUI(string uiName)
    {
        removeAssetBundle(getUIAssetBundlePath(uiName));
    }

    IEnumerator loadAssetAsync(AssetBundle ab, string uiName, Action<GameObject> callback)
    {
        var req = ab.LoadAssetAsync<GameObject>(uiName);
        yield return req;
        callback((GameObject)req.asset);
    }

    string getUIAssetBundlePath(string uiName)
    {
        return "UI/Panel/" + uiName + ".ab";
    }

    List<UIAtlas> atlass = new List<UIAtlas>();

    public void loadSprite(RecorderSprite sprite, UnityAction<Sprite, Material> callback)
    {
        if (!sprite || sprite.name.isNullOrEmpty())
        {
            Debug.LogError("sprite is null");
            return;
        }
        if (callback.isNull())
        {
            Debug.LogError("callback is null");
            return;
        }
        if (!loadSpriteFromAtlas(sprite, callback))
        {
            StartCoroutine(loadSpriteFromAB(sprite, callback));
        }
    }

    bool loadSpriteFromAtlas(RecorderSprite sprite, UnityAction<Sprite, Material> callback)
    {
        if (!sprite || sprite.name.isNullOrEmpty())
        {
            Debug.LogError("sprite is null");
            return false;
        }
        if (callback.isNull())
        {
            Debug.LogError("callback is null");
            return false;
        }
        if (atlass.isNullOrEmpty()) return false;

        var priorityAtlas = atlass.FirstOrDefault(a => a.name == sprite.atlas);
        if (priorityAtlas)
        {
            var s = priorityAtlas.getSprite(sprite.name);
            if (s)
            {
                callback(s, priorityAtlas.material);
                return true;
            }
        }

        foreach (var atlas in atlass)
        {
            if (priorityAtlas && priorityAtlas.name == atlas.name) continue;

            var s = atlas.getSprite(sprite.name);
            if (s)
            {
                callback(s, atlas.material);
                return true;
            }
        }
        return false;
    }

    IEnumerator loadSpriteFromAB(RecorderSprite sprite, UnityAction<Sprite, Material> callback)
    {
        if (!sprite || sprite.name.isNullOrEmpty())
        {
            Debug.LogError("sprite is null");
            yield break;
        }
        if (callback.isNull())
        {
            Debug.LogError("callback is null");
            yield break;
        }

        if (!preloadAssetBundles.isNullOrEmpty())
        {
            foreach (var ab in preloadAssetBundles)
            {
                if (ab.name.Contains("common/atlas/"))
                {
                    if (!atlass.Any(a => a.name == ab.name))
                    {
                        yield return StartCoroutine(loadAtlas(ab));
                    }
                    if (loadSpriteFromAtlas(sprite, callback))
                    {
                        yield break;
                    }
                }
            }
        }
        Debug.LogError("啥子都没找到!!!请检查图集。图集名：" + sprite.atlas + "  图片名：" + sprite.name);
        callback(null, null);
    }

    IEnumerator loadAtlas(AssetBundle ab)
    {
        var atlas = new UIAtlas
        {
            name = ab.name,
            compressed = !ab.name.Contains("Uncompressed"),
        };
        var materialRequest = ab.LoadAllAssetsAsync<Material>();
        yield return materialRequest;
        if (!materialRequest.isNull() && materialRequest.asset)
        {
            atlas.material = materialRequest.asset as Material;
        }
        var spritesRequest = ab.LoadAllAssetsAsync<Sprite>();
        yield return spritesRequest;
        if (!spritesRequest.isNull() && !spritesRequest.allAssets.isNullOrEmpty())
        {
            var sprites = new List<Sprite>();
            spritesRequest.allAssets.forEach(a =>
            {
                var sprite = a as Sprite;
                if (!sprite.texture.name.Contains("Alpha"))
                {
                    sprites.Add(sprite);
                }
            });
            atlas.sprites = sprites;
        }
        atlass.Add(atlas);
    }

    /// <summary>
    /// jlx 2017.02.09
    /// 待验证？
    /// </summary>
    public void unloadAtlas()
    {
        if (atlass.isNullOrEmpty()) return;
        atlass.ForEach(a =>
        {
            if (a.material)
            {
                Resources.UnloadAsset(a.material);
            }
            if (!a.sprites.isNullOrEmpty())
            {
                a.sprites.forEach(b =>
                {
                    Resources.UnloadAsset(b);
                });
            }
        });
    }

    Dictionary<string, int> emotionCounter = new Dictionary<string, int>();

    public void loadEmotion(string name, UnityAction<Sprite[]> callback)
    {
        if (name.isNullOrEmpty())
        {
            Debug.LogError("name is null");
            return;
        }
        if (callback.isNull())
        {
            Debug.LogError("callback is null");
            return;
        }
        getAssetBundleAsync(getEmotionABPath(name), ab =>
        {
            if (!ab)
            {
                Debug.LogError("ab is null");
                return;
            }
            StartCoroutine(loadEmotion(ab, name, callback));
        });
    }

    IEnumerator loadEmotion(AssetBundle ab, string name, UnityAction<Sprite[]> callback)
    {
        if (!ab)
        {
            Debug.LogError("ab is null");
            yield break;
        }
        if (name.isNullOrEmpty())
        {
            Debug.LogError("name is null");
            yield break;
        }
        if (callback.isNull())
        {
            Debug.LogError("callback is null");
            yield break;
        }

        var request = ab.LoadAllAssetsAsync<Sprite>();
        yield return request;
        callback(request.allAssets.Cast<Sprite>().ToArray());
        if (emotionCounter.ContainsKey(name))
        {
            emotionCounter[name] += 1;
        }
        else
        {
            emotionCounter.Add(name, 1);
        }
    }

    string getEmotionABPath(string name)
    {
        return "UI/Emotion/" + name + ".ab";
    }

    public void unloadEmotion(string name)
    {
        if (name.isNullOrEmpty())
        {
            Debug.LogError("name is null");
            return;
        }

        if (emotionCounter.isNullOrEmpty() ||
            !emotionCounter.ContainsKey(name))
        {
            return;
        }

        if (emotionCounter[name] <= 1)
        {
            removeAssetBundle(getEmotionABPath(name));
            emotionCounter.Remove(name);
            return;
        }
        emotionCounter[name] -= 1;
    }

    public void unloadAllEmotion()
    {
        if (emotionCounter.isNullOrEmpty()) return;
        emotionCounter.forEach(a =>
        {
            removeAssetBundle(getEmotionABPath(a.Key));
        });
        emotionCounter.Clear();
    }

#endif
}
