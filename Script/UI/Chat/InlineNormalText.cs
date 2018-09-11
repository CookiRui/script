using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//[ExecuteInEditMode]
[AddComponentMenu("UI/InlineNormalText", 200)]
public class InlineNormalText : Text, IPointerClickHandler
{
    /// <summary>
    /// 用正则取标签属性 名称-大小-宽度比例
    /// </summary>
    private static readonly Regex m_spriteTagRegex =
          new Regex(@"<quad name=(.+?) size=(-?\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);
    /// <summary>
    /// 需要渲染的图片信息列表
    /// </summary>
    private TList<InlineSpriteInfor> listSprite;
    /// <summary>
    /// 图片资源
    /// </summary>
    private SpriteAsset m_spriteAsset;

    /// 标签的信息列表listTagInfor

    /// <summary>
    /// 图片渲染组件
    /// </summary>
    private SpriteGraphic m_spriteGraphic;
    /// <summary>
    /// CanvasRenderer
    /// </summary>
    private CanvasRenderer m_spriteCanvasRenderer;

    /// <summary>
    /// 图片渲染管理
    /// </summary>
    //private SpriteGraphicManager m_SGManager;
    /// <summary>
    /// 渲染mesh
    /// </summary>
    private Mesh m_spriteMesh;
    #region 动画标签解析
    //最多动态表情数量
    int mAnimLimitNum = 8;
    TList<int> mEmojiIndexList;
    TList<TList<SpriteTagInfor>> m_AnimSpiteTag;
    TList<InlineSpriteInfor[]> m_AnimSpriteInfor;
    #endregion

    //解析超链接
    string m_OutputText;

    //private IList<UIVertex> _OldVerts;

    #region 超链接
    /// <summary>
    /// 超链接信息列表
    /// </summary>
    private readonly TList<HrefInfo> m_HrefInfos = new TList<HrefInfo>();

    /// <summary>
    /// 文本构造器
    /// </summary>
    private static readonly StringBuilder s_TextBuilder = new StringBuilder();

    /// <summary>
    /// 超链接正则
    /// </summary>
    private static readonly Regex s_HrefRegex =
        new Regex(@"<a href=([^>\n\s]+)>(.*?) line=([0,1]) color=(red|green|blue|#[0-9|a-z|A-Z]{6}) </a>", RegexOptions.Singleline);

    private readonly UIVertex[] m_TempVerts = new UIVertex[4];

    [System.Serializable]
    public class HrefClickEvent : UnityEvent<string> { }

    [SerializeField]
    private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

    /// <summary>
    /// 超链接点击事件
    /// </summary>
    public HrefClickEvent onHrefClick
    {
        get { return m_OnHrefClick; }
        set { m_OnHrefClick = value; }
    }

    #endregion

    public SpriteAsset spriteAsset {
        get {
#if UNITY_EDITOR
            Debug.Log("m_spriteAsset:" + (m_spriteAsset==null));
#endif
            return m_spriteAsset;
        }
        set {
            if (value != m_spriteAsset)
            {
                m_spriteAsset = value;
                m_spriteGraphic.m_spriteAsset = value;
            }
        }
    }

    //富文本长度数据
    private int[] mRichTextParams = new int[3];
    public int[] richTextParams
    {
        get {
            return mRichTextParams;
        }
    }



    protected override void Awake()
    {
        //base.Awake();
        checkComponent();
    }

    private void checkComponent()
    {
        if (m_spriteGraphic == null)
        {
            m_spriteGraphic = GetComponentInChildren<SpriteGraphic>(true);
            if (m_spriteGraphic == null)
            {
                GameObject obj = new GameObject("Image", typeof(SpriteGraphic));
                obj.transform.SetParent(this.transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                m_spriteGraphic = obj.GetComponent<SpriteGraphic>();
            }
            else m_spriteGraphic.gameObject.SetActive(true);
            //m_spriteGraphic.gameObject.SetActive(false);
            
        }

        if (m_spriteCanvasRenderer == null)
        {
            m_spriteCanvasRenderer = m_spriteGraphic.GetComponentInChildren<CanvasRenderer>();
            if (m_spriteMesh == null) m_spriteMesh = new Mesh(); else m_spriteMesh.Clear();
            m_spriteCanvasRenderer.SetMesh(m_spriteMesh);
            m_spriteGraphic.UpdateMaterial();
        }

        if (m_spriteGraphic != null)
            m_spriteAsset = m_spriteGraphic.m_spriteAsset;
    }

    /// <summary>
    /// 初始化 
    /// </summary>
    protected override void OnEnable()
    {
        //在编辑器中，可能在最开始会出现一张图片，就是因为没有激活文本，在运行中是正常的。可根据需求在编辑器中选择激活...
        base.OnEnable();
        checkComponent();    
        //对齐几何
        if (!alignByGeometry)
        {
            alignByGeometry = true;
        }
        else {
            //启动 更新顶点
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// 在设置顶点时调用
    /// </summary>】、

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();

        //解析超链接
        m_OutputText = GetHrefRichText();

        //解析标签属性
        //if (listTagInfor != null) listTagInfor.Clear(); else listTagInfor = new BetterList<SpriteTagInfor>();
        if (mEmojiIndexList != null) mEmojiIndexList.Clear(); else mEmojiIndexList = new TList<int>();
        if (m_AnimSpiteTag != null) m_AnimSpiteTag.Clear(); else m_AnimSpiteTag = new TList<TList<SpriteTagInfor>>();

        if (m_spriteAsset == null)
            return;

        //if (m_spriteGraphic) m_spriteGraphic.gameObject.SetActive(true);
        MatchEmojiRichText();
        
    }

    /// <summary>
    /// 绘制模型
    /// </summary>
    /// <param name="toFill"></param>
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        //base.OnPopulateMesh(toFill);
        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        //cachedTextGenerator.Populate(m_OutputText, settings);
        this.cachedTextGenerator.PopulateWithErrors(m_OutputText, settings, base.gameObject);
        Rect inputRect = rectTransform.rect;

        // get the text alignment anchor point for the text in local space
        Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
        Vector2 refPoint = Vector2.zero;
        refPoint.x = (textAnchorPivot.x == 1 ? inputRect.xMax : inputRect.xMin);
        refPoint.y = (textAnchorPivot.y == 0 ? inputRect.yMin : inputRect.yMax);

        // Determine fraction of pixel to offset text mesh.
        Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

        // Apply the offset to the vertices
        IList<UIVertex> verts = this.cachedTextGenerator.verts;
        float unitsPerPixel = 1f / pixelsPerUnit;
        //Last 4 verts are always a new line...
        int vertCount = verts.Count - 4;
        //int vertCount = verts.Count;

        toFill.Clear();

        if (m_spriteAsset != null)
        {
            //清乱码
            /*for (int i = 0; i < m_AnimSpiteTag.Count; i++)
            {
                if(m_AnimSpiteTag[i].Length > 0)
                {
                    //UGUIText不支持<quad/>标签，表现为乱码，我这里将他的uv全设置为0,清除乱码
                    for (int m = m_AnimSpiteTag[i][0].index * 4; m < m_AnimSpiteTag[i][0].index * 4 + 4; m++)
                    {
                        //if (m >= verts.Count) return;
                        if (m >= verts.Count)
                        {
                            //Debug.LogError("verts:" + verts.Count + "_" + m);
                            return;
                        }
                        UIVertex tempVertex = verts[m];
                        tempVertex.uv0 = Vector2.zero;
                        verts[m] = tempVertex;
                    }
                }
            }*/

            //移除错误顶点 截断
            for (int i = 0; i < m_AnimSpiteTag.Count; )
            {
                if (m_AnimSpiteTag[i].Count > 0)
                {
                    //UGUIText不支持<quad/>标签，表现为乱码，我这里将他的uv全设置为0,清除乱码
                    TList<SpriteTagInfor> tempSpriteTag = m_AnimSpiteTag[i];
                    for (int j = 0; j < tempSpriteTag.Count; )
                    {
                        bool isRemove = false;
                        for (int m = tempSpriteTag[j].index * 4; m < tempSpriteTag[j].index * 4 + 4; m++)
                        {
                            //if (m >= verts.Count) return;
                            if (m >= vertCount)
                            {
                                isRemove = true;
                                break;
                            }
                            //if (j == 0)
                            {
                                UIVertex tempVertex = verts[m];
                                tempVertex.uv0 = Vector2.zero;
                                verts[m] = tempVertex;
                            }
                        }

                        //移除对应错误顶点的数据 
                        if (isRemove) tempSpriteTag.RemoveAt(j); else j++;
                    }
                    if (tempSpriteTag.Count == 0)
                    {
                        m_AnimSpiteTag.RemoveAt(i);
                        mEmojiIndexList.RemoveAt(i);
                        continue;
                    }
                }
                i++;
            }
        }

        //计算标签  计算偏移值后 再计算标签的值 
        //  CalcQuadTag(verts);

        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }


        if (m_spriteAsset != null)
        {
            //计算标签 计算偏移值后 再计算标签的值
            List<UIVertex> vertsTemp = new List<UIVertex>();
            for (int i = 0; i < vertCount; i++)
            {
                UIVertex tempVer = new UIVertex();
                toFill.PopulateUIVertex(ref tempVer, i);
                vertsTemp.Add(tempVer);
            }
            CalcQuadTag(vertsTemp);
        }
       

        m_DisableFontTextureRebuiltCallback = false;

        //绘制图片
        DrawSprite();

        HerfBoxEncapsulate(toFill);
        AddHerfUnderlineQuad(toFill,settings);
    }

    #region 处理超链接的包围盒
    void HerfBoxEncapsulate(VertexHelper toFill)
    {

        // 处理超链接包围框
        UIVertex vert = new UIVertex();
        foreach (var hrefInfo in m_HrefInfos)
        {
            hrefInfo.boxes.Clear();
            if (hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框
            toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
            {
                if (i >= toFill.currentVertCount)
                {
                    break;
                }

                toFill.PopulateUIVertex(ref vert, i);
                pos = vert.position;
                if (pos.x < bounds.min.x) // 换行重新添加包围框
                {
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                }
            }
            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        } 
    }
    #endregion

    #region 处理超链接的下划线--拉伸实现
    void AddHerfUnderlineQuad(VertexHelper toFill, TextGenerationSettings settings)
    {

        TextGenerator _UnderlineText = new TextGenerator();
        _UnderlineText.Populate("_", settings);
        IList<UIVertex> _TUT = _UnderlineText.verts;

        foreach (var hrefInfo in m_HrefInfos)
        {
            if (!hrefInfo.useLine || hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }

            for (int i = 0; i < hrefInfo.boxes.Count; i++)
            {
                Vector3 _StartBoxPos = new Vector3(hrefInfo.boxes[i].x, hrefInfo.boxes[i].y, 0.0f);
                Vector3 _EndBoxPos = _StartBoxPos + new Vector3(hrefInfo.boxes[i].width, 0.0f, 0.0f);
                AddUnderlineQuad(toFill, _TUT, _StartBoxPos, _EndBoxPos);
            }
        }

    }
    #endregion

    #region 添加下划线
    void AddUnderlineQuad(VertexHelper _VToFill, IList<UIVertex> _VTUT, Vector3 _VStartPos, Vector3 _VEndPos)
    {
        Vector3[] _TUnderlinePos = new Vector3[4];
        _TUnderlinePos[0] = _VStartPos;
        _TUnderlinePos[1] = _VEndPos;
        _TUnderlinePos[2] = _VEndPos + new Vector3(0, fontSize * 0.2f, 0);
        _TUnderlinePos[3] = _VStartPos + new Vector3(0, fontSize * 0.2f, 0);

        for (int i = 0; i < 4; ++i)
        {
            int tempVertsIndex = i & 3;
            m_TempVerts[tempVertsIndex] = _VTUT[i % 4];
            m_TempVerts[tempVertsIndex].color = Color.blue;

            m_TempVerts[tempVertsIndex].position = _TUnderlinePos[i];

            if (tempVertsIndex == 3)
                _VToFill.AddUIVertexQuad(m_TempVerts);
        }
    }
    #endregion

    /// <summary>
    /// 解析quad标签  主要清除quad乱码 获取表情的位置
    /// </summary>
    /// <param name="verts"></param>
    void CalcQuadTag(IList<UIVertex> verts)
    {

        if (m_AnimSpriteInfor != null) m_AnimSpriteInfor.Clear(); else m_AnimSpriteInfor = new TList<InlineSpriteInfor[]>();

        //通过标签信息来设置需要绘制的图片的信息
        if (listSprite != null) listSprite.Clear(); else listSprite = new TList<InlineSpriteInfor>();

        for (int i = 0; i < m_AnimSpiteTag.Count; i++)
        {
            TList<SpriteTagInfor> tempTagInfor = m_AnimSpiteTag[i];
            InlineSpriteInfor[] tempSpriteInfor = new InlineSpriteInfor[tempTagInfor.Count];
            for (int j = 0; j < tempTagInfor.Count; j++)
            {
                tempSpriteInfor[j] = new InlineSpriteInfor();
                //Vector3 offset = Vector3.zero;
                Vector3 offset = new Vector3(0, tempTagInfor[j].size.y * 0.2f, 0);

                //if (tempTagInfor[j].index == 0)
                //{
                //    Vector2 anchorPivot = GetTextAnchorPivot(alignment);
                //    Vector2 rectSize = rectTransform.sizeDelta;
                //    //相对中心点位置 '- 0.5' + '0.5'
                //    tempSpriteInfor[j].textpos = -rectSize / 2.0f + new Vector2(rectSize.x * anchorPivot.x, rectSize.y * anchorPivot.y - tempTagInfor[i].size.y);
                //}
                //else
                //{
                //    tempSpriteInfor[j].textpos = verts[((tempTagInfor[j].index) * 4) - 1].position;
                //}

                tempSpriteInfor[j].textpos = verts[((tempTagInfor[j].index + 1) * 4) - 1].position;//第一字符
                //设置图片的位置
                tempSpriteInfor[j].vertices = new Vector3[4];
                tempSpriteInfor[j].vertices[0] = new Vector3(0, 0, 0) + tempSpriteInfor[j].textpos - offset;
                tempSpriteInfor[j].vertices[1] = new Vector3(tempTagInfor[j].size.x, tempTagInfor[j].size.y, 0) + tempSpriteInfor[j].textpos - offset;
                tempSpriteInfor[j].vertices[2] = new Vector3(tempTagInfor[j].size.x, 0, 0) + tempSpriteInfor[j].textpos - offset;
                tempSpriteInfor[j].vertices[3] = new Vector3(0, tempTagInfor[j].size.y, 0) + tempSpriteInfor[j].textpos - offset;

                //计算其uv
                Rect newSpriteRect = m_spriteAsset.listSpriteInfor[0].rect;
                for (int m = 0; m < m_spriteAsset.listSpriteInfor.Count; m++)
                {
                    //通过标签的名称去索引spriteAsset里所对应的sprite的名称
                    //if (tempTagInfor[j].name == m_spriteAsset.listSpriteInfor[m].name)
                    if (m_spriteAsset.listSpriteInfor[m].name.StartsWith(tempTagInfor[j].name))
                    {
                        newSpriteRect = m_spriteAsset.listSpriteInfor[m].rect;
                        break;
                    }
                }
                //Vector2 newTexSize = m_spriteAsset.sourceSize;

                tempSpriteInfor[j].uv = new Vector2[4];
                tempSpriteInfor[j].uv[0] = new Vector2(newSpriteRect.x / m_spriteAsset.sourceSize.x, newSpriteRect.y / m_spriteAsset.sourceSize.y);
                tempSpriteInfor[j].uv[1] = new Vector2((newSpriteRect.x + newSpriteRect.width) / m_spriteAsset.sourceSize.x, (newSpriteRect.y + newSpriteRect.height) / m_spriteAsset.sourceSize.y);
                tempSpriteInfor[j].uv[2] = new Vector2((newSpriteRect.x + newSpriteRect.width) / m_spriteAsset.sourceSize.x, newSpriteRect.y / m_spriteAsset.sourceSize.y);
                tempSpriteInfor[j].uv[3] = new Vector2(newSpriteRect.x / m_spriteAsset.sourceSize.x, (newSpriteRect.y + newSpriteRect.height) / m_spriteAsset.sourceSize.y);
                //Debug.Log(newSpriteRect.x + "_" + newSpriteRect.y + "_" + newSpriteRect.min.ToString());
                //tempSpriteInfor[j].uv[0] = new Vector2(newSpriteRect.min.x / m_spriteAsset.sourceSize.x, newSpriteRect.min.y / m_spriteAsset.sourceSize.y);
                //tempSpriteInfor[j].uv[1] = new Vector2((newSpriteRect.min.x + newSpriteRect.width) / m_spriteAsset.sourceSize.x, (newSpriteRect.min.y + newSpriteRect.height) / m_spriteAsset.sourceSize.y);
                //tempSpriteInfor[j].uv[2] = new Vector2((newSpriteRect.min.x + newSpriteRect.width) / m_spriteAsset.sourceSize.x, newSpriteRect.min.y / m_spriteAsset.sourceSize.y);
                //tempSpriteInfor[j].uv[3] = new Vector2(newSpriteRect.min.x / m_spriteAsset.sourceSize.x, (newSpriteRect.min.y + newSpriteRect.height) / m_spriteAsset.sourceSize.y);

                //声明三角顶点所需要的数组
                tempSpriteInfor[j].triangles = new int[6];
            }
            m_AnimSpriteInfor.Add(tempSpriteInfor);
            listSprite.Add(tempSpriteInfor[0]);
        }
        //_OldVerts = verts;
    }

    //#region 更新图片的信息
    //protected void UpdateSpriteInfor()
    //{
    //    if (_OldVerts == null)
    //        return;

    //    CalcQuadTag(_OldVerts);
    //}
    //#endregion

    /// <summary>
    /// 绘制图片
    /// </summary>
    void DrawSprite()
    {
        if (m_spriteAsset == null) return;
        if (listSprite == null) return;
        if (m_spriteMesh == null) m_spriteMesh = new Mesh(); else m_spriteMesh.Clear();

        TList<Vector3> tempVertices = new TList<Vector3>();
        TList<Vector2> tempUv = new TList<Vector2>();
        TList<int> tempTriangles = new TList<int>();

        for (int i = 0; i < listSprite.Count; i++)
        {
            for (int j = 0; j < listSprite[i].vertices.Length; j++)
            {
                tempVertices.Add(listSprite[i].vertices[j]);
            }
            for (int j = 0; j < listSprite[i].uv.Length; j++)
            {
                tempUv.Add(listSprite[i].uv[j]);
            }
            for (int j = 0; j < listSprite[i].triangles.Length; j++)
            {
                tempTriangles.Add(listSprite[i].triangles[j]);
            }
        }
        //计算顶点绘制顺序
        for (int i = 0; i < tempTriangles.Count; i++)
        {
            if (i % 6 == 0)
            {
                int num = i / 6;
                tempTriangles[i] = 0 + 4 * num;
                tempTriangles[i + 1] = 1 + 4 * num;
                tempTriangles[i + 2] = 2 + 4 * num;

                tempTriangles[i + 3] = 1 + 4 * num;
                tempTriangles[i + 4] = 0 + 4 * num;
                tempTriangles[i + 5] = 3 + 4 * num;
            }
        }

        m_spriteMesh.vertices = tempVertices.ToArray();
        m_spriteMesh.uv = tempUv.ToArray();
        m_spriteMesh.triangles = tempTriangles.ToArray();

        m_spriteCanvasRenderer.SetMesh(m_spriteMesh);
        m_spriteGraphic.UpdateMaterial();
    }


    float fTime = 0.0f;
    int iIndex = 0;
    void LateUpdate()
    {
        if (m_spriteAsset == null) return;
        if (mEmojiIndexList.Count == 0) return;
        if (m_AnimSpriteInfor != null) {
            fTime += Time.deltaTime;
            if (fTime >= 0.1f)
            {
                for (int i = 0; i < mEmojiIndexList.Count; i++)
                {
                    if (mEmojiIndexList[i] >= m_AnimSpriteInfor.Count) return;
                    if(m_AnimSpriteInfor[mEmojiIndexList[i]].Length < 2) continue;
                    if (iIndex >= m_AnimSpriteInfor[mEmojiIndexList[i]].Length) iIndex = 0;
                    listSprite[mEmojiIndexList[i]] = m_AnimSpriteInfor[mEmojiIndexList[i]][iIndex];
                }
                DrawSprite();
                iIndex++;
                if (iIndex >= mAnimLimitNum)
                {
                    iIndex = 0;
                }
                fTime = 0.0f;
            }
        }       
    }

    #region 超链接解析
    /// <summary>
    /// 获取超链接解析后的最后输出文本
    /// </summary>
    /// <returns></returns>
    protected string GetHrefRichText()
    {
        //text = text.Replace("\n<quad","<quad");
        s_TextBuilder.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;
        MatchCollection hrefMatchCollection = s_HrefRegex.Matches(text);
        int matchLength = 0;
        int hrefNameLength = 0;
        foreach (Match match in hrefMatchCollection)
        {
            s_TextBuilder.Append(text.Substring(indexText, match.Index - indexText));
            s_TextBuilder.Append(string.Format("<color={0}>", match.Groups[4].Value));// 超链接颜色

            var group = match.Groups[1];
            var show = match.Groups[2];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + show.Length) * 4 - 1,
                useLine = match.Groups[3].Value == "1",
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);

            s_TextBuilder.Append(show.Value);
            s_TextBuilder.Append("</color>");
            indexText = match.Index + match.Length;

            hrefNameLength += show.Length;
            matchLength += match.Length;
        }
        mRichTextParams[2] = hrefNameLength;
        mRichTextParams[0] = this.text.Length - matchLength + mRichTextParams[2];
        s_TextBuilder.Append(text.Substring(indexText, text.Length - indexText));
        return s_TextBuilder.ToString();
    }
     #endregion

    #region 解析动画标签
    /// <summary>
    /// 解析动画标签
    /// </summary>
    protected void MatchEmojiRichText() {
        MatchCollection spriteTagMatchCollection = m_spriteTagRegex.Matches(m_OutputText);
        int matchLength = 0;
        foreach (Match match in spriteTagMatchCollection)
        {

            List<string> tempListName = new List<string>();
            for (int i = 0, len = m_spriteAsset.listSpriteInfor.Count; i < len; i++)
            {
                // Debug.Log((m_spriteAsset.listSpriteInfor[i].name));
                if (m_spriteAsset.listSpriteInfor[i].name.StartsWith(match.Groups[1].Value))
                {
                    tempListName.Add(m_spriteAsset.listSpriteInfor[i].name);
                }
            }
            if (tempListName.Count > 0)
            {
                float size = float.Parse(match.Groups[2].Value);
                float width = float.Parse(match.Groups[3].Value);
                if (size <= 0f) size = this.fontSize;
                if (width <= 0f) width = 1;
                int sLength = tempListName.Count;
                TList<SpriteTagInfor> spriteTagInfor = new TList<SpriteTagInfor>();
                for (int i = 0; i < sLength; i++)
                {
                    SpriteTagInfor tempSpriteTag = new SpriteTagInfor();
                    tempSpriteTag.name = tempListName[i];
                    tempSpriteTag.index = match.Index;
                    tempSpriteTag.size = new Vector2(size * width, size);
                    tempSpriteTag.Length = match.Length;
                    spriteTagInfor.Add(tempSpriteTag);
                }
                //listTagInfor.Add(tempArrayTag[0]);
                m_AnimSpiteTag.Add(spriteTagInfor);
                mEmojiIndexList.Add(m_AnimSpiteTag.Count - 1);
            }

            matchLength += match.Length;
        }
        mRichTextParams[1] = spriteTagMatchCollection.Count;
        mRichTextParams[0] = mRichTextParams[0] - matchLength + mRichTextParams[1];
    }
    #endregion

    /// <summary>
    /// 点击事件检测是否点击到超链接文本
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        foreach (var hrefInfo in m_HrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    m_OnHrefClick.Invoke(hrefInfo.name);
                    return;
                }
            }
        }
    }

    #region 超链接信息类
    /// <summary>
    /// 超链接信息类
    /// </summary>
    protected class HrefInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public bool useLine;

        public readonly List<Rect> boxes = new List<Rect>();
    }
    #endregion
}


[System.Serializable]
public class SpriteTagInfor
{
    /// <summary>
    /// sprite名称
    /// </summary>
    public string name;
    /// <summary>
    /// 对应的字符索引
    /// </summary>
    public int index;
    /// <summary>
    /// 大小
    /// </summary>
    public Vector2 size;

    public int Length;
}


[System.Serializable]
public class InlineSpriteInfor
{
    // 文字的最后的位置
    public Vector3 textpos;
    // 4 顶点 
    public Vector3[] vertices;
    //4 uv
    public Vector2[] uv;
    //6 三角顶点顺序
    public int[] triangles;
}
