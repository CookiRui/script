using UnityEngine;
using System.Collections;
using Cratos;
using FixMath.NET;
using BW31.SP2D;
using System.Collections.Generic;

class SinglePlayerUpdateSimulator : FrameSyncUpdateSimulator
{
    List<ServerFrameInputEvent> events = new List<ServerFrameInputEvent>();

    protected override ServerFrameMsg createServerFrameMsg()
    {
        ServerFrameMsg serverEvent = new ServerFrameMsg();
        serverEvent.frameNum = serverCurrentFrameNum;

        events.Clear();
        List<ClientFrameMsg> clientMsgs = InputEventTranslator.instance.translateInputToEvent();
        if (clientMsgs != null)
        {
            for (int i = 0; i < clientMsgs.Count; ++i)
            {
                ServerFrameInputEvent sie = new ServerFrameInputEvent();
                sie.angle = clientMsgs[i].angle;
                sie.keys[0] = clientMsgs[i].keys[0];
                sie.keys[1] = clientMsgs[i].keys[1];
                sie.objectID = 1;
                events.Add(sie);
            }
        }

        serverEvent.events = events;

        ++serverCurrentFrameNum;

        return serverEvent;
    }
};

class SimpleRunGame : MonoBehaviour
{
    public uint BasePlayerID = 1;
    public int MapID = 0;
    public FBTeam team = FBTeam.kBlue;

    public GameObject ballHitEffect = null;
    public GameObject ballTailEffect = null;
    public GameObject actorEffect = null;
    public Transform actorEffectPoint = null;

    public bool useAIPlayer;
    public bool useAIGK;
    uint PlayerID = 1;


    void Start()
    {
        Debuger.SetOutputType(Debuger.OutputType.Console);
        Debuger.SetDebugLevel((int)(Debuger.DebugLevel.Normal | Debuger.DebugLevel.Warning | Debuger.DebugLevel.Error));
        Game.instance.newFBGame();
        StartCoroutine(delayCreate());
    }

    IEnumerator delayCreate()
    {
        yield return new WaitForSeconds(0.1f);
        Game.instance.fbGame.setupFBGame(0, (uint)MapID);
        Game.instance.fbGame.createCoach(string.Format("{0}/Resources/Config/Behaviac", Application.dataPath));

        var count = 1;
        if (useAIPlayer)
        {
            count += 7;
        }
        if (useAIGK)
        {
            count += 2;
        }
        for (int i = 1; i <= count; i++)
        {
            ulong uid = (ulong)i;
            uint playerID = (uint)i;
            uint baseID = 0;
            if (i == 1)
            {
                baseID = BasePlayerID;
            }
            else if (i < 9)
            {
                baseID = (uint)((i + 2) % 3 + 1);
            }
            else
            {
                baseID = 6;
            }
            int teamID = (i + (int)team) % 2 + 1;
            string name = i.ToString();
            bool mainActor = i == 1;
            bool ai = i != 1;
            FBPlayer player = Game.instance.fbGame.createPlayer(uid, playerID, baseID, teamID, name, mainActor, ai);
        }
        /*
        FBPlayer player = Game.instance.fbGame.createPlayer(1, 1, BasePlayerID, 1, "", true, false);

        FBPlayer aiPlayer = Game.instance.fbGame.createPlayer(2, 2, 1, 2, "", false, false);
        */

        Game.instance.fbGame.fbGameCreateCompleted();
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
        }
        if (RealTimeRAProcessCenter.instance != null)
        {
            RealTimeRAProcessCenter.instance.run(Time.deltaTime);
        }
    }

    void Awake()
    {
        gameObject.AddComponent<Zeus>();
        gameObject.AddComponent<ResourceManager>();

        RealTimeRAProcessCenter.create();

        Game.create();

        SceneViews.create();

        LogicEvent.add("onWaitForSync", this, "onWaitForSync");
        LogicEvent.add("editor_onActorPassBallOut", this, "editor_onActorPassBallOut");
        LogicEvent.add("editor_onBallHit", this, "editor_onBallHit");

        LogicEvent.add("onFBGameNewed", this, "onFBGameNewed");
        LogicEvent.add("onFBGameDestroyed", this, "onFBGameDestroyed");

        LogicEvent.add("onFBGameEnter", this, "onFBGameEnter");
        LogicEvent.add("onFBGameStart", this, "onFBGameStart");
        LogicEvent.add("onFBGameOver", this, "onFBGameOver");
    }
    void editor_onBallHit(Vector3 position)
    {
        if (ballHitEffect != null)
        {
            GameObject ga = GameObject.Instantiate(ballHitEffect);
            DelayDestroy delay = ga.AddComponent<DelayDestroy>();
            delay.destroyTime = 5.0f;
            ga.transform.position = position;
        }
    }
    void editor_onActorPassBallOut(ActorView actor)
    {
        if (actorEffect != null)
        {
            GameObject ga = GameObject.Instantiate(actorEffect);
            DelayDestroy delay = ga.AddComponent<DelayDestroy>();
            delay.destroyTime = 5.0f;
            if (actorEffectPoint == null)
                ga.transform.parent = actor.transform;
            else
                ga.transform.parent = actorEffectPoint;
            ga.transform.localPosition = Vector3.zero;
        }

        if (ballTailEffect != null)
        {
            GameObject ga = GameObject.Instantiate(ballTailEffect);
            DelayDestroy delay = ga.AddComponent<DelayDestroy>();
            delay.destroyTime = 10.0f;
            ga.transform.parent = SceneViews.instance.getCurFBScene().ball.transform;
            ga.transform.localPosition = Vector3.zero;
        }

    }

    void OnDestroy()
    {
        RealTimeRAProcessCenter.terminate();

        Game.terminate();

        SceneViews.terminate();
    }
    void onWaitForSync()
    {
        Game.instance.fbGame.changeState(GameState.Ready);
    }
    void onFBGameNewed()
    {
        RealTimeRAProcessCenter.instance.GetType().GetField("fbGame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance).SetValue(RealTimeRAProcessCenter.instance, Game.instance.fbGame);

    }
    void onFBGameDestroyed()
    {
        if (RealTimeRAProcessCenter.instance != null)
        {
            RealTimeRAProcessCenter.instance.GetType().GetField("fbGame",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance).SetValue(RealTimeRAProcessCenter.instance, null);
        }

        Time.timeScale = 1.0f;

    }

    void onFBGameStart()
    {

    }

    void onFBGameOver()
    {
        RealTimeRAProcessCenter.instance.over();
    }

    void onFBGameEnter()
    {
        SinglePlayerUpdateSimulator simulator = createFrameSyncUpdateSimulator(Game.instance.fbGame.frameSync);
        Game.instance.fbGame.frameSyncUpdater = simulator;
        simulator.start();

        RealTimeRAProcessCenter.instance.start();

    }



    SinglePlayerUpdateSimulator _frameSyncUpdateSimulator = null;
    SinglePlayerUpdateSimulator createFrameSyncUpdateSimulator(FrameSync frameSync)
    {
        if (_frameSyncUpdateSimulator == null)
        {
            _frameSyncUpdateSimulator = new SinglePlayerUpdateSimulator();
        }
        _frameSyncUpdateSimulator.init(frameSync);

        return _frameSyncUpdateSimulator;
    }

}
