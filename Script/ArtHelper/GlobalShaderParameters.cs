using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class GlobalShaderParameters : MonoBehaviour {

    public Color globalColor = Color.white;
	public Color mapColor = Color.white;

    // Use this for initialization
    private void Start () {
        m_globalColorId = Shader.PropertyToID("GlobalColor");
		m_mapColorId = Shader.PropertyToID("MapColor");

    }

    private void OnPreRender() {
        Shader.SetGlobalColor(m_globalColorId, globalColor);
		Shader.SetGlobalColor(m_mapColorId, mapColor);


    }


    int m_globalColorId;
	int m_mapColorId;
}
