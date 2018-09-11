using Cratos;
using System;
using System.Collections;
using UnityEngine;

class Main : MonoBehaviour
{
    public bool useReporter;
    void Start()
    {
        addComponent();
#if !SCENEEDITOR_TOOL
        LuaProxy.instance.startLuaVM(gameObject);
#endif
        //uint vvv2 = bs.getBit(0,4);
        if (Application.isEditor || !useReporter)
        {
            var t = transform.Find("Reporter");
            if (t != null)
            {
                Destroy(t.gameObject);
            }
        }
    }

    void addComponent()
    {
        gameObject.AddComponent<Zeus>();
        gameObject.AddComponent<ResourceManager>();
        gameObject.AddComponent<MsgHandle>();
    }

    void Update()
    {
        if (_frameSyncUpdateSimulator != null)
        {
            _frameSyncUpdateSimulator.run();
        }

        if (Game.instance != null)
        {
            Game.instance.run();
            RealTimeRAProcessCenter.instance.run(Time.deltaTime);

        }
    }

    void Awake()
    {
        Debuger.SetOutputType(Debuger.OutputType.Console);
        Debuger.SetDebugLevel((int)(Debuger.DebugLevel.Normal | Debuger.DebugLevel.Warning | Debuger.DebugLevel.Error));
        Debuger.Open();

        Game.create();

        RealTimeRAProcessCenter.create();

        SceneViews.create();

        Profiler.create();

#if !SCENEEDITOR_TOOL
        LuaProxy.create();
#endif

#if FRAME_RECORDING
        FrameRecording.create();
#endif

        LogicEvent.add("onWaitForSync", this, "onWaitForSync");

        //FBGame刚被创建
        LogicEvent.add("onFBGameNewed", this, "onFBGameNewed");
        //FBGame被删除
        LogicEvent.add("onFBGameDestroyed", this, "onFBGameDestroyed");

        //FBGame游戏开始前，比如进场
        LogicEvent.add("onFBGameEnter", this, "onFBGameEnter");
        //FBGame游戏正式开始
        LogicEvent.add("onFBGameStart", this, "onFBGameStart");
        //FBGame游戏结束
        LogicEvent.add("onFBGameOver", this, "onFBGameOver");



    }

    void OnDestroy()
    {
        LogicEvent.remove(this);

#if FRAME_RECORDING
        FrameRecording.terminate();
#endif

#if !SCENEEDITOR_TOOL
        LuaProxy.terminate();
#endif

        SceneViews.terminate();

        RealTimeRAProcessCenter.terminate();

        Profiler.terminate();

        Game.terminate();

        Debuger.Close();
    }


    void onFBGameNewed()
    {

        RealTimeRAProcessCenter.instance.GetType().GetField("fbGame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance).SetValue(RealTimeRAProcessCenter.instance, Game.instance.fbGame);
     
#if FRAME_RECORDING
        FrameRecording.instance.GetType().GetField("fbGame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance).SetValue(FrameRecording.instance, Game.instance.fbGame);
#endif
    }
    void onFBGameDestroyed()
    {
        if (RealTimeRAProcessCenter.instance != null)
        {
            RealTimeRAProcessCenter.instance.GetType().GetField("fbGame",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance).SetValue(RealTimeRAProcessCenter.instance, null);
        }

        Time.timeScale = 1.0f;

        RealTimeRAProcessCenter.instance.over();
    }

    void onFBGameEnter()
    {
        FrameSyncUpdateSimulator simulator = createFrameSyncUpdateSimulator(Game.instance.fbGame.frameSync);
        Game.instance.fbGame.frameSyncUpdater = simulator;
        simulator.start();

        RealTimeRAProcessCenter.instance.start();

    }

    void onFBGameStart()
    {
        FrameSyncUpdater syncUpdater = new FrameSyncUpdater(Game.instance.fbGame.frameSync);
        Game.instance.fbGame.frameSyncUpdater = syncUpdater;
        syncUpdater.start();

        RealTimeRAProcessCenter.instance.reset();

        Profiler.instance.startPing();
    }

    void onFBGameOver()
    {
        Profiler.instance.stopPing();

        //??保存录像
        //RealTimeRAProcessCenter.instance.saveReplay();

        //创建帧同步更新模拟器
        FrameSyncUpdateSimulator simulator = createFrameSyncUpdateSimulator(Game.instance.fbGame.frameSync);

        Game.instance.fbGame.frameSyncUpdater = simulator;

        simulator.start();

    }

    void onWaitForSync()
    {
        //Debug.Log("**********onWaitForSync");
        Zeus.inst.sendToGateway(new TableClientReadyReq());
    }

    FrameSyncUpdateSimulator _frameSyncUpdateSimulator = null;
    FrameSyncUpdateSimulator createFrameSyncUpdateSimulator( FrameSync frameSync )
    {
        if( _frameSyncUpdateSimulator == null)
            _frameSyncUpdateSimulator = new FrameSyncUpdateSimulator();
        _frameSyncUpdateSimulator.init(frameSync);
        return _frameSyncUpdateSimulator;
    }

}