using Cratos;
using System.Collections.Generic;

class RealTimeRAProcessCenter : Singleton<RealTimeRAProcessCenter>
{

    FBGame fbGame = null;

    public RenderableActionPlayControlCenter controlCenter = null;

    public override void onInit()
    {
        base.onInit();

        controlCenter = new RenderableActionPlayControlCenter();

    }

    public void start()
    {
        Game.instance.fbGame.logicFrameQueue.reset();
        controlCenter.start(Game.instance.fbGame.logicFrameQueue);
    }



    public void over()
    {
        controlCenter.over();
    }

    public void reset()
    {
        controlCenter.reset();

        Game.instance.fbGame.logicFrameQueue.reset();
    }


    public void run(float timeDelta)
    {
        if (fbGame == null || fbGame.frameSync == null)
            return;
        if (controlCenter != null)
        {
            if (SceneViews.instance != null)
            {
                var scene = SceneViews.instance.getCurFBScene();
                if (scene != null)
                {
                    scene.actionProcessSpeedTimeScale = controlCenter.mainPlayer.timeScale;
                }
            }
            controlCenter.run(timeDelta);
        }
    }

    public void saveReplay()
    {
        //BytesStream stream = new BytesStream();
        ////List<RenderableAction> streamActions = new List<RenderableAction>();
        //var e = (controlCenter.playBackPlayer.cache as RenderableActionRecorderCache).getAll().GetEnumerator();
        //while (e.MoveNext())
        //{
        //    //streamActions.Add(e.Current);
        //    stream.Write((byte)e.Current.typeID);
        //    e.Current.serialize(stream);
        //}

        //ResourceManager.inst.saveToFile(stream, "Replay/replay.bytes");

        //stream.Pos = 0;
        //List<RenderableAction> ssss = new List<RenderableAction>();
        //while (stream.Pos < stream.Used)
        //{
        //    RenderableActionID typeID = (RenderableActionID)stream.ReadByte();

        //    RenderableAction ra = RenderableActionFactory.instance.create(typeID);

        //    try
        //    {
        //        ra.unserialize(stream);
        //    }
        //    catch
        //    {
        //        int iii = 0;
        //    }

        //    ssss.Add(ra);
        //}

        //Debuger.Log("serializecount:" + renderableActionList.Count + " unserializecount:" + ssss.Count);

    }

    public override void onUninit()
    {
        base.onUninit();

        controlCenter.clear();
        controlCenter = null;
    }
}