using UnityEngine;
using DG.Tweening;
using System.Collections;
using Cratos;

partial class CameraCtrl
{
    public Location replayDoor { get; private set; }
    public ActorView replayGK { get; set; }
    public float positionRandomValue { get; private set; }
    public float shootRandomValue { get; private set; }
    public float goalRandomValue { get; private set; }
}

namespace FBCamera
{
    class ReplayState : CameraStateBase
    {
        LookAtAction lookAtAction;
        CameraPosition cameraPosition;
        bool inversePosition { get { return cameraCtrl.replayDoor == Location.kLeftDoor; } }
        bool focusShooter;

        public ReplayState(CameraCtrl ctrl) : base(ctrl) { }

        public override void enter()
        {
            cameraPosition = cameraCtrl.config.getRandomPosition(cameraCtrl.positionRandomValue);
            if (cameraPosition == null)
            {
                Debug.LogError("cameraPosition is null ");
                return;
            }

            LogicEvent.add("onBallLastDetached", this, "onBallLastDetached");
            LogicEvent.add("onBallGoal", this, "onBallGoal");
            LogicEvent.add("onOwnerAttached", this, "onOwnerAttached");
            LogicEvent.add("onOwnerDetached", this, "onOwnerDetached");
            LogicEvent.add("onBeginKillerSkill", this, "onBeginKillerSkill");
            LogicEvent.add("onEndKillerSkill", this, "onEndKillerSkill");
            
            cameraCtrl.StartCoroutine(delayEnter(0.1f));
        }

        public override void execute()
        {
            base.execute();

            if (lookAtTarget != null)
            {
                if (cameraCtrl.ball.owner == null
                    && lookAtAction != null
                    && lookAtAction.lookAtType == LookAtType.GK
                    && cameraCtrl.replayGK != null
                    && lookAtTarget != cameraCtrl.replayGK)
                {
                    var closeGK = Vector2.Distance(new Vector2
                    {
                        x = cameraCtrl.ball.transform.position.x,
                        y = cameraCtrl.ball.transform.position.z
                    },
                    new Vector2
                    {
                        x = cameraCtrl.replayGK.transform.position.x,
                        y = cameraCtrl.replayGK.transform.position.z
                    }) < cameraCtrl.config.lookAtGKDistance;

                    if (closeGK)
                    {
                        setLookAtTarget(cameraCtrl.replayGK, false);
                    }
                }
            }
        }

        public override void exit()
        {
            base.exit();
            cameraCtrl.transform.DOKill();
            cameraCtrl.cam.DOKill();

            cameraCtrl.replayGK = null;
            lookAtAction = null;
            focusShooter = false;
        }

        #region private methods

        void playAction(CameraActionBase action)
        {
            if (action == null)
            {
                Debug.LogError("action is null");
                return;
            }
            switch (action.type)
            {
                case CameraActionType.LookAt:
                    lookAtAction = action as LookAtAction;
                    //Debug.LogError("lookAtAction type " + lookAtAction.lookAtType);
                    break;
                case CameraActionType.FOV:
                    {
                        var fovAction = action as FOVAction;
                        var time = fovAction.time.Value;
                        var tweener = cameraCtrl.cam.DOFieldOfView(fovAction.target, time);
                        if (fovAction.beginTime.HasValue)
                        {
                            tweener.SetDelay(fovAction.beginTime.Value);
                        }
                        if (fovAction.stayTime.HasValue)
                        {
                            var stayTime = fovAction.stayTime.Value;
                            tweener.OnStart(() =>
                            {
                                cameraCtrl.cam.DOFieldOfView(cameraCtrl.config.defaultFOV, time).SetDelay(stayTime);
                            });
                        }
                    }
                    break;
                case CameraActionType.Move:
                    {
                        var moveAction = action as MoveAction;
                        var tweener = cameraCtrl.transform.DOMove(moveAction.getTarget(inversePosition), moveAction.time.Value).SetEase(Ease.InOutCubic);
                        if (moveAction.beginTime.HasValue)
                        {
                            tweener.SetDelay(moveAction.beginTime.Value);
                        }
                    }
                    break;
            }
        }

        protected override Vector3 getLookAtPosition()
        {
            if (focusShooter)
            {
                var actor = lookAtTarget as ActorView;
                if (actor != null)
                {
                    return actor.getWaistPoint().position;
                }
            }
            return base.getLookAtPosition();
        }

        #endregion

        #region events

        void onBallLastDetached(uint id)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(id);
            var actions = cameraPosition.getShootActions(cameraCtrl.shootRandomValue);
            if (!actions.isNullOrEmpty())
            {
                actions.forEach(a => playAction(a));
            }
            if (lookAtAction != null && lookAtAction.lookAtType == LookAtType.Player)
            {
                setLookAtTarget(actor, false);
            }
            else
            {
                setLookAtTarget(cameraCtrl.ball, false);
            }
        }

        void onBallGoal(FBTeam team, Location door, uint id)
        {
            var actions = cameraPosition.getGoalActions(cameraCtrl.goalRandomValue);
            if (!actions.isNullOrEmpty())
            {
                actions.forEach(a => playAction(a));
            }
        }

        void onOwnerAttached(uint id, bool gk)
        {
            setLookAtTarget(SceneViews.instance.getCurFBScene().getActor(id), false);
        }

        void onOwnerDetached(uint id)
        {
            setLookAtTarget(cameraCtrl.ball, false);
        }

        IEnumerator delayEnter(float delay)
        {
            yield return new WaitForSeconds(delay);
            setLookAtTarget(cameraCtrl.ball.owner == null ? (EntityView)cameraCtrl.ball : cameraCtrl.ball.owner);
            var actions = cameraPosition.getActions();
            if (!actions.isNullOrEmpty())
            {
                actions.forEach(a => playAction(a));
            }
            cameraCtrl.transform.position = cameraPosition.getPosition(inversePosition);
            cameraCtrl.transform.forward = (lookAtTarget.getCenterPosition() - cameraCtrl.transform.position).normalized;

            useLerpLookAt = false;
            cameraCtrl.StartCoroutine(delaySetUseLerpLookAt(true, 0.2f));
        }

        void onBeginKillerSkill(ActorView acotr)
        {
            focusShooter = true;
        }

        void onEndKillerSkill(ActorView acotr)
        {
            focusShooter = false;
        }

        #endregion
    }
}
