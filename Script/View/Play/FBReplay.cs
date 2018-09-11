using System.Collections.Generic;
using System.IO;

partial class FBReplay : Singleton<FBReplay>
{
    //RenderableActionPlayControlCenter controlCenter = null;
    //bool over;
    //public int playSpeed { get; private set; }

    //public FBReplay()
    //{
    //}

    //public override void onInit()
    //{
    //    base.onInit();

    //    controlCenter = new RenderableActionPlayControlCenter();

    //}
    //public override void onUninit()
    //{
    //    base.onUninit();

    //    controlCenter.reset();
    //    controlCenter = null;
    //    Events.fire2Rendering("onFBReplayDestroyed");
    //}

    //public bool createFBReplay()
    //{
    //    string replayFile = ResourceManager.inst.WriteablePath + "Replay/replay.bytes";
    //    if (!File.Exists(replayFile)) return false;

    //    using (var fs = File.OpenRead(replayFile))
    //    {
    //        var length = (int)fs.Length;
    //        var bytes = new byte[length];
    //        fs.Read(bytes, 0, length);
    //        createFBReplay(bytes);
    //        return true;
    //    }
    //}

    //public void createFBReplay(byte[] data)
    //{
    //    playSpeed = 1;
    //    over = false;
    //    Events.fire2Rendering("onFBReplayCreated");

    //    start(data);
    //}

    //public void changePlaySpeed()
    //{
    //    if (playSpeed >= 4)
    //    {
    //        playSpeed = 1;
    //    }
    //    else
    //    {
    //        playSpeed++;
    //    }
    //    setPlaySpeed(playSpeed);
    //}

    //void setPlaySpeed(int speed)
    //{
    //    //TODO....controlCenter.mainPlayer.playSpeed = speed;
    //    controlCenter.playBackPlayer.playSpeed = speed;
    //}

    ////开始全场回放

    //public void start(byte[] data)
    //{
    //    BytesStream stream = new BytesStream(data);

    //    List<RenderableAction> aa = new List<RenderableAction>();
    //    while (stream.Pos < stream.Used)
    //    {
    //        RenderableActionID typeID = (RenderableActionID)stream.ReadByte();

    //        RenderableAction ra = RenderableActionFactory.instance.create(typeID);

    //        ra.unserialize(stream);

    //        controlCenter.pusRenderableAction(ra);

    //        aa.Add(ra);
    //    }

    //    //TODO....controlCenter.mainPlayer.renderTimeElasped = 0.0f;
    //    //TODO....controlCenter.mainPlayer.ignoreFrameSpeedup = true;
    //    controlCenter.processBegin = true;

    //}

    //public void run(float time)
    //{
    //    controlCenter.run(time);

    //    //TODO....if (!over && controlCenter.mainPlayer.cache.getNeedProcessed().isNullOrEmpty())
    //    {
    //        over = true;
    //        setPlaySpeed(1);
    //        Events.fire2Lua("onReplayOver");
    //    }
    //}

}