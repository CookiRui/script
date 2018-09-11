using System;
using Cratos;

public interface IFrameSyncUpdater
{
    Type thisType { get; }

    void updateFrame(ServerFrameMsg cmd, float time);

    void updateFrames(FramesMsg cmd, float time);

    void start();

    void destroy();
}

//帧同步更新的驱动器
public class FrameSyncUpdater : IFrameSyncUpdater
{
    public static Type type = typeof(FrameSyncUpdater);

    FrameSync frameSync;

    Type _type;
    public FrameSyncUpdater(FrameSync sync)
    {
        frameSync = sync;
        _type = this.GetType();
    }

    //更新一帧
    public void updateFrame(ServerFrameMsg cmd, float time)
    {
        if (frameSync == null)
            return;

        frameSync.pushFrameInputEvent(cmd);
        frameSync.udpateFrame(time);
    }

    //更新多帧
    public void updateFrames(FramesMsg cmd, float time)
    {
        if (frameSync == null)
            return;

        var count = cmd.allFrameMessages.Count;
        for (int i = 0; i < count; ++i)
        {
            frameSync.pushFrameInputEvent(cmd.allFrameMessages[i]);
            frameSync.udpateFrame(time);
        }
    }

    public void destroy()
    {
 
    }

    public void start()
    {
    }

    public Type thisType
    {
        get { return _type; }
    }
}
