namespace RAL
{
    //class ShowOffAction : TimeSliceAction
    //{
    //    public override void onGetNew()
    //    {
    //        base.onGetNew();
    //        frameCount = (uint)(RenderingConstTable.uiTalkTime / (float)FrameSync.PHYSICS_UPDATE_TIME);
    //    }
    //}

    //class ShowOffActionProcessor : TimeSliceActionProcessor<ShowOffAction>
    //{
    //    protected override void onActionBegin(RenderableActionPlayControl processor)
    //    {
    //        Events.fire2Lua(
    //            "onActorTalk",
    //            SceneViews.instance.getCurFBScene().getActor(_actionObject.objectID).transform,
    //            "大力出奇迹",
    //            _actionObject.physicalFrameNumber);
    //    }

    //    protected override void onActionEnd(RenderableActionPlayControl processor)
    //    {
    //        Events.fire2Lua(
    //           "onHideActorTalk",
    //            SceneViews.instance.getCurFBScene().getActor(_actionObject.objectID).transform,
    //            _actionObject.physicalFrameNumber);
    //    }
    //}
}
