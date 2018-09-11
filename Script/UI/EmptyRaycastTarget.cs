using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/EmptyRaycastTarget", 201)]
public class EmptyRaycastTarget : MaskableGraphic
{

    protected EmptyRaycastTarget()
    {
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
    }
}

