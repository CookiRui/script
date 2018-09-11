using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FaceAnim : MonoBehaviour {

    [HideInInspector]
    public Vector4 scaleOffset;

    private void Start() {
        renderer = GetComponent<Renderer>();
        shaderPropertyId = Shader.PropertyToID("_FaceTex_ST");
    }

    private void OnWillRenderObject() {
        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetVector(shaderPropertyId, scaleOffset);
        renderer.SetPropertyBlock(block);
    }

    private new Renderer renderer;
    private int shaderPropertyId;
}
