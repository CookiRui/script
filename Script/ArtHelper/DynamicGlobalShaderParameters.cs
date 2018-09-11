using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class DynamicGlobalShaderParameters : MonoBehaviour {

    public Color skillColor = Color.white;

    // Use this for initialization
    private void Start () 
    {

        m_skillColorId = Shader.PropertyToID("SkillColor");

        LogicEvent.add("lerpSkillColor", this, "lerpSkillColor");
        LogicEvent.add("resetSkillColor", this, "resetSkillColor");
    }


    Color currentSkillColor = Color.white;

    private void OnPreRender() {
        Shader.SetGlobalColor(m_skillColorId, currentSkillColor);
    }

    float curLerpTime = 0.0f;
    float slerpTotalTime = 0.0f;
    void lerpSkillColor(float totalTime)
    {
        curLerpTime = totalTime;
        slerpTotalTime = totalTime;
    }

    float curLerpTime1 = 0.0f;
    float slerpTotalTime1 = 0.0f;
    void resetSkillColor(float totalTime)
    {
        if (currentSkillColor == Color.white)
            return;
        curLerpTime1 = totalTime;
        slerpTotalTime1 = totalTime;
    }

    void Update()
    {
        if (curLerpTime > 0.0f)
        {
            curLerpTime -= Time.unscaledDeltaTime;
            currentSkillColor = Color.Lerp(Color.white, skillColor, 1.0f - curLerpTime / slerpTotalTime);
        }
        if (curLerpTime1 > 0.0f)
        {
            curLerpTime1 -= Time.unscaledDeltaTime;
            currentSkillColor = Color.Lerp(skillColor, Color.white, 1.0f - curLerpTime1 / slerpTotalTime);
        }
    }


    int m_skillColorId;
}
