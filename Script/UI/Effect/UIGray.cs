/*
    author:jlx
*/
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
public class UIGray : MonoBehaviour
{
    Material material;
    void OnEnable()
    {
        if (!material)
        {
            material = createMaterial();
        }
    }

    void OnDestroy()
    {
        destroyMaterial();
    }

    protected virtual void Update()
    {
        var graphic = GetComponent<Graphic>();
        if (graphic)
        {
            graphic.material = material;
        }
    }

    Material createMaterial()
    {
        return new Material(Shader.Find("Custom/UIGray"));
    }

    void destroyMaterial()
    {
        if (material)
        {
            DestroyImmediate(material);
        }
        var graphic = GetComponent<Graphic>();
        if (graphic)
        {
            graphic.material = null;
            graphic.SetMaterialDirty();
        }
    }

}
