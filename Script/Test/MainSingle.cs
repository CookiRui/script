using Cratos;
using System;
using UnityEngine;

class MainSingle : MonoBehaviour
{
    void Start()
    {
        addComponent();
#if !SCENEEDITOR_TOOL
        LuaProxy.instance.startLuaVM(gameObject);
#endif
        //uint vvv2 = bs.getBit(0,4);
    }

    void addComponent()
    {
        gameObject.AddComponent<ResourceManager>();
    }

    void Update()
    {
        if (_frameSyncUpdateSimulator != null)
            _frameSyncUpdateSimulator.run();

        Game.instance.run();

        if (RealTimeRAProcessCenter.instance != null)
            RealTimeRAProcessCenter.instance.run(Time.deltaTime);
    }
    void Awake()
    {
        Debuger.SetOutputType(Debuger.OutputType.Console);
        Debuger.SetDebugLevel((int)(Debuger.DebugLevel.Normal | Debuger.DebugLevel.Warning | Debuger.DebugLevel.Error));
        Debuger.Open();

        Game.create();

        RealTimeRAProcessCenter.create();

        //NetworkUpdater.create();

        SceneViews.create();

#if !SCENEEDITOR_TOOL
        LuaProxy.create();
#endif

#if FRAME_RECORDING
        FrameRecording.create();
#endif

        LogicEvent.add("onWaitForSync", this, "onWaitForSync");

        //FBGame�ձ�����
        LogicEvent.add("onFBGameNewed", this, "onFBGameNewed");
        //FBGame��ɾ��
        LogicEvent.add("onFBGameDestroyed", this, "onFBGameDestroyed");

        //FBGame��Ϸ��ʼǰ���������
        LogicEvent.add("onFBGameEnter", this, "onFBGameEnter");
        //FBGame��Ϸ��ʽ��ʼ
        LogicEvent.add("onFBGameStart", this, "onFBGameStart");
        //FBGame��Ϸ����
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

        //NetworkUpdater.terminate();

        RealTimeRAProcessCenter.terminate();

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
        SinglePlayerUpdateSimulator simulator = createFrameSyncUpdateSimulator(Game.instance.fbGame.frameSync);
        Game.instance.fbGame.frameSyncUpdater = simulator;
        simulator.start();

        RealTimeRAProcessCenter.instance.start();

    }

    void onFBGameStart()
    {
    }

    void onFBGameOver()
    {
        RealTimeRAProcessCenter.instance.over();
    }

    void onWaitForSync()
    {
        Game.instance.fbGame.changeState(GameState.Ready);
    }
    SinglePlayerUpdateSimulator _frameSyncUpdateSimulator = null;
    SinglePlayerUpdateSimulator createFrameSyncUpdateSimulator(FrameSync frameSync)
    {
        if (_frameSyncUpdateSimulator != null)
        {
            _frameSyncUpdateSimulator = new SinglePlayerUpdateSimulator();
        }
        _frameSyncUpdateSimulator.init(frameSync);

        return _frameSyncUpdateSimulator;
    }
}