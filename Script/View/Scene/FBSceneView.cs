using FixMath.NET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cratos;

partial class FBSceneView : FBSceneViewBase
{
    bool enableRecordInput;

    Dictionary<uint,ushort> frameWithTime = new Dictionary<uint,ushort>();

    public override void onEnter()
    {
        base.onEnter();
        LogicEvent.add("onEnableRecordInput", this, "onEnableRecordInput");

#if UNITY_EDITOR
        LogicEvent.add("onDoorKeeperCatchingBallView", this, "onDoorKeeperCatchingBallView");
        LogicEvent.add("onMainCharacterPassingBall", this, "onMainCharacterPassingBall");
#endif
    }

    public override void onExit()
    {
        base.onExit();
        LogicEvent.remove(this);
    }

    public void setMainCharacterData(uint playerID, int teamID)
    {
        mainActorFrameID = playerID;
        mainActorTeam = (FBTeam)teamID;
    }

    public override void createWorld(uint mapID)
    {
        base.createWorld(mapID);
        /*
        //jlx 2017.03.29-log:测试用；
        gameEnv.AddComponent<DrawSceneWireframe>().set(
            new Vector3
            {
                x = (float)FBGame.instance.fbWorld.config.worldSize.x * 2,
                z = (float)FBGame.instance.fbWorld.config.worldSize.y * 2,
            },
            new Vector3
            {
                x = (float)FBGame.instance.fbWorld.config.doorSize.x * 2,
                y = (float)FBGame.instance.fbWorld.config.doorSize.y,
                z = (float)FBGame.instance.fbWorld.config.doorSize.z * 2,
            });
        */
    }

    private void Update()
    {
        if (!enableRecordInput)
            return;
        if (InputEventTranslator.instance== null) return;
        InputEventTranslator.instance.record();
    }

    void onEnableRecordInput(bool enable)
    {
        if (enableRecordInput == enable) return;
        enableRecordInput = enable;
        InputEventTranslator.instance.clear();
    }

    public override void ballAttach(uint id)
    {
        base.ballAttach(id);
        switchBtn(id);
    }

    public override void ballDetach(uint id)
    {
        base.ballDetach(id);
        switchBtn(id);
    }

    void switchBtn(uint id)
    {
        LogicEvent.fire2Lua("onSwitchBtn", id == mainActorFrameID, getActor(id).team == mainActorTeam);
    }

    protected override void onSceneLoaded()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Camera main is null");
            return;
        }
        Camera.main.transform.addComponent<CameraCtrl>();
    }

    protected override void onActorLoaded(ActorView actor, GameObject go)
    {
        if (actor == null || go == null) return;

        if (WithoutEnterShow_4Test_EditorOnly.instance!=null)
        {
            if (actor.team == mainActorTeam && !actor.gk)
            {
                var sr = actor.GetComponentInChildren<SkinnedMeshRenderer>();
                if (sr != null)
                {
                    //sr.enabled = false;
                }
            }
        }
        else
        {
            actor.setAniamtorController(true);
            actor.animator.SetBool("warmUp", true);
        }
    }

    //请求焦点
    public override void requestFocus(ActorView actor)
    {
        base.requestFocus(actor);

        SkinnedMeshRenderer smr = actor.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr != null)
        {
            smr.material.SetColor("_Switch", new Color(0.0f, 1.0f, 0.0f, 1.0f));
        }

        LogicEvent.fire("lerpSkillColor", 1.0f);
        LogicEvent.fire2Lua("onChangeNameBindPoint", actor.id, actor.getWaistPoint(), 0, actor.height);
    }

    //释放焦点
    public override void releaseFocus(ActorView actor)
    {
        base.releaseFocus(actor);

        SkinnedMeshRenderer smr = actor.GetComponent<SkinnedMeshRenderer>();
        if (smr != null)
        {
            smr.material.SetColor("_Switch", new Color(1.0f, 0.0f, 0.0f, 1.0f));
        }

        LogicEvent.fire("resetSkillColor", 1.0f);
        LogicEvent.fire2Lua("onChangeNameBindPoint", actor.id, actor.transform, -0.3f, actor.height * 1.5f);
    }

    public override bool canRecordThisFrame(RAL.LogicFrame frame)
    {
        for (int j = 0; j < frame.physicsFrames.Length; ++j)
        {
            for (int i = 0; i < frame.physicsFrames[j].actions.Length; ++i)
            {
                if (frame.physicsFrames[j].actions[i].typeID == RenderableActionID.UpdateMatchTimeAction)
                    return true;
            }
        }
        return false;
    }

    public override void recordFrameWithTime( uint frame, ushort time)
    {
        if (frameWithTime.ContainsKey(frame))
        {
            frameWithTime[frame] = time;
        }
        else
        {
            frameWithTime.Add(frame, time);
        }
    }

    public ushort getTimeByFrame(uint frame)
    {
        ushort time;
        if (frameWithTime.TryGetValue(frame, out time))
        {
            return time;
        }
        return 0;
    }

#if UNITY_EDITOR

    //
    public bool showCathingBallArea = false;

    public float showTargetAreaTime = 100.0f;

    Vector3 centerPosition;
    Quaternion direction;
    List<float> areaData = new List<float>();
    float showTime = 0.0f;

    private static Material m_targetAreaMaterial = null;

    public Vector3 hitCenter = new Vector3();

    void onDoorKeeperCatchingBallView(FBActor actor, BW31.SP2D.FixVector2 point, Fix64 height)
    {
        areaData = new List<float>();
        showTime = Time.fixedTime;
        centerPosition = new Vector3((float)actor.getPosition().x, (float)actor.height, (float)actor.getPosition().y);
        direction = Quaternion.LookRotation(new Vector3((float)actor.direction.x, 0, (float)actor.direction.y));
        for (int i = 0; i < actor.configuration.dkcb_edgeLimit.Length; ++i)
        {
            areaData.Add((float)actor.configuration.dkcb_edgeLimit[i]);
        }

        hitCenter.y = (float)height;
        hitCenter.x = (float)point.x;
        hitCenter.z = (float)point.y;
    }

    private void renderCatchingBallTargetArea()
    {
        if (!showCathingBallArea)
            return;
        if (areaData == null || areaData.Count == 0)
            return;
        if (Time.fixedTime > showTime + showTargetAreaTime)
        {
            areaData = null;
            return;
        }

        createMaterial();
        m_targetAreaMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.TRS(centerPosition, direction, Vector3.one));

        //0A 1B 2C 3D 4E 5F 6G
        //======
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.white);
        GL.Vertex3(-areaData[6], 0, 0);
        GL.Vertex3(areaData[6], 0, 0);
        GL.Vertex3(areaData[6], areaData[5], 0);
        GL.Vertex3(-areaData[6], areaData[5], 0);
        GL.Vertex3(-areaData[6], 0, 0);
        GL.End();

        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.green);
        GL.Vertex3(-areaData[0], 0, 0);
        GL.Vertex3(areaData[0], 0, 0);
        GL.Vertex3(areaData[0], areaData[1], 0);
        GL.Vertex3(-areaData[0], areaData[1], 0);
        GL.Vertex3(-areaData[0], 0, 0);
        GL.End();

        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.green);
        GL.Vertex3(-areaData[0], areaData[1], 0);
        GL.Vertex3(areaData[0], areaData[1], 0);
        GL.Vertex3(areaData[0], areaData[2], 0);
        GL.Vertex3(-areaData[0], areaData[2], 0);
        GL.Vertex3(-areaData[0], areaData[1], 0);
        GL.End();

        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.green);
        GL.Vertex3(-areaData[0], areaData[2], 0);
        GL.Vertex3(areaData[0], areaData[2], 0);
        GL.Vertex3(areaData[0], areaData[3], 0);
        GL.Vertex3(-areaData[0], areaData[3], 0);
        GL.Vertex3(-areaData[0], areaData[2], 0);
        GL.End();

        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.green);
        GL.Vertex3(-areaData[6], 0, 0);
        GL.Vertex3(-areaData[0], 0, 0);
        GL.Vertex3(-areaData[0], areaData[4], 0);
        GL.Vertex3(-areaData[6], areaData[4], 0);
        GL.Vertex3(-areaData[6], 0, 0);
        GL.End();

        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.green);
        GL.Vertex3(areaData[6], 0, 0);
        GL.Vertex3(areaData[0], 0, 0);
        GL.Vertex3(areaData[0], areaData[4], 0);
        GL.Vertex3(areaData[6], areaData[4], 0);
        GL.Vertex3(areaData[6], 0, 0);
        GL.End();

        GL.PopMatrix();

        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));

        GL.Begin(GL.QUADS);
        GL.Color(Color.red);
        GL.Vertex3(hitCenter.x - 0.051f, hitCenter.y - 0.051f, hitCenter.z - 0.051f);
        GL.Vertex3(hitCenter.x + 0.051f, hitCenter.y - 0.051f, hitCenter.z + 0.051f);
        GL.Vertex3(hitCenter.x + 0.051f, hitCenter.y + 0.051f, hitCenter.z - 0.051f);
        GL.Vertex3(hitCenter.x - 0.051f, hitCenter.y + 0.051f, hitCenter.z + 0.051f);
        GL.End();
        GL.PopMatrix();

    }
    private void OnRenderObject()
    {
        renderCatchingBallTargetArea();

        renderFindTargetArea();
    }

    void createMaterial()
    {
        if (m_targetAreaMaterial == null)
        {
            m_targetAreaMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            m_targetAreaMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_targetAreaMaterial.SetInt("_ZWrite", 0);
            m_targetAreaMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_targetAreaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_targetAreaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        }

    }


    //===================传球范围==================================================

    float minR;
    float maxR;
    float bestR;
    float angle;
    Vector3 centerPos;
    Quaternion shootBallDirection;
    List<Vector3> preferPoints = new List<Vector3>();
    Vector3 targetPosition = Vector3.zero;
    void onMainCharacterPassingBall(FBActor actor, BW31.SP2D.FixVector2 direction, int passType, FBActor target, List<FBActor> preferTargets)
    {
        showPassingBallTime = Time.fixedTime;
        minR = (float)actor.configuration.passBallMinR[passType];
        maxR = (float)actor.configuration.passBallMaxR[passType];
        bestR = (float)actor.configuration.passBallBestR[passType];
        angle = (float)actor.configuration.passBallFov[passType] * Mathf.Rad2Deg;
        shootBallDirection = Quaternion.LookRotation(new Vector3((float)direction.x, 0.0f, (float)direction.y));
        centerPos = new Vector3((float)actor.getPosition().x, 0.0f, (float)actor.getPosition().y);
        for (int i = 0; i < preferTargets.Count; ++i)
        {
            preferPoints.Add(new Vector3((float)preferTargets[i].getPosition().x, 0.0f, (float)preferTargets[i].getPosition().y));
        }

        if (target != null)
            targetPosition = new Vector3((float)target.getPosition().x, 0.0f, (float)target.getPosition().y);
        else
            targetPosition = Vector3.zero;

    }


    public bool showPassBallFindingTargetArea = true;
    public float showPassBallFindTargetAreaTime = 10.0f;
    float showPassingBallTime = float.MinValue;
    private static Material m_targetFindingAreaMaterial = null;
    private void renderFindTargetArea()
    {
        if (!showPassBallFindingTargetArea || showPassingBallTime < 0.0f)
            return;
        if (Time.fixedTime > showPassingBallTime + showPassBallFindTargetAreaTime)
        {
            preferPoints.Clear();
            targetPosition = Vector3.zero;
            return;
        }

        if (m_targetAreaMaterial == null)
        {
            m_targetAreaMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            m_targetAreaMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_targetAreaMaterial.SetInt("_ZWrite", 0);
            m_targetAreaMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_targetAreaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_targetAreaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        }

        m_targetAreaMaterial.SetPass(0);

        float diff = 0.5f;

        GL.PushMatrix();

        GL.MultMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));

        GL.Begin(GL.QUADS);
        //绘制潜在位置
        GL.Color(new Color(0, 0f, 0.5f, 0.6f));
        for (int i = 0; i < preferPoints.Count; ++i)
        {
            GL.Vertex3(preferPoints[i].x - diff, 2.0f, preferPoints[i].z - diff);
            GL.Vertex3(preferPoints[i].x + diff, 2.0f, preferPoints[i].z - diff);
            GL.Vertex3(preferPoints[i].x + diff, 2.0f, preferPoints[i].z + diff);
            GL.Vertex3(preferPoints[i].x - diff, 2.0f, preferPoints[i].z + diff);
        }
        //绘制目标位置
        if (targetPosition != Vector3.zero)
        {
            GL.Color(new Color(0, 0f, 1, 1f));
            GL.Vertex3(targetPosition.x - diff, 2.0f, targetPosition.z - diff);
            GL.Vertex3(targetPosition.x + diff, 2.0f, targetPosition.z - diff);
            GL.Vertex3(targetPosition.x + diff, 2.0f, targetPosition.z + diff);
            GL.Vertex3(targetPosition.x - diff, 2.0f, targetPosition.z + diff);

        }
        GL.End();

        GL.MultMatrix(Matrix4x4.TRS(centerPos, shootBallDirection, Vector3.one));

        //绘制BestR
        GL.Begin(GL.QUADS);
        GL.Color(new Color(1, 1, 1, 1f));
        var x = 0.0f;
        var y = bestR;
        GL.Vertex3(x - diff, 0, y - diff);
        GL.Vertex3(x + diff, 0, y - diff);
        GL.Vertex3(x + diff, 0, y + diff);
        GL.Vertex3(x - diff, 0, y + diff);
        GL.End();

        GL.Begin(GL.QUADS);
        GL.Color(new Color(1, 0, 0, 0.1f));
        float angleA = Mathf.PI * 0.5f - angle * Mathf.Deg2Rad;
        float angleB = Mathf.PI * 0.5f + angle * Mathf.Deg2Rad;

        float lastX1 = Mathf.Cos(angleA) * minR;
        float lastY1 = Mathf.Sin(angleA) * minR;
        float lastX2 = Mathf.Cos(angleA) * maxR;
        float lastY2 = Mathf.Sin(angleA) * maxR;

        int count = 60;
        for (int i = 1; i <= count; ++i)
        {
            var a = Mathf.Lerp(angleA, angleB, (float)i / count);
            var x1 = Mathf.Cos(a) * minR;
            var y1 = Mathf.Sin(a) * minR;
            var x2 = Mathf.Cos(a) * maxR;
            var y2 = Mathf.Sin(a) * maxR;

            GL.Vertex3(x1, 0, y1);
            GL.Vertex3(x2, 0, y2);

            GL.Vertex3(lastX2, 0, lastY2);
            GL.Vertex3(lastX1, 0, lastY1);

            lastX1 = x1;
            lastY1 = y1;
            lastX2 = x2;
            lastY2 = y2;
        }

        GL.End();

        GL.PopMatrix();

    }


#endif
};
