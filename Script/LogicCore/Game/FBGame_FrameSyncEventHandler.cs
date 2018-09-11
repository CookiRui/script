using FixMath.NET;
using Cratos;

public partial class FBGame
{
    public void LogicStart()
    {
        LogicEvent.fire("onFBGameStart");
        fbWorld.world.resetFrame();
        aiWorld.reset();
        changeState(GameState.Ready);
    }

    public void LogicOver()
    {

    }

    public void update(float time)
    {
        setCurrentRenderActionList(m_logicBeforeRenderActions);
        generateRenderAction<RAL.None>();

        //TimeScale不用缩放
        runTimeScale(FrameSync.LOGIC_UPDATE_TIME);

        //状态机更新
        runLogic(FrameSync.LOGIC_UPDATE_TIME * logicTimeScale);

        //world更新暂停
        if (curStateType != GameState.Replay)
        {
            if (aiWorld != null)
            {
                aiWorld.workspace.FrameSinceStartup = m_logicFrameQueue.nextFrameId;
            }

            for (int i = 0; i < m_worldRenderActions.Length; ++i)
            {
                setCurrentRenderActionList(m_worldRenderActions[i]);

                Fix64 thisUpdateTime = FrameSync.PHYSICS_UPDATE_TIME * logicTimeScale;
                fbWorld.updateOneFrame(thisUpdateTime);
            }
            fbWorld.lateUpdate(FrameSync.LOGIC_UPDATE_TIME * logicTimeScale);
        }

        var physics = new RAL.PhysicsFrame[FrameSync.LOGIC_PHYSICS_MULTIPLY];
        physics[0].actions = new RAL.RenderAction[m_logicBeforeRenderActions.Count + m_worldRenderActions[0].Count];

        m_logicBeforeRenderActions.CopyTo(physics[0].actions);
        m_worldRenderActions[0].CopyTo(physics[0].actions, m_logicBeforeRenderActions.Count);
        m_logicBeforeRenderActions.Clear();
        m_worldRenderActions[0].Clear();
        //for (int i = 1; i < m_worldRenderActions.Length - 1; ++i)
        //{
        //    physics[i] = new RAL.PhysicsFrame()
        //    {
        //        actions = m_worldRenderActions[i].ToArray()
        //    };
        //    m_worldRenderActions[i].Clear();
        //}
        physics[1] = new RAL.PhysicsFrame() {
            actions = m_worldRenderActions[1].ToArray()
        };
        m_worldRenderActions[1].Clear();
        physics[2] = new RAL.PhysicsFrame() {
            actions = m_worldRenderActions[2].ToArray()
        };
        m_worldRenderActions[2].Clear();
        physics[3].actions = new RAL.RenderAction[m_logicAfterRenderActions.Count + m_worldRenderActions[3].Count];

        m_worldRenderActions[3].CopyTo(physics[3].actions);
        m_logicAfterRenderActions.CopyTo(physics[3].actions, m_worldRenderActions[3].Count);
        m_worldRenderActions[3].Clear();
        m_logicAfterRenderActions.Clear();

        m_logicFrameQueue.push(time, physics);
    }

    public void preUpdate()
    {
        postPlayerInputEvent();
    }

    //如果该帧有输入事件，那么提交至服务器
    void postPlayerInputEvent()
    {
#if !GAMECORE_SERVER
        var msgs = InputEventTranslator.instance.translateInputToEvent();
        if (msgs == null || msgs.Count == 0)
            return;
        for (int i = 0; i < msgs.Count; i++)
        {
            RoomSession.inst.send(msgs[i]);
        }
#endif
    }

    public void handleFrameInputEvent(ServerFrameInputEvent inputEvent)
    {
        if (!gamePlayers.ContainsKey(inputEvent.objectID))
        {
            Debuger.LogError("No FBPlayer:" + inputEvent.objectID);
            return;
        }
        gamePlayers[inputEvent.objectID].onServerFrameInputEvent(inputEvent);

    }



    /// <summary>
    /// 测试使用函数，后面删除
    /// </summary>
    public void onCheckBallState()
    {
        if (ConstTable.DebugStateAction == 2)
        {
            if (fbWorld.ball.owner != null)
            {
                //自己停止Defend
                fbWorld.ball.owner.stopDefend();

                FBTeam notHodlingBallTeam = (FBTeam)(3 - (int)fbWorld.ball.owner.team);
                FBActor actor = fbWorld.getActorByTeam(notHodlingBallTeam);
                if (actor != null && actor.checkMovingState())
                {
                    actor.startDefend(fbWorld.ball.owner);
                }

            }
            else
            {

            }
        }
    }
}