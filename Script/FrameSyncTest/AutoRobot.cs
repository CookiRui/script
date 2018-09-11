using UnityEngine;
using System.Collections;

class AutoRobot : MonoBehaviour
{
    //播放速度
    public float PlaySpeed = 1.0f;
    //测试次数
    public int TestingCount = 10;   

    void Start()
    {
        StartCoroutine(robotRun());
    }

    IEnumerator robotRun()
    {
        for (int i = 0; i < TestingCount; ++i)
        {
            Debuger.SetOutputFile("debug" + i + ".txt");
            Debuger.Open();

            FrameRecording.instance.PlaySpeed = PlaySpeed;
            FrameRecording.instance.renderPlayerSpeed = PlaySpeed - 0.1f;
            FrameRecording.instance.beginSimulation();

            while (!FrameRecording.instance.isGameOver)
            {
                yield return new WaitForSeconds(1.0f);
            }

            FrameRecording.instance.stopSimulation();

            Debuger.Close();
        }
        yield return null;

        string[] files = new string[TestingCount];
        //检测文件不同
        for (int i = 0; i < TestingCount; ++i)
        {
            files[i] = Application.persistentDataPath + "/debug" + i + ".txt";
        }
        bool sameFileContent = HelperTool.compareTextFile(files);
        if (sameFileContent)
        {
            Debug.Log("===================Same File Content");
        }

    }

    void Update()
    {
        Game.instance.run();

        RealTimeRAProcessCenter.instance.run(Time.deltaTime);
    }
    void Awake()
    {
        gameObject.AddComponent<ResourceManager>();

        Debuger.SetOutputType(Debuger.OutputType.File);
        Debuger.SetDebugLevel((int)(Debuger.DebugLevel.Logic));

        Game.create();

        RealTimeRAProcessCenter.create();

        FrameRecording.create();

        SceneViews.create();

        LogicEvent.add("onFBGameNewed", this, "onFBGameNewed");
        LogicEvent.add("onFBGameStart", this, "onFBGameStart");
        LogicEvent.add("onFBGameOver", this, "onFBGameOver");


    }

    void OnDestroy()
    {
        Game.terminate();

        RealTimeRAProcessCenter.terminate();

        FrameRecording.terminate();

        SceneViews.terminate();

    }


    void onFBGameNewed( FBGame game )
    {

        RealTimeRAProcessCenter.instance.GetType().GetField("fbGame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance).SetValue(RealTimeRAProcessCenter.instance, game);

#if FRAME_RECORDING
        FrameRecording.instance.GetType().GetField("fbGame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance).SetValue(FrameRecording.instance, game);
#endif
    }
    void onFBGameDeleted()
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
        //RealTimeRAProcessCenter.instance.start();
    }

    void onFBGameOver()
    {
        //RealTimeRAProcessCenter.instance.over();
    }
}