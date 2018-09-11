using RenderingProcess;
using System.Collections.Generic;

class RenderableActionPlayControlCenter
{
    //主播放
    public LogicFrameProcessor mainPlayer = null;    

    //回放控制
    public PlayBackLogicFrameProcessor playBackPlayer = null;
    //回放内容
    PlayBackLogicFrameList playBackQueue = null;

    public RenderableActionPlayControlCenter()
    {
        mainPlayer = new LogicFrameProcessor();
        mainPlayer.onLogicFrameBegin = this.onLogicFrameBegin;
        mainPlayer.onLogicFrameEnd = this.onLogicFrameEnd;

        playBackPlayer = new PlayBackLogicFrameProcessor();
        playBackQueue = new PlayBackLogicFrameList();
    }

    RAL.LogicFrameQueue _logicFrameQueue = null;

    public void reset()
    {
        mainPlayer.reset();
    }

    public void clear()
    {
        
    }

    public void start(RAL.LogicFrameQueue queue)
    {
        _logicFrameQueue = queue;
        mainPlayer.logicFrameQueue = queue;
        playBackPlayer.setRenderActionGenerator(queue.renderActionGenerator);
        playBackQueue.setRenderActionGenerator(queue.renderActionGenerator);

        processBegin = true;

    }

    public void over()
    {
        processBegin = false; 
    }

    bool processBegin = false;   
    public void run(float time)
    {
        if (!processBegin)
            return;



        mainPlayer.update();

        if (_replaying)
            playBackPlayer.update();

    }

    void onLogicFrameBegin(RAL.LogicFrame frame)
    {
        if (_replaying)
            return;

        //保存状态机
        SceneViews.instance.getCurFBScene().createRecord(frame.frameId);
    }
    void onLogicFrameEnd(RAL.LogicFrame frame)
    {
        if (_replaying)
            return;

        //插入队列
        playBackQueue.push( frame);
    }



    bool _replaying = false;
    public void beginReplay(uint beginFrame, uint endFrame)
    {
        _replaying = true;
        playBackQueue.cast(beginFrame);
        playBackPlayer.replay(playBackQueue, beginFrame * (float)FrameSync.LOGIC_UPDATE_TIME);
    }

    public void endReplay()
    {
        _replaying = false;

        playBackQueue.clear();
    }
};