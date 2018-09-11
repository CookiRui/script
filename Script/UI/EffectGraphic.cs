using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectGraphic : Graphic
{
    protected Vector3[] vertexPosition;
    protected Vector2[] vertexUV;
    protected Color32[] vertexColors;
    protected int[] vertexIndices;
    protected Texture sampleMainTexture;

    protected Color32 colorVertex = Color.white;
    protected Vector2 uvVertex = Vector2.zero;

    protected bool useQuadPaint = false;

    public override Texture mainTexture
    {
        get
        {
            return sampleMainTexture;              
        }
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (vertexPosition == null) return;
        vh.Clear();
        colorVertex = this.color;
        if (useQuadPaint)
        {
            for (int index = 0, length = vertexPosition.Length / 4; index < length; index++)
            {
                UIVertex[] vbo = new UIVertex[4];
                for(int i = 0;i < 4;i++)
                {
                    var vert = UIVertex.simpleVert;
                    if (vertexColors != null) colorVertex = vertexColors[i + index * 4];
                    vert.color = colorVertex;
                    vert.position = vertexPosition[i + index * 4];
                    if (vertexUV != null) uvVertex = vertexUV[i + index * 4];
                    vert.uv0 = uvVertex;            
                    vbo[i] = vert;
                }
                vh.AddUIVertexQuad(vbo);
            }
        }
        else {
            if (vertexIndices == null) return;
            for (int index = 0, length = vertexPosition.Length; index < length; index++)
            {
                if (vertexColors != null) colorVertex = vertexColors[index];
                if (vertexUV != null) uvVertex = vertexUV[index];
                vh.AddVert(vertexPosition[index], colorVertex, uvVertex);
            }

            for (int index = 0, length = vertexIndices.Length / 3; index < length; index++)
            {
                vh.AddTriangle(vertexIndices[0 + index * 3], vertexIndices[1 + index * 3], vertexIndices[2 + index * 3]);
            }
        }
    }

    public void fillVertextData(Vector3[] vertexPosition, Vector2[] vertexUV, Color32[] vertexColors, int[] vertexIndices)
    {
        if (vertexPosition == null || vertexIndices == null) return;
        if (useQuadPaint)
        {
            this.vertexPosition = new Vector3[vertexIndices.Length / 3 * 4];
            this.vertexUV = new Vector2[vertexIndices.Length / 3 * 4];
            this.vertexColors = new Color32[vertexIndices.Length / 3 * 4];
            for (int index = 0, length = vertexIndices.Length / 3; index < length; index++)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.vertexPosition[index * 4 + i] = vertexPosition[vertexIndices[i % 3 + index * 3]];
                    //this.vertexPosition[index * 4 + i] = PixelAdjustPoint(this.vertexPosition[index * 4 + i]);
                    if (vertexUV != null) this.vertexUV[index * 4 + i] = vertexUV[vertexIndices[i % 3 + index * 3]];
                    else this.vertexUV[index * 4 + i] = uvVertex;
                    if (vertexColors != null) this.vertexColors[index * 4 + i] = vertexColors[vertexIndices[i % 3 + index * 3]];
                    else this.vertexColors[index * 4 + i] = colorVertex;
                }
            }
        }
        else {
            for (int index = 0, length = vertexPosition.Length; index < length; index++)
            {
                vertexPosition[index] = PixelAdjustPoint(vertexPosition[index]); 
            }
            this.vertexPosition = vertexPosition;
            this.vertexUV = vertexUV;
            this.vertexColors = vertexColors;
            this.vertexIndices = vertexIndices;
        }
        this.SetVerticesDirty();
    }


    public static EffectGraphic get(RectTransform rectTrans)
    {
        if (rectTrans == null) return null;
        EffectGraphic effect = rectTrans.GetComponent<EffectGraphic>();
        if (!effect) effect = rectTrans.gameObject.AddComponent<EffectGraphic>();
        return effect;
    }
}
