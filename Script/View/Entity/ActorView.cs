using UnityEngine;
using System.Collections;

public enum ActorAnimatorState
{
    Idle = 0,                   //空闲
    Run,                    //跑步
    Other,                  //不是空闲且不是跑步的其他状态
    MovingToIdle,
}

public enum EffectType
{
    //地面
    OnGround,

    //球的位置
    BallPosition,

    //绑定点
    BindingPoint,
}

//角色绑定点列表
public struct BindPoint
{
    public static readonly string[] BindPoints = new string[] {
        "ballPoint",
        "feetHaloPoint"
    };
}

class ActorView : EntityView
{
    public uint testId;
    public float[] runAnimiationNormalSpeed;
    public FBTeam team;
    //readonly string rightFootPoint = "Main/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 R Thigh/Bip001 R Calf/Bip001 R Foot/Bip001 R Toe0/right foot";
    /// <summary>
    /// 胸（腰？）部蓄力点
    /// </summary>
    //readonly string chestPoint = "Main/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/chest";

    readonly string ballPoint = "skeleton/ball";
    readonly string feetHaloPoint = "FeetHalo";
    readonly string namePoint = "skeleton/Bip001";
    readonly string outLineShader = "MiMo FB/Actor LOD2 outline Test ";
    readonly string meshPath = "skeleton/000";

    public float height { get; set; }
    public bool gk { get; set; }
    public string nickName { get; set; }
    public FiveElements element { get; set; }

    //public ActorMovingAction ama = null;
    /// <summary>
    /// 射门蓄力特效
    /// </summary>
    //AnimatorStateRecorder recorder;
    GameObject runningEffect;
    readonly string normalStrikeEffect = "Normal_ShootStrike";
    readonly string superStrikeFireEffect = "Special_ShootStrike_Fire";
    readonly string superStrikeWaterEffect = "Special_ShootStrike_Water";
    string superStrikeEffect;

    private Animator _animator;

    public Animator animator
    {
        get
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>(true);
                if (_animator != null)
                {
                    _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }
            }

            return _animator;
        }
    }

 
    public uint roleId { get; set; }

    RuntimeAnimatorController[] animatorControllers;
    Transform ballPointTrans;
    Transform feetHaloPointTrans;
    Transform waistPointTrans;

    public override void onCreate()
    {
        //animator = GetComponentInChildren<Animator>(true);
        //recorder = animator.addComponent<AnimatorStateRecorder>();
        GameObject feetHaloObj = new GameObject(feetHaloPoint);
        feetHaloObj.transform.SetParent(transform, false);
        feetHaloObj.transform.localRotation = Quaternion.Euler(new Vector3(90.0f, 0, 0));

        ModelResourceLoader.inst.loadAnimatorControllers(name, a =>
        {
            if (a == null) return;
            animatorControllers = a.ToArray();
        });
        switch (element)
        {
            case FiveElements.Fire:
                superStrikeEffect = superStrikeFireEffect;
                break;
            case FiveElements.Water:
                superStrikeEffect = superStrikeWaterEffect;
                break;
        }
    }

    //jlx 2017.03.23-log:修复：跑动动画没有跟移动速度相关
    public void updateAnimtorSpeed(float speed, uint moveType)
    {
        if (moveType >= runAnimiationNormalSpeed.Length)
            return;
        if (runAnimiationNormalSpeed[moveType] <= 0) return;
        setAnimtorSpeed(speed / runAnimiationNormalSpeed[moveType]);
    }

    public bool isRunState()
    {
        if (animator == null) return false;
        return animator.GetInteger("state") == (int)ActorAnimatorState.Run;
    }

    public void setAnimtorSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
        }
    }

    public void resetAnimtorSpeed()
    {
        setAnimtorSpeed(1);
    }

    public void setAnimatorTrigger(string trigger)
    {
        resetAnimatorTrigger();
        this.animator.SetTrigger(trigger);
    }

    public void resetAnimatorTrigger()
    {
        for (int i = 0; i < this.animator.parameters.Length; ++i)
        {
            if (this.animator.parameters[i].type == AnimatorControllerParameterType.Trigger)
            {
                this.animator.ResetTrigger(this.animator.parameters[i].nameHash);
            }
        }
    }

    public Transform getBallPoint()
    {
        if (ballPointTrans == null)
        {
            ballPointTrans = getChild(ballPoint);
        }
        return ballPointTrans;
    }

    public Transform getFeetHaloPoint()
    {
        if (feetHaloPointTrans == null)
        {
            feetHaloPointTrans = getChild(feetHaloPoint);
        }
        return feetHaloPointTrans;
    }

    public Transform getWaistPoint()
    {
        if (waistPointTrans == null)
        {
            waistPointTrans = getChild(namePoint);
        }
        return waistPointTrans;
    }

    public void showRunEffect()
    {
        if (gk) return;
        if (runningEffect == null)
        {
            runningEffect = EffectResourceLoader.inst.playEffect("Smoke_Running_3D", transform);
        }
        else
        {
            var effect = runningEffect.GetComponent<ContinuousEffect>();
            if (effect != null)
            {
                effect.show();
            }
        }
    }

    public void hideRunEffect()
    {
        if (gk) return;

        if (runningEffect != null)
        {
            var effect = runningEffect.GetComponent<ContinuousEffect>();
            if (effect == null)
            {
                effect = runningEffect.AddComponent<ContinuousEffect>();
            }
            effect.hide(false);
        }
    }

    public void showHalo(string name)
    {
        if (name == null)
        {
            Debug.LogError("name is null");
            return;
        }
        EffectResourceLoader.inst.playEffect(name, getFeetHaloPoint());
    }

    public void shoot(ShootType type)
    {
        switch (type)
        {
            case ShootType.Normal:
                showStrikeEffect(normalStrikeEffect, EffectType.BallPosition,Vector3.zero);
                break;
            case ShootType.Power:
                break;
            case ShootType.Super:
                //showStrikeEffect(superStrikeEffect, EffectType.BallPosition);
                break;
            case ShootType.Killer:
                break;
        }
    }

    public void pass()
    {
        showStrikeEffect(normalStrikeEffect, EffectType.BallPosition,new Vector3 { y = 0.3f});
    }

    void showStrikeEffect(string name, EffectType type,Vector3 offset)
    {
        if (name == null)
        {
            Debug.LogError("name is null");
            return;
        }
        switch (type)
        {
            case EffectType.BallPosition:
                EffectResourceLoader.inst.playEffect(name, null, SceneViews.instance.getCurFBScene().ball.transform.position + offset, transform.eulerAngles, Vector3.one);
                break;
            case EffectType.BindingPoint:
                break;
            case EffectType.OnGround:
                EffectResourceLoader.inst.playEffect(name, null, transform.position, transform.eulerAngles, Vector3.one);
                break;
        }
    }

    public override Vector3 getCenterPosition()
    {
        return new Vector3 { x = transform.position.x, y = transform.position.y + height * 0.5f, z = transform.position.z };
    }

    public void setAniamtorController(bool performance)
    {
        if (animator == null) return;

        if (animatorControllers == null || animatorControllers.Length <= 1) return;
        var animatroController = animatorControllers[performance ? 1 : 0];
        if (animator.runtimeAnimatorController != animatroController)
        {
            animator.runtimeAnimatorController = animatroController;
            animator.parameters.forEach(a =>
            {
                switch (a.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(a.nameHash, false);
                        break;
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(a.nameHash, 0);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(a.nameHash, 0);
                        break;
                }
            });
        }
    }

    public void playSkillEffect()
    {
        EffectResourceLoader.inst.playEffect(name + "_skill01", transform);
    }

    public override AnimatorRecord createAnimatorRecord()
    {
        if (_animator == null)
            return null;

        AnimatorRecord newRecord = new AnimatorRecord();

        newRecord.stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        var length = _animator.parameters.Length;
        newRecord.parameters = new AnimatorParamParam[length];
        for (int i = 0; i < length; ++i)
        {
            var animParam = _animator.parameters[i];
            AnimatorParamParam param = new AnimatorParamParam();
            param.nameHash = animParam.nameHash;
            param.name = animParam.name;
            param.type = animParam.type;
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                param.boolValue = _animator.GetBool(param.nameHash);
            }
            else if (param.type == AnimatorControllerParameterType.Float)
            {
                param.floatValue = _animator.GetFloat(param.nameHash);
            }
            else if (param.type == AnimatorControllerParameterType.Int)
            {
                param.intValue = _animator.GetInteger(param.nameHash);
            }
            newRecord.parameters[i] = param;
        }
        newRecord.direction = this.transform.localRotation;
        newRecord.position = this.getPosition();
        newRecord.isHoldingBall = getBallPoint().childCount > 0;

        return newRecord;
    }

    public override void restoreAnimatorRecord(AnimatorRecord record)
    {
        for (int i = 0; i < record.parameters.Length; ++i)
        {
            AnimatorParamParam a = record.parameters[i];
            switch (a.type)
            {
                case AnimatorControllerParameterType.Bool:
                    _animator.SetBool(a.nameHash, a.boolValue);
                    break;
                case AnimatorControllerParameterType.Float:
                    _animator.SetFloat(a.nameHash, a.floatValue);
                    break;
                case AnimatorControllerParameterType.Int:
                    _animator.SetInteger(a.nameHash, a.intValue);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    _animator.ResetTrigger(a.name);
                    break;
            }
        }

        if (record.isHoldingBall)
        {
            SceneViews.instance.getCurFBScene().ballAttach(this.id);
        }
        this.setPosition(record.position);
        this.setRotation(record.direction);

    }


    public void addOutLine()
    {
        var renderer = transform.find<Renderer>(meshPath);
        if (renderer!=null)
        {
            renderer.material.shader = Shader.Find(outLineShader);
        }
    }
#if UNITY_EDITOR

    public float m_rangeIndicatorRadius = 0;

    [Range(0, 180)]
    public float m_rangeIndicatorFanAngle = 0;

    private static Material m_rangeIndicatorMaterial = null;

    private void OnRenderObject()
    {
        if (m_rangeIndicatorRadius <= 0)
        {
            return;
        }

        if (m_rangeIndicatorMaterial == null)
        {
            m_rangeIndicatorMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            m_rangeIndicatorMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_rangeIndicatorMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_rangeIndicatorMaterial.SetInt("_ZWrite", 0);
            m_rangeIndicatorMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_rangeIndicatorMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        }

        m_rangeIndicatorMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one));
        GL.Begin(GL.LINE_STRIP);

        GL.Color(Color.white);
        int count = 64;
        for (int i = 0; i <= count; ++i)
        {
            var a = Mathf.PI * 2 * i / count;
            var x = Mathf.Cos(a) * m_rangeIndicatorRadius;
            var y = Mathf.Sin(a) * m_rangeIndicatorRadius;
            GL.Vertex3(x, 0, y);
        }
        GL.End();

        GL.Begin(GL.TRIANGLES);
        GL.Color(new Color(1, 0, 0, 0.5f));
        float angleA = Mathf.PI * 0.5f - m_rangeIndicatorFanAngle * Mathf.Deg2Rad;
        float angleB = Mathf.PI * 0.5f + m_rangeIndicatorFanAngle * Mathf.Deg2Rad;
        float lastX = Mathf.Cos(angleA) * m_rangeIndicatorRadius;
        float lastY = Mathf.Sin(angleA) * m_rangeIndicatorRadius;

        for (int i = 1; i <= count; ++i)
        {
            var a = Mathf.Lerp(angleA, angleB, (float)i / count);
            var x = Mathf.Cos(a) * m_rangeIndicatorRadius;
            var y = Mathf.Sin(a) * m_rangeIndicatorRadius;
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(x, 0, y);
            GL.Vertex3(lastX, 0, lastY);
            lastX = x;
            lastY = y;
        }

        GL.End();

        GL.PopMatrix();
    }

#endif
}
