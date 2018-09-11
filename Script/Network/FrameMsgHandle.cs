using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cratos;

//帧消息处理
public class FrameMsgHanlde
{
    public FrameMsgHanlde()
    {
        initEvent();
    }

    void initEvent()
    {
        Events.add("ServerFrameMsg", this, "onServerFrameMsg");
        Events.add("FramesMsg", this, "onFramesMsg");
    }

    public void destroy()
    {
        Events.remove(this);
    }

    public void onServerFrameMsg(ServerFrameMsg cmd)
    {
        if (!Game.instance.fbGame.frameSync.frameSyncStarted)
            Game.instance.fbGame.frameSync.start();

        if (Game.instance.fbGame.frameSyncUpdater.thisType != FrameSyncUpdater.type)
            return;

        Game.instance.fbGame.frameSyncUpdater.updateFrame(cmd, UnityEngine.Time.realtimeSinceStartup);

#if FRAME_RECORDING
        if (FrameRecording.instance != null)
        {
            FrameRecording.instance.pushServerMessage(cmd);
        }
#endif
    }

    public void onFramesMsg(FramesMsg cmd)
    {
        if (Game.instance.fbGame.frameSyncUpdater.thisType != FrameSyncUpdater.type)
            return;

        Game.instance.fbGame.frameSyncUpdater.updateFrames(cmd, UnityEngine.Time.unscaledTime);
    }
}
