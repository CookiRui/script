using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Cratos;


abstract class FBSceneViewBase : SceneViewBase
{

    //动画的TimeScale
    protected float _animatorTimeScale = 1.0f;
    //渲染处理的加速的TimeScale
    protected float _actionProcessSpeedTimeScale = 1.0f;

    public float animatorTimeScale
    {
        get { return _animatorTimeScale; }
        set
        {
            if (_animatorTimeScale == value)
                return;
            _animatorTimeScale = value;
            invalidateTimeScale();
        }
    }
    public float actionProcessSpeedTimeScale
    {
        get { return _actionProcessSpeedTimeScale; }
        set
        {
            if (_actionProcessSpeedTimeScale == value)
                return;
            _actionProcessSpeedTimeScale = value;
            invalidateTimeScale();
        }
    }

    void invalidateTimeScale()
    {
        float timeScale = actionProcessSpeedTimeScale * animatorTimeScale;
        if (timeScale == Time.timeScale)
            return;
        Time.timeScale = timeScale;
    }

    protected GameObject gameEnv = null;

    protected Dictionary<uint, EntityView> sceneEntityList = new Dictionary<uint, EntityView>();

    public uint mainActorFrameID = 0;
    public FBTeam mainActorTeam = FBTeam.kNone;

    public Vector3 leftDoorPosition { get; set; }
    public Vector3 rightDoorPosition { get; set; }


    bool _recordingAnimatorState = false;
    public bool recordingAnimatorState
    {
        get { return _recordingAnimatorState; }
        set { _recordingAnimatorState = value; }
    }

    Dictionary<int, LogicFrameAnimatorRecord> totalFrameRecords = new Dictionary<int, LogicFrameAnimatorRecord>();

    public override void onEnter()
    {
        base.onEnter();
        LogicEvent.add("onTestDrawPosition", this, "onTestDrawPosition");
        LogicEvent.add("onSceneLoaded", this, "onSceneLoaded");
    }

    public override void onExit()
    {
        base.onExit();

        if (gameEnv != null)
        {
            GameObject.DestroyObject(gameEnv);
            gameEnv = null;
        }

        var e = sceneEntityList.GetEnumerator();
        while (e.MoveNext())
        {
            GameObject.DestroyObject(e.Current.Value.gameObject);
        }
        sceneEntityList.Clear();

        GameObject.DestroyObject(ball.gameObject);
        ball = null;
    }

    //创建环境
    public virtual void createWorld(uint mapID)
    {
        SceneResourceLoader.inst.unLoadSceneMap();
        //fix me
        SceneResourceLoader.inst.loadScene("m107", "m107_01");
        //SceneResourceLoader.inst.loadScene("m101", "m101_01");
    }

    //创建ball
    public virtual BallView createBall(uint id, string prefab, float radius)
    {
        GameObject ga = ModelResourceLoader.inst.loadModel(prefab);
        ga.transform.parent = this.transform;

        BallView se = ga.AddComponent<BallView>();
        se.id = id;
        se.setRadius(radius);
        se.onCreate();

        ball = se;

        return se;
    }

    public BallView ball = null;

    public virtual ActorView createActor(uint id,
                                        FBTeam team,
                                        string avatarName,
                                        Dictionary<string, string> avatarPart,
                                        float[] runAnimiationNormalSpeed,
                                        float height,
                                        bool gk,
                                        string name,
                                        FiveElements element,
                                        uint roleId)
    {
        ActorView se = null;
        GameObject ga = ModelResourceLoader.inst.createAvatar(avatarName, avatarPart, go => onActorLoaded(se, go));
        ga.transform.parent = this.transform;
        se = ga.AddComponent<ActorView>();
        se.id = id;
        se.testId = id;
        se.team = team;
        se.runAnimiationNormalSpeed = runAnimiationNormalSpeed;
        se.height = height;
        se.gk = gk;
        se.nickName = name;
        se.element = element;
        se.roleId = roleId;

        se.onCreate();
        sceneEntityList.Add(id, se);

        return se;
    }

    protected virtual void onActorLoaded(ActorView actor, GameObject go) { }

    public T getSceneEntity<T>(uint id)
        where T : EntityView
    {
        var actor = getActor(id);
        if (actor) return actor as T;

        if (ball != null && id == ball.id)
            return ball as T;

        return null;
    }

    public ActorView getActor(uint id)
    {
        if (!sceneEntityList.ContainsKey(id)) return null;
        var entity = sceneEntityList[id];
        if (entity.isNull()) return null;
        if (!(entity is ActorView)) return null;
        return entity as ActorView;
    }

    public ActorView getMainActor()
    {
        return getActor(mainActorFrameID);
    }

    public int totoalEntities()
    {
        return sceneEntityList.Count;
    }

    public void clearEntityStartSamplePosition()
    {
        sceneEntityList.forEach(a => a.Value.startSamplePosition = null);
    }

    public void restoreActorsAnimator(int logicFrameID)
    {

        LogicFrameAnimatorRecord record = null;
        if (!totalFrameRecords.TryGetValue(logicFrameID, out record))
        {
            Debuger.Log("No Actor animator Record????....." + logicFrameID);
            return;
        }

        ball.transform.position = record.ballPosition;


        Debuger.Log("restoreActorsAnimator " + logicFrameID + " ballPosition:" + record.ballPosition);

        record.recordActors.forEach(a =>
        {
            AnimatorRecord entityAnimatorRecord = a.Value;
            if (entityAnimatorRecord != null)
            {
                EntityView view = null;
                if (sceneEntityList.TryGetValue(a.Key, out view))
                {
                    view.restoreAnimatorRecord(entityAnimatorRecord);
                }
            }
        });

        //totalFrameRecords.Clear();


    }
    public void createRecord(int logicFrameID)
    {
        if (!recordingAnimatorState)
            return;

        //Debuger.Log("createRecord " + logicFrameID);

        LogicFrameAnimatorRecord record = new LogicFrameAnimatorRecord();
        foreach (var item in sceneEntityList)
        {
            AnimatorRecord entityAnimatorRecord = item.Value.createAnimatorRecord();
            if (entityAnimatorRecord != null)
            {
                record.recordActors.Add(item.Value.id, entityAnimatorRecord);
            }
        }
        record.ballPosition = ball.transform.position;
        totalFrameRecords.Add(logicFrameID, record);
    }


    public virtual void ballAttach(uint id)
    {
        var actor = getActor(id);
        if (actor != null)
        {
            if (actor.animator != null)
            {
                actor.animator.SetBool("keepingBall", true);
            }
            ball.attach(actor);
        }
        LogicEvent.fire2Lua("onBallAttached", id);
    }

    public virtual void ballDetach(uint id)
    {
        var actor = getActor(id);
        if (actor != null)
        {
            if (actor.animator != null)
            {
                actor.animator.SetBool("keepingBall", false);
            }
            ball.detach(actor);
        }
        LogicEvent.fire2Lua("onBallDetached", id);
    }

    public Vector3 getEnemyDoorPosition(FBTeam team)
    {
        return team == FBTeam.kBlue ? rightDoorPosition : leftDoorPosition;
    }

    void setDoorNet()
    {
        var cloths = FindObjectsOfType<Cloth>();
        if (cloths.isNullOrEmpty()) return;

        ClothSphereColliderPair[] cs2 = new ClothSphereColliderPair[] { new ClothSphereColliderPair(ball.sphereCollider) };
        cloths.forEach(a => { a.sphereColliders = cs2; });
    }

    private void OnDrawGizmos()
    {
        //drawTraceBallPosition();
        //testDrawPosition();
    }

    private void drawTraceBallPosition()
    {
        if (ball == null) return;
        var doorPosition = getEnemyDoorPosition(FBTeam.kBlue);
        var ballPosition = ball.transform.position;
        ballPosition.y = 0;

        var dir = ballPosition - doorPosition;
        var position = doorPosition + dir * 0.1f + dir.normalized * 1;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, 0.5f);
    }

    void testDrawPosition()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, distance);
    }

    Vector3 position;
    float distance;

    protected void onTestDrawPosition(BW31.SP2D.FixVector2 position, FixMath.NET.Fix64 distance)
    {
        this.position = new Vector3 { x = (float)position.x, z = (float)position.y };
        this.distance = Mathf.Max(0.5f, (float)distance);
        //Debug.Log(this.position, distance);
    }

    public void setActorsAnimatorController(bool performance)
    {
        if (sceneEntityList == null) return;
        sceneEntityList.forEach(a =>
        {
            var actor = a.Value as ActorView;
            if (actor != null)
            {
                actor.setAniamtorController(performance);
            }
        });
    }

    public List<uint> getActorIds()
    {
        return sceneEntityList.Keys.Where(a => a != ball.id).ToList();
    }

    //请求焦点
    public virtual void requestFocus(ActorView actor)
    {
        actor.playSkillEffect();
        LogicEvent.fire2Rendering("onBeginKillerSkill", actor);
    }

    //释放焦点
    public virtual void releaseFocus(ActorView actor)
    {
        LogicEvent.fire2Rendering("onEndKillerSkill", actor);
    }

    public void gameInit()
    {
        setDoorNet();
        if (!sceneEntityList.isNullOrEmpty())
        {
            sceneEntityList.forEach(a =>
            {
                var actor = a.Value as ActorView;
                if (actor == null) return;

                Color nameColor;
                if (actor.team == mainActorTeam)
                {
                    if (actor.id == mainActorFrameID)
                    {
                        actor.showHalo("FeetHalo_blue");
                    }
                    nameColor = new Color { r = 0, g = 97 / 255.0f, b = 255 / 255.0f, a = 1 };
                }
                else
                {
                    nameColor = new Color { r = 255 / 255.0f, g = 34 / 255.0f, b = 38 / 255.0f, a = 1 };
                }
                if (!actor.gk)
                {
                    LogicEvent.fire2Lua("onCreateActorName", actor.id, actor.transform, -0.3f, actor.height * 1.5f, actor.nickName, nameColor);
                }

            });
        }
    }

    protected virtual void onSceneLoaded() { }

    public virtual void recordFrameWithTime(uint frame, ushort time) { }

    public virtual bool canRecordThisFrame(RAL.LogicFrame frame) { return false; }
}
