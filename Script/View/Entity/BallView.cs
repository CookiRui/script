using UnityEngine;
using System.Collections;

class BallView : EntityView
{
    SphereCollider _sphereCollider;
    public SphereCollider sphereCollider
    {
        get
        {
            if (_sphereCollider == null)
            {
                _sphereCollider = GetComponentInChildren<SphereCollider>();
            }

            return _sphereCollider;
        }
    }

    Transform _ball;
    Transform ball
    {
        get
        {
            if (_ball == null)
            {
                int count = transform.childCount;
                if (count > 0)
                {
                    Transform t = transform.GetChild(0);
                    if (t != null && t.childCount>0)
                    {
                        _ball = t.GetChild(0);
                    }
                }
            }

            return _ball;
        }
    }

    BallRotator _ballRot;
    BallRotator rotator
    {
        get
        {
            if (_ballRot == null && ball != null)
            {
                _ballRot = ball.addComponent<BallRotator>();
                if (_ballRot != null)
                {
                    _ballRot.parabolaK = config.parabolaK;
                    _ballRot.arclineK = config.arclineK;
                    _ballRot.radius = radius;
                }
            }

            return _ballRot;
        }
    }
    public ActorView owner { get; private set; }
    public ActorView kicker { get; private set; }

    /// <summary>
    /// 拖尾特效
    /// </summary>
    GameObject trailEffect;
    float showTimer;
    GameObject landedEffect;
    ShootType? shootType;
    Vector3 lastPosition;
    BallConfig config;
    GameObject energyEffect;
    float radius;
    GameObject chargeEffect;
    Coroutine showChargeEffectCoroutine;

    private void Awake()
    {
        config = new BallConfig(ConfigResourceLoader.inst.loadConfig("Config/ball.xml").ToXml());
        //rotator = ball.addComponent<BallRotator>();
        //rotator.parabolaK = config.parabolaK;
        //rotator.arclineK = config.arclineK;
#if UNITY_EDITOR
        instance = this;
        logicPosition = new System.Collections.Generic.LinkedList<Vector3?>();
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (ReferenceEquals(instance, this))
        {
            instance = null;
        }
#endif
    }

    //public BallAttachAction ba = null;
    public void attach(EntityView parent)
    {
        owner = parent as ActorView;
        var childnode = owner.getBallPoint();
        transform.parent = childnode;

        lastPosition = transform.position;
        hideTrailEffect();
        //jlx2017.05.09-log:捡球的时候需要把 startSamplePosition 置为空
        startSamplePosition = null;
        shootType = null;
        changeColliderSize(0);
        if (rotator != null)
        {
            rotator.clear();
        }

        this.transform.localPosition = Vector3.zero;
        positionDirty = false;
    }

    public void detach(EntityView dummyEntity)
    {
        transform.parent = SceneViews.instance.current.transform;
        base.setRotation(Quaternion.identity);

        kicker = dummyEntity as ActorView;
        owner = null;

        startSamplePosition = transform.position;
        hideChargeEffect();
    }

    //经过固定时间从当前位置差值到目标位置
    public void slerp(Vector3 end, float totalSlerpTime)
    {
        startSamplePosition = null;
        startMovePosition = transform.position;
        targetMovePosition = end;
        slerpTimeElapsed = 0.0f;
        this.totalSlerpTime = totalSlerpTime;
    }

    Vector3 startMovePosition;
    Vector3 targetMovePosition;
    float slerpTimeElapsed = 0.0f;
    float totalSlerpTime = 0.0f;
    void updateSlerp(float time)
    {
        if (slerpTimeElapsed >= totalSlerpTime)
            return;

        Vector3 s = targetMovePosition - startMovePosition;
        float l = s.magnitude;

        Vector3 thisPosition = startMovePosition + (l * slerpTimeElapsed / totalSlerpTime) * s.normalized;

        base.setPosition(thisPosition);

        //Debuger.Log("UpdateSlerp Position:" + thisPosition);
        slerpTimeElapsed = slerpTimeElapsed + time;
    }

    //球不带旋转
    public override void setRotation(Quaternion rotation) { }

    public override void lookAt(Vector3 dest) { }

    public override void setPosition(Vector3 dest)
    {
        if (owner != null)
            return;
        //Debuger.Log("setPosition Position:" + dest);
        targetMovePosition = dest;
        base.setPosition(dest);
    }

    protected override void Update()
    {
        updateSlerp(Time.deltaTime);

        base.Update();
        if (owner == null)
        {
            if (transform.position == lastPosition)
            {
                //jlx2017.05.09-log:如果球滚动停止，删除拖尾特效
                if (trailEffect != null)
                {
                    showTimer += Time.deltaTime;
                    if (showTimer > 0.1f)
                    {
                        hideTrailEffect();
                    }
                }
            }
            else
            {
                if (rotator != null)
                {
                    rotator.rotate(lastPosition, transform.position);
                }

                lastPosition = transform.position;
            }
        }
        else
        {
            if (owner.animator != null && rotator != null)
            {
                var stateInfo = owner.animator.GetCurrentAnimatorStateInfo(0);
                if (isRotateByAnimation(stateInfo, owner.roleId))
                {
                    rotator.clear();
                }
                else
                {
                    rotator.roll();
                }
            }
            if (transform.position != lastPosition)
            {
                if (rotator != null)
                {
                    rotator.rotate(lastPosition, transform.position);
                }
                if (trailEffect != null)
                {
                    trailEffect.transform.forward = (transform.position - lastPosition).normalized;
                }
                lastPosition = transform.position;
            }
        }
    }

    bool isRotateByAnimation(AnimatorStateInfo info, uint id)
    {
        if (config == null) return false;
        if (isRotateByAnimation(info, config.commonAnimations)) return true;

        string[] actorAnimations;
        if (config.actorAnimations.TryGetValue(id, out actorAnimations))
        {
            if (isRotateByAnimation(info, actorAnimations)) return true;
        }

        return false;
    }

    bool isRotateByAnimation(AnimatorStateInfo info, string[] animations)
    {
        if (animations == null || animations.Length == 0) return false;

        for (int i = 0; i < animations.Length; i++)
        {
            if (info.IsName(animations[i])) return true;
        }
        return false;
    }

    #region effect
    /// <summary>
    /// 显示拖尾特效
    /// </summary>
    public void showTrailEffect(string name)
    {
        if (trailEffect != null)
        {
            Destroy(trailEffect);
        }
        if (name == null)
        {
            Debug.LogError("name is null");
            return;
        }
        trailEffect = EffectResourceLoader.inst.playEffect(name, transform);
        if (trailEffect != null)
        {
            trailEffect.SetActive(false);
            StartCoroutine(delayShowTrailEffect());
        }

        showTimer = 0;
    }

    IEnumerator delayShowTrailEffect()
    {
        yield return null;
        if (trailEffect != null)
        {
            trailEffect.SetActive(true);
        }
    }

    void showHitEffect(string name, Vector3 position, Vector3 forward)
    {
        if (name == null)
        {
            Debug.LogError("name is null");
            return;
        }

        var effect = EffectResourceLoader.inst.playEffect(name, null, position, Vector3.zero, Vector3.one);
        if (effect != null)
        {
            effect.transform.forward = forward;
        }
    }

    public void showLandedEffect(string name, Vector2 position)
    {
        if (name == null)
        {
            Debug.LogError("name is null");
            return;
        }
        landedEffect = EffectResourceLoader.inst.playEffect(name, null, new Vector3 { x = position.x, z = position.y }, Vector3.zero, Vector3.one);
    }

    public void showCollidedWallEffect(string name, Vector3 poisition, Vector3 angles)
    {
        if (name == null)
        {
            Debug.LogError("name is null");
            return;
        }
        EffectResourceLoader.inst.playEffect(name, null, poisition, angles, Vector3.one);
    }

    public void hideTrailEffect()
    {
        if (trailEffect == null) return;

        var continuousEffect = trailEffect.GetComponent<ContinuousEffect>();
        if (continuousEffect == null)
        {
            continuousEffect = trailEffect.AddComponent<ContinuousEffect>();
        }
        continuousEffect.hide(true);
        trailEffect = null;
    }

    public void destroyLandedEffect()
    {
        if (!landedEffect) return;
        Destroy(landedEffect);
    }

    #endregion

    #region public methods

    public void shoot(ShootType type,
                        Vector3 velocity,
                        float angle,
                        Vector3 target,
                        ActorView shooter)
    {
        shootType = type;

        switch (type)
        {
            case ShootType.Normal:
                showTrailEffect(config.normalTrailEffect);
                setRotateType(velocity);
                break;
            case ShootType.Power:
                showTrailEffect(config.powerTrailEffect);
                setRotateType(velocity);
                break;
            case ShootType.Super:
                showTrailEffect(config.getSuperTrailEffect(shooter.element));
                rotator.arcline(angle);
                break;
            case ShootType.Killer:
                showTrailEffect(shooter.name + "_" + config.killerTrailEffect);

                var dir = target - transform.position;
                var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(dir), Vector3.one);

                var killerSkillRotate = config.getKillerSkillRotate(shooter.roleId);
                var axis = matrix.MultiplyVector(killerSkillRotate.axis).normalized;
                rotator.killerSkill(axis, killerSkillRotate.angularVelocity);
                break;
        }
    }

    public void hitLand(Vector3 position, float preHeightVelocity)
    {
        if (preHeightVelocity < -config.minLandHeightVelocity)
        {
            showHitEffect(config.passHitLandEffect, position, Vector3.forward);
        }
        changeColliderSize(0);

    }

    public void hitWall(Vector3 position, Vector3 normal, FiveElements element)
    {
        if (shootType.HasValue)
        {
            hideTrailEffect();
            shootType = null;
        }
        changeColliderSize(0);
        showHitEffect(config.passHitWallEffect, position, normal);
    }

    public void hitNet(Vector3 position, Vector3 collidedVelocity, FiveElements element)
    {
        if (shootType.HasValue)
        {
            hideTrailEffect();
            switch (shootType.Value)
            {
                case ShootType.Normal:
                case ShootType.Power:
                    showHitEffect(config.normalHitNetEffect, position, collidedVelocity);
                    break;
                case ShootType.Super:
                    showHitEffect(config.getHitNetEffect(element), position, collidedVelocity);
                    break;
                case ShootType.Killer:
                    showHitEffect(config.normalHitNetEffect, position, collidedVelocity);
                    break;
            }
            shootType = null;
        }
        changeColliderSize(1);
    }

    public void changeColliderSize(float size)
    { 
        if (sphereCollider == null)return;
        if (sphereCollider.radius == size) return;
        sphereCollider.radius = size;
    }

    public void setRadius(float r)
    {
        radius = r;
    }

    public void setRotateType(Vector3 velocity)
    {
        if (rotator == null) return;
        //Debug.LogError("setRotateType velocity.y :" + velocity.y);
        if (velocity.y == 0)
        {
            rotator.roll();
        }
        else
        {
            rotator.parabola(velocity);
        }
    }

    public void energyLevelChanged(byte oldLevel, byte newLevel)
    {
        if (oldLevel == newLevel) return;
        hideEnergyEffect();
        var effectName = config.getEnergyEffect(newLevel);
        if (string.IsNullOrEmpty(effectName)) return;

        energyEffect = EffectResourceLoader.inst.playEffect(effectName, transform);
    }

    public void hideEnergyEffect()
    {
        if (energyEffect != null)
        {
            Destroy(energyEffect);
        }
    }

    /// <summary>
    /// 显示射门蓄力特效
    /// </summary>
    public void showChargeEffect()
    {
        if (showChargeEffectCoroutine != null)
        {
            StopCoroutine(showChargeEffectCoroutine);
        }
        if (chargeEffect != null)
        {
            Destroy(chargeEffect);
        }

        showChargeEffectCoroutine = StartCoroutine(delayShowChargeEffect());
    }

    IEnumerator delayShowChargeEffect()
    {
        yield return new WaitForSeconds(config.chargeDelay);
        chargeEffect = EffectResourceLoader.inst.playEffect(config.chargeEffect, transform);
    }

    /// <summary>
    /// 隐藏射门蓄力特效
    /// </summary>
    public void hideChargeEffect()
    {
        if (showChargeEffectCoroutine != null)
        {
            StopCoroutine(showChargeEffectCoroutine);
        }

        if (chargeEffect == null) return;

        chargeEffect.AddComponent<ContinuousEffect>().hide(true);
    }

    public void pass()
    {
        showTrailEffect(config.passTrailEffect);
    }

    #endregion

#if UNITY_EDITOR
    public static BallView instance { get; private set; }
    public System.Collections.Generic.LinkedList<Vector3?> logicPosition { get; set; }

    public bool drawLogicPosition = false;

    private static Material ms_logicPositionMaterial = null;

    private void OnRenderObject()
    {
        if (!drawLogicPosition || Camera.current != Camera.main)
        {
            return;
        }
        if (ms_logicPositionMaterial == null)
        {
            ms_logicPositionMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            ms_logicPositionMaterial.hideFlags = HideFlags.HideAndDontSave;
            ms_logicPositionMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            ms_logicPositionMaterial.SetInt("_ZWrite", 0);
            ms_logicPositionMaterial.SetInt("ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            ms_logicPositionMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ms_logicPositionMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }

        ms_logicPositionMaterial.SetPass(0);

        GL.PushMatrix();

        foreach (Vector3? pt in logicPosition)
        {
            if (pt == null)
            {
                continue;
            }
            GL.MultMatrix(Matrix4x4.Translate((Vector3)pt));
            GL.Begin(GL.LINES);

            GL.Color(Color.red);
            GL.Vertex3(-0.25f, 0, 0);
            GL.Vertex3(0.25f, 0, 0);

            GL.Color(Color.green);
            GL.Vertex3(0, -0.25f, 0);
            GL.Vertex3(0, 0.25f, 0);

            GL.Color(Color.blue);
            GL.Vertex3(0, 0, -0.25f);
            GL.Vertex3(0, 0, 0.25f);

            GL.End();
        }
        GL.PopMatrix();
    }
#endif
}
