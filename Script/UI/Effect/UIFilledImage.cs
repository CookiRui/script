/*
    author:jlx
*/

using UnityEngine;
using UnityEngine.UI;

public class UIFilledImage : Image
{
    public enum UIFilledType
    {
        Horizontal,
        Vertical,
        Circle
    }
    public UIFilledType filledType { get; set; }
    public float amount = 1;
    Material filledMaterial;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!filledMaterial)
        {
            filledMaterial = createMaterial();
        }
    }

    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        var material = base.GetModifiedMaterial(baseMaterial);
        material.SetFloat("_FilledType", (float)filledType);
        material.SetFloat("_Amount", amount);
        material.SetVector("_Rect", getRect());
        return material;
    }

    public void Update()
    {
        material = filledMaterial;
        UpdateMaterial();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        destroyMaterial();
    }

    Vector4 getRect()
    {
        if (!sprite)
        {
            return Vector4.zero;
        }
        var pivot = sprite.pivot;
        var rect = sprite.rect;
        return new Vector4
        {
            x = rect.x + pivot.x,
            y = rect.y + pivot.y,
            z = rect.width,
            w = rect.height,
        };
    }

    Material createMaterial()
    {
        return new Material(Shader.Find("Custom/UIFilled"));
    }

    void destroyMaterial()
    {
        if (filledMaterial)
        {
            DestroyImmediate(filledMaterial);
        }
    }
}
