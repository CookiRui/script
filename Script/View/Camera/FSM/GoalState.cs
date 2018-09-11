using UnityEngine;
using Cratos;

partial class CameraCtrl
{
    public uint goaler { get; set; }
}

namespace FBCamera
{
    class GoalState : CameraStateBase
    {
        float timer;
        ActorView actor;
        bool beginGoalShow;
        float? waitTime;
        public GoalState(CameraCtrl ctrl) : base(ctrl) { }

        public override void enter()
        {
            base.enter();
            actor = SceneViews.instance.getCurFBScene().getActor(cameraCtrl.goaler);
            timer = 0;
            waitTime = cameraCtrl.config.goalBeginTime;//转身时间
            beginGoalShow = false;
            LogicEvent.add("onBeginGoalShow", this, "onBeginGoalShow");
        }

        public override void execute()
        {
            if (!beginGoalShow) return;

            if (waitTime.HasValue)
            {
                if (timer < waitTime.Value)
                {
                    timer += Time.deltaTime;
                    return;
                }
                waitTime = null;
                timer = 0;
            }

            if (timer < cameraCtrl.config.goalMoveTime)
            {
                var position = Vector3.Slerp(cameraCtrl.config.goalBeginPosition, cameraCtrl.config.goalEndPosition, timer / cameraCtrl.config.goalMoveTime);
                cameraCtrl.transform.position = calculateOffsetPosition(actor, position);
                cameraCtrl.transform.forward = (actor.getCenterPosition() - cameraCtrl.transform.position).normalized;
                timer += Time.deltaTime;
            }
            else
            {
                beginGoalShow = false;
            }
        }
      
        void onBeginGoalShow()
        {
            beginGoalShow = true;
        }

    }
}
