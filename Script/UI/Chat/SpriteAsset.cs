using UnityEngine;
using System.Collections.Generic;

public class SpriteAsset : ScriptableObject
{
    /// <summary>
    /// 图片资源
    /// </summary>
    public Texture texSource;
    /// <summary>
    /// 所有sprite信息 SpriteAssetInfor类为具体的信息类
    /// </summary>
    public List<SpriteInfor> listSpriteInfor;

    public Vector2 mSourceSize = Vector2.zero;
    public Vector2 sourceSize {
        get {
            if (texSource != null && mSourceSize == Vector2.zero)
            {
                mSourceSize = new Vector2(texSource.width, texSource.height);
            }
            return mSourceSize;
        }
    }

    protected List<string> styleSpriteNames;

    void OnEnable() {

        if (styleSpriteNames == null)
        {
            styleSpriteNames = new List<string>();
            string tag = null;
            foreach (SpriteInfor sInfo in listSpriteInfor)
            {
                tag = sInfo.name.Split('_')[0];
                if (styleSpriteNames.IndexOf(tag) < 0)
                {
                    styleSpriteNames.Add(tag);
                }
            }            
        }
    }
}

[System.Serializable]
public class SpriteInfor
{
    /// <summary>
    /// ID
    /// </summary>
    public int ID;
    /// <summary>
    /// 名称
    /// </summary>
    public string name;
    /// <summary>
    /// 中心点
    /// </summary>
    public Vector2 pivot;
    /// <summary>
    ///坐标&宽高
    /// </summary>
    public Rect rect;
    /// <summary>
    /// 精灵
    /// </summary>
    public Sprite sprite;
}