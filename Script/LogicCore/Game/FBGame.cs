using System.Collections.Generic;
using BW31.SP2D;
using FixMath.NET;
using System;

public partial class FBGame : ILogicUpdater, IFrameSyncEventHandler
{
    //踢球时主角自己的ID
    public uint mainActorID { get; private set; }
    public FBTeam mainActorTeam { get; private set; }

    public void destory()
    {
        frameSync.removeLogicUpdator(this);
        frameSync = null;

        LogicEvent.remove(this);

        destroyWorld();

        blueScore = 0;
        redScore = 0;
        curState = null;

        onFBGameDestroyed();
    }

    public FBWorld fbWorld { get; private set; }
    public Dictionary<uint, FBPlayer> gamePlayers = new Dictionary<uint, FBPlayer>();
    public Dictionary<uint, ulong> uids = new Dictionary<uint, ulong>();


    void destroyWorld()
    {
        var e = gamePlayers.GetEnumerator();
        while (e.MoveNext())
        {
            e.Current.Value.destroy();
        }
        gamePlayers.Clear();

        if (fbWorld != null)
        {
            fbWorld.clear();
            fbWorld = null;
        }

        aiWorld.destory();
        aiWorld = null;
    }

    public FrameSync frameSync = null;

    IFrameSyncUpdater _frameSyncUpdater = null;
    public IFrameSyncUpdater frameSyncUpdater
    {
        get { return _frameSyncUpdater; }
        set
        {
            if (_frameSyncUpdater != null)
                _frameSyncUpdater.destroy();

            _frameSyncUpdater = value;
        }
    }

    public void setupFBGame(ulong tableId, uint mapID)
    {
        LogicEvent.add("onChangeCampState", this, "onChangeCampState");

        frameSync = new FrameSync();
        frameSync.addLogicUpdator(this);
        frameSync.frameSyncEventHandler = this;
        
        this.tableId = tableId;

        fbWorld = new FBWorld(this);
        fbWorld.randomSeed = 100;

        var worldConfig = new FBWorld.Configuration();
        var btConfig = new FBWorld.BTConfiguration(worldConfig.worldSize);
        fbWorld.setup(worldConfig, btConfig);
        matchTimer = Fix64.Zero;
        ////创建球
        createBall();

        fbWorld.onGoal += goal;

        onFBGameCreated();
    }

    public AIWorld aiWorld = null;

    public void createCoach(string behaviacPath)
    {
        aiWorld = new AIWorld(behaviacPath);

        var btName = "Coach";
        //红方教练    
        fbWorld.createCoach(this, FBTeam.kRed, aiWorld.workspace, btName);
        //蓝方教练
        fbWorld.createCoach(this, FBTeam.kBlue, aiWorld.workspace, btName);

        //红方门将教练
        fbWorld.createGKCoach(this,FBTeam.kRed, aiWorld.workspace, btName);
        //蓝方门将教练
        fbWorld.createGKCoach(this,FBTeam.kBlue, aiWorld.workspace, btName);
    }

    //创建成员
    public FBPlayer createPlayer(ulong uid,
                                uint playerID,
                                uint baseID,
                                int teamID,
                                string name,
                                bool mainActor,
                                bool ai)
    {
        if (mainActor)
        {
            mainActorID = playerID;
            mainActorTeam = (FBTeam)teamID;
            onMainActorCreated(playerID, mainActorTeam);
        }
        //jlx 2017.07.05 log:测试用
        if (MainCharacter.instance != null)
        {
            if (!MainCharacter.instance.useAIPlayer)
            {
                if (ai && baseID != 6) return null;
            }
            if (!MainCharacter.instance.useAIGK)
            {
                if (baseID == 6) return null;
            }
        }
        FBPlayer fbPlayer = new FBPlayer(playerID, baseID, (FBTeam)teamID, name, mainActor, ai, fbWorld, aiWorld.workspace);
        if (ai)
        {
            fbPlayer.aiTakeOver(true);
        }
        fbPlayer.onCreated();
        gamePlayers.Add(playerID, fbPlayer);
        uids.Add(playerID, uid);
        return fbPlayer;
    }

    void createBall()
    {
        fbWorld.createBall(ConstTable.ballID, FixVector3.kZero, new FBBall.Configuration());
    }

    public void debugLogicStart()
    {
        fbWorld.debugLogicStart();
    }
    public void debugFrameLogic()
    {
        //debug state
        Debuger.LogLogicNewLine();
        Debuger.LogLogic(string.Format("FrameID:{0} Score:{1}-{2}", frameSync.currentLogicFrameNum, blueScore, redScore));

        fbWorld.debugFrameLogic();
    }
    public void resetPosition()
    {
        fbWorld.ball.forceCheck = true;
        fbWorld.ball.setSampleType(FBBall.SampleType.IgnorePositionSampleSlerp);
        fbWorld.ball.reset(FixVector3.kZero);
        repositionActor();
    }
    /// <summary>
    /// 重置球员位置
    /// </summary>
    void repositionActor()
    {
        var actorCount = gamePlayers.Count;
        var redIndex = 0;
        var blueIndex = 0;

        foreach (var gamePlayer in gamePlayers)
        {
            var actor = gamePlayer.Value.actor;
            var position = FixVector3.kZero;
            var oritation = FixVector2.kZero;
            var camp = getCampType(actor.team);
            switch (actor.team)
            {
                case FBTeam.kRed:
                    if (actor.isDoorKeeper())
                    {
                        position = fbWorld.config.redAttackPositions[4];
                    }
                    else
                    {
                        position = camp == CampType.Attack ? fbWorld.config.redAttackPositions[redIndex] : fbWorld.config.redDefensePositions[redIndex];
                        redIndex++;
                    }
                    oritation = new FixVector2 { x = (Fix64)(-1) };
                    break;
                case FBTeam.kBlue:
                    if (actor.isDoorKeeper())
                    {
                        position = fbWorld.config.blueAttackPositions[4];
                    }
                    else
                    {
                        position = camp == CampType.Attack ? fbWorld.config.blueAttackPositions[blueIndex] : fbWorld.config.blueDefensePositions[blueIndex];
                        blueIndex++;
                    }
                    oritation = new FixVector2 { x = (Fix64)1 };
                    break;
            }

            actor.reset(position);
            actor.direction = oritation;
        }
    }

    public void playersAITakeOver(bool value)
    {
        foreach (var item in gamePlayers)
        {
            var player = item.Value;
            if (!player.ai)
            {
                player.aiTakeOver(value);
            }
        }
    }

    public void resetPlayerState()
    {
        foreach (var item in gamePlayers)
        {
            var player = item.Value;
            player.actor.stop();
            player.actor.doIdle();
        }
    }

    void onChangeCampState(FBTeam team)
    {
        switch (team)
        {
            case FBTeam.kBlue:
                blueCamp = CampType.Attack;
                redCamp = CampType.Defence;
                break;
            case FBTeam.kRed:
                blueCamp = CampType.Defence;
                redCamp = CampType.Attack;
                break;
        }
    }

    public ulong getPlayerUid(uint id)
    {
        if (id <= 0)
        {
            Debuger.LogError("id <= 0 " + id);
            return 0;
        }
        foreach (var item in uids)
        {
            if (item.Key == id) return item.Value;
        }
        return 0;
    }


    public FBGame() {

        m_logicFrameQueue = new RAL.LogicFrameQueue();

        m_worldRenderActions = new List<RAL.RenderAction>[FrameSync.LOGIC_PHYSICS_MULTIPLY];
        for (int i = 0; i < FrameSync.LOGIC_PHYSICS_MULTIPLY; ++i) {
            m_worldRenderActions[i] = new List<RAL.RenderAction>();
        }
    }

    private Fix64 _LogicTimeScale = Fix64.One;

    public Fix64 logicTimeScale
    {
        get { return _LogicTimeScale; }
        set 
        {
            if (_LogicTimeScale == value)
                return;
            _LogicTimeScale = value;
            generateRenderAction<RAL.AnimatorScaleAction>((float)_LogicTimeScale);
        }
    }

    //
    bool lerpToTimeScaleFlag = false;
    Fix64 lerpNeedTime = Fix64.Zero;
    Fix64 destTimeScale = Fix64.Zero;
    Fix64 timeScaleLerpSpeed = Fix64.Zero;
    Fix64 timeCounting = Fix64.Zero;

    //缓慢设置到特定的TimeScale
    public void lerpToTimeScale(Fix64 destTimeScale, Fix64 needTime)
    {
        waitingFlag = false;
        lerpToTimeScaleFlag = true;
        lerpNeedTime = needTime;
        this.destTimeScale = destTimeScale;
        timeCounting = Fix64.Zero;
        timeScaleLerpSpeed = (destTimeScale - this.logicTimeScale) / needTime;
    }

    bool waitingFlag = false;
    Fix64 waitCounting = Fix64.Zero;
    Fix64 scaleLastingTime = Fix64.Zero;
    Fix64 recoverNeedTime = Fix64.Zero;
    Action onBeginRecover;
    Action onRecoverComplete;
    //瞬间变化到destTimeScale，并且持续一定的时间，然后按照recoverNeedTime进行恢复
    public void lerpToTimeScale(Fix64 destTimeScale, 
                                Fix64 lastingTime,
                                Fix64 recoverNeedTime,
                                Action onBegin = null,
                                Action onBeginRecover = null, 
                                Action onRecoverComplete = null)
    {
        this.waitCounting = Fix64.Zero;
        this.logicTimeScale = destTimeScale;

        this.waitingFlag = true;
        this.scaleLastingTime = lastingTime;
        this.recoverNeedTime = recoverNeedTime;
      
        if (onBegin != null)
        {
            onBegin();
        }
        this.onBeginRecover = onBeginRecover;
        this.onRecoverComplete = onRecoverComplete;
    }

    void runTimeScale(Fix64 deltaTime)
    {
        if (lerpToTimeScaleFlag)
        {
            timeCounting += deltaTime;
            if (timeCounting < lerpNeedTime)
                this.logicTimeScale += timeScaleLerpSpeed * deltaTime;
            else
            {
                this.logicTimeScale = destTimeScale;
                lerpToTimeScaleFlag = false;
                if (onRecoverComplete != null)
                {
                    onRecoverComplete();
                    onRecoverComplete = null;
                }
            }
        }

        if (waitingFlag)
        {
            waitCounting += deltaTime;
            if (waitCounting > this.scaleLastingTime)
            {
                lerpToTimeScale(Fix64.One, this.recoverNeedTime);
                waitingFlag = false;
                if (onBeginRecover != null)
                {
                    onBeginRecover();
                    onBeginRecover = null;
                }
            }
        }

    }

    void runLogic(Fix64 deltaTime)
    {
        if (curState == null) return;
        curState.execute(deltaTime);
    }
}