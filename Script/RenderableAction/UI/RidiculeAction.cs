namespace RAL
{
    //class RidiculeAction : TimeSliceAction
    //{
    //    public override void onGetNew()
    //    {
    //        base.onGetNew();
    //        frameCount = (uint)(RenderingConstTable.uiTalkTime / (float)FrameSync.PHYSICS_UPDATE_TIME);
    //    }
    //}
    //class RidiculeActionProcessor : TimeSliceActionProcessor<RidiculeAction>
    //{
    //    protected override void onActionBegin(RenderableActionPlayControl processor)
    //    {
    //        Events.fire2Lua(
    //           "onActorTalk",
    //           SceneViews.instance.getCurFBScene().getActor(_actionObject.objectID).transform,
    //           "碧池",
    //           _actionObject.physicalFrameNumber);
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

