using UnityEngine;

public partial class SpriteGraphic
{
    public SpriteAsset m_spriteAsset;
    //[HideInInspector]
    //public InlineNormalText mTextComponent;
    public override Texture mainTexture
    {
        get
        {
            if (m_spriteAsset == null)
                return s_WhiteTexture;

            if (m_spriteAsset.texSource == null)
                return s_WhiteTexture;
            else
                return m_spriteAsset.texSource;
        }
    }
    protected override void OnEnable()
    {
        //不调用父类的OnEnable 渲染整张图片
        //base.OnEnable();  
        //mTextComponent = GetComponentInParent<InlineNormalText>();
        //if (mTextComponent) mTextComponent.SetVerticesDirty();
    }

#if UNITY_EDITOR
    //在编辑器下 
    protected override void OnValidate()
    {
        base.OnValidate();
        //Debug.Log("Texture ID is " + this.texture.GetInstanceID());
        //this.SetLayoutDirty();
        //if (mTextComponent) mTextComponent.SetVerticesDirty();
        //this.SetMaterialDirty();
    }
#endif

    //只处理渲染数据
    public override void SetVerticesDirty()
    { 
        //DO Nothing
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
    }

    /// <summary>
    /// 绘制后 需要更新材质
    /// </summary>
    public new void UpdateMaterial()
    {
        base.UpdateMaterial();
    }

}
