using System.Collections.Generic;
using FixMath.NET;
using Cratos;

public class FrameSync
{
    public const int LOGIC_FPS = 15;
    public const int LOGIC_PHYSICS_MULTIPLY = 4;
    public const int LOGIC_PHYSICS_FPS = LOGIC_FPS * LOGIC_PHYSICS_MULTIPLY;
    //逻辑帧更新频率
    public static readonly Fix64 LOGIC_UPDATE_TIME = Fix64.One / (Fix64)LOGIC_FPS;
    //物理更新频率
    public static readonly Fix64 PHYSICS_UPDATE_TIME = Fix64.One / (Fix64)LOGIC_PHYSICS_FPS;
    //渲染帧相对于逻辑帧的延迟    
    public static readonly float RENDER_FRAME_DELAY = 0;//(float)LOGIC_UPDATE_TIME;


    ushort _currentLogicFrameNum = 0;
    public ushort currentLogicFrameNum
    {
        get { return _currentLogicFrameNum; }
        set { _currentLogicFrameNum = value; }
    }    

    bool _frameSyncStarted = false;

    public bool frameSyncStarted { get { return _frameSyncStarted; } }


    //客户端收到的服务器收集的帧输入事件
    List<ServerFrameMsg> serverFrameInputEventList = new List<ServerFrameMsg>();
    public IFrameSyncEventHandler frameSyncEventHandler = null;
    List<ILogicUpdater> logicUpdators = new List<ILogicUpdater>();

    public FrameSync()
    {
        _currentLogicFrameNum = 0;
    }

    public void start()
    {
        _currentLogicFrameNum = 0;
        _frameSyncStarted = true;
        for (int i = 0; i < logicUpdators.Count; ++i)
        {
            logicUpdators[i].LogicStart();
        }
    }

    public void over()
    {
        for (int i = 0; i < logicUpdators.Count; ++i)
        {
            logicUpdators[i].LogicOver();
        }

        frameSyncEventHandler = null;
        _frameSyncStarted = false;
    }


    public void udpateFrame(float time)
    {
        if (logicUpdators.Count == 0)
            return;

        while (serverFrameInputEventList.Count > 0)
        {
            runLogic(time);
        }
    }

    public void runLogic(float time)
    {
        //
        for (int i = 0; i < logicUpdators.Count; ++i)
        {
            logicUpdators[i].preUpdate();
        }

        ServerFrameMsg currentFrameInputEvent = popFrameInputEvent();

        if (frameSyncEventHandler != null)
        {
            for (int i = 0; i < currentFrameInputEvent.events.Count; ++i)
            {
                ServerFrameInputEvent playerInputEvent = currentFrameInputEvent.events[i];

                frameSyncEventHandler.handleFrameInputEvent(playerInputEvent);
            }
        }

        //Debuger.Log("current frame num " + currentFrameInputEvent.frameNum);
        //丢帧？？？？
        if (currentFrameInputEvent.frameNum　!= 0 && currentFrameInputEvent.frameNum != (currentLogicFrameNum + 1) )
        {
            Debuger.LogError("!!!!!!!!!!!!!!!!LostFrame: " + (currentLogicFrameNum + 1) + " ,currentFrameNum "+currentFrameInputEvent.frameNum);
        }

        currentLogicFrameNum = currentFrameInputEvent.frameNum;

        for (int i = 0; i < logicUpdators.Count; ++i)
        {
            logicUpdators[i].update(time);
        }
    }

    public void addLogicUpdator(ILogicUpdater updator)
    {
        logicUpdators.Add(updator);
    }
    public void removeLogicUpdator(ILogicUpdater updator)
    {
        logicUpdators.Remove(updator);
    }
    
    void removeAllUpdators()
    {
        logicUpdators.Clear();
    }

    //插入一个帧输入事件，从网络层收到的帧同步消息通过该接口插入到列表中去
    public void pushFrameInputEvent(ServerFrameMsg fie)
    {
        serverFrameInputEventList.Add(fie);
    }

    public ServerFrameMsg popFrameInputEvent()
    {
        if (serverFrameInputEventList.Count == 0)
            return null;
        ServerFrameMsg fie = serverFrameInputEventList[0];
        serverFrameInputEventList.RemoveAt(0);

        return fie;
    }

    public ServerFrameMsg peekFrameInputEvent(int idx)
    {
        if (idx < 0 || idx >= serverFrameInputEventList.Count)
            return null;

        return serverFrameInputEventList[idx];
    }
};
