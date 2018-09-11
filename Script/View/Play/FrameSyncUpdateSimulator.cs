using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cratos;

class FrameSyncUpdateSimulator : IFrameSyncUpdater
{
    bool _stopped = true;
    public static Type type = typeof(FrameSyncUpdateSimulator);

    Type _type;
    public FrameSyncUpdateSimulator()
    {
        _type = this.GetType();
    }
    public void init(FrameSync sync)
    {
        frameUpdator = new FrameSyncUpdater(sync);
        serverCurrentFrameNum = sync.currentLogicFrameNum;
    }

    int lastFrameNum = -1;
    float frameSyncTimeOrigin = 0;
    public void start()
    {
        //Debug.LogError("FrameSyncUpdateSimulator Start");
        lastFrameNum = -1;
        frameSyncTimeOrigin = Time.unscaledTime;
        _stopped = false;

        lastTime = Time.unscaledTime;

    }
    public void destroy()
    {
        _stopped = true;
    }

    public void run()
    {
        if (_stopped)
            return;
        
        var frameSyncTime = Time.unscaledTime - frameSyncTimeOrigin;

        var currentFrameNum = (int)(frameSyncTime * 15);

        for (int i = lastFrameNum; i < currentFrameNum; ++i)
        {
            ServerFrameMsg serverFrameMsg = createServerFrameMsg();

            //updateFrame(serverFrameMsg,Game.instance.logicFrameQueue.nextFrameId / (float)FrameSync.LOGIC_FPS);
            updateFrame(serverFrameMsg, Time.realtimeSinceStartup);
        }
        var d = currentFrameNum - lastFrameNum;
        count += d;
        lastFrameNum = currentFrameNum;
    }

    float lastTime = 0;
    int count = 0;

    FrameSyncUpdater frameUpdator = null;

    public Type thisType
    {
        get {  return _type; }
    }
    public void updateFrame(ServerFrameMsg cmd, float time)
    {
        frameUpdator.updateFrame(cmd, time);
    }
    public void updateFrames(FramesMsg cmd, float time)
    {
        frameUpdator.updateFrames(cmd, time);
    }

    protected ushort serverCurrentFrameNum = 0;
   
    protected virtual ServerFrameMsg createServerFrameMsg()
    {
        ServerFrameMsg serverEvent = new ServerFrameMsg();
        serverEvent.frameNum = serverCurrentFrameNum;

        ++serverCurrentFrameNum;

        return serverEvent;
    }
    //List<ServerFrameInputEvent> events = new List<ServerFrameInputEvent>();
    //public void simulateMainInput(ClientFrameMsg input)
    //{
    //    ServerFrameInputEvent sie = new ServerFrameInputEvent();
    //    sie.angle = input.angle;
    //    sie.keys[0] = input.keys[0];
    //    sie.keys[1] = input.keys[1];
    //    //sie.moveDirectionX = input.moveDirectionX;
    //    //sie.moveDirectionY = inputd.moveDirectionY;
    //    sie.objectID = 1;
    //    events.Add(sie);
    //}

};