using System;
using UnityEngine;
using UnityEngine.UI;

public class CircleImage : Image
{
    const int FILL_PERCENT = 100;
    float thickness = 5;

    [SerializeField]
    [Range(3, 360)]
    int _segments = 36;

    public int segments
    {
        get { return _segments; }
        set
        {
            if (_segments != value)
            {
                _segments = value;
                SetVerticesDirty();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(transform);
#endif
            }
        }
    }

    private float width;
    private float height;

    private float uvWidth = 1;
    private float uvHeight = 1;

    private Vector2 uvLeftDown = Vector2.zero;

    private Sprite cacheSprite = null;

    private RectTransform _curRectTransform;
    private RectTransform curRectTransform {
        get {
            if (_curRectTransform == null)
            {
                _curRectTransform = rectTransform;
            }
            return _curRectTransform;
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        width = curRectTransform.rect.width;
        height = curRectTransform.rect.height;
        this.thickness = (float)Mathf.Clamp(this.thickness, 0, width * 0.5f);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        //float outer = -rectTransform.pivot.x * width; //错误做法
        //float inner = -rectTransform.pivot.x * width + this.thickness;

        if (this.sprite != null)
        {
            if (cacheSprite != this.sprite)
            {
                cacheSprite = this.sprite;
                GetSelectedSpriteRect(cacheSprite, out uvLeftDown, out uvWidth, out uvHeight);
            }         
        }
        else 
        {
            cacheSprite = null;
            uvWidth = 1;
            uvHeight = 1;
        }

        width = curRectTransform.rect.width;
        height = curRectTransform.rect.height;

        float outer = -0.5f * width; //顺时针绘制为负值
        float inner = -0.5f * width + this.thickness;

        vh.Clear();

        Vector2 prevX = Vector2.zero;
        Vector2 prevY = Vector2.zero;

        Vector2 uv0 = Vector2.zero;
        Vector2 uv1 = Vector2.up;
        Vector2 uv2 = Vector2.one;
        Vector2 uv3 = Vector2.right;
        Vector2 pos0;
        Vector2 pos1;
        Vector2 pos2;
        Vector2 pos3;

        float angleByStep = (FILL_PERCENT / 100f * (Mathf.PI * 2f)) / segments;
        float currentAngle = 0f;//必然有一个固定起点（1 * outer，0）
        float offset = 0.5f; //固定相对中心点(0,0) -- 对应UV偏移0.5f(左下角0,0)

        for (int i = 0; i < segments + 1; i++)
        {

            float c = Mathf.Cos(currentAngle);
            float s = Mathf.Sin(currentAngle);

            StepThroughPointsAndFill(outer, inner, ref prevX, ref prevY, out pos0, out pos1, out pos2, out pos3, c, s);

            uv0 = new Vector2(pos0.x / width * uvWidth + uvLeftDown.x + offset * uvWidth, pos0.y / height * uvHeight + uvLeftDown.y + offset * uvHeight);
            uv1 = new Vector2(pos1.x / width * uvWidth + uvLeftDown.x + offset * uvWidth, pos1.y / height * uvHeight + uvLeftDown.y + offset * uvHeight);
            uv2 = new Vector2(pos2.x / width * uvWidth + uvLeftDown.x + offset * uvWidth, pos2.y / height * uvHeight + uvLeftDown.y + offset * uvHeight);
            uv3 = new Vector2(pos3.x / width * uvWidth + uvLeftDown.x + offset * uvWidth, pos3.y / height * uvHeight + uvLeftDown.y + offset * uvHeight);

            vh.AddUIVertexQuad(SetVbo(new[] { pos0, pos1, pos2, pos3 }, new[] { uv0, uv1, uv2, uv3 }));

            currentAngle += angleByStep;
        }
    }

    private void GetSelectedSpriteRect(Sprite sprite,out Vector2 uvLeftDown,out float uvWidth,out float uvHeight)
    {
       float widthTexture = sprite.texture.width;
       float heightTexture = sprite.texture.height;
       uvLeftDown = new Vector2(sprite.rect.x / widthTexture, sprite.rect.y / heightTexture);
       uvWidth = sprite.rect.width / widthTexture;
       uvHeight = sprite.rect.height / heightTexture;
    }

    private void StepThroughPointsAndFill(float outer, float inner, ref Vector2 prevX, ref Vector2 prevY, 
        out Vector2 pos0, out Vector2 pos1, out Vector2 pos2, out Vector2 pos3, float c, float s)
    {
        pos0 = prevX;
        pos1 = new Vector2(outer * c, outer * s);

        pos2 = Vector2.zero;
        pos3 = Vector2.zero;

        prevX = pos1;
        prevY = pos2;

    }

    protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
    {
        UIVertex[] vbo = new UIVertex[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            vbo[i] = vert;
        }
        return vbo;
    }

}
