using UnityEngine;
using RAL;
using Cratos;

namespace RenderingProcess
{
    [RenderActionProcessor(typeof(None))]
    public static class NoneRenderActionProcessor
    {
        public static void doDone(None ra)
        {
            ra.dump();
        }
    }

    [RenderActionProcessor(typeof(GameInitAction))]
    public static class InitGameActionProcessor
    {
        public static void doDone(GameInitAction actionObject)
        {
            SceneViews.instance.getCurFBScene().gameInit();
        }
    }


    [RenderActionProcessor(typeof(CreateWorldAction))]
    public static class CreateWorldActionProcessor
    {
        public static void doDone(CreateWorldAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            scene.createWorld(actionObject.mapID);
            scene.leftDoorPosition = new Vector3 { x = -actionObject.mainExtent.x };
            scene.rightDoorPosition = new Vector3 { x = actionObject.mainExtent.x };
        }
    }

    [RenderActionProcessor(typeof(CreateActorAction))]
    public static class CreateActorActionProcessor
    {
        public static void doDone(CreateActorAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().createActor(
                                                          actionObject.objectID,
                                                          actionObject.team,
                                                          actionObject.avatarName,
                                                          actionObject.avatarPart,
                                                          actionObject.runAnimiationNormalSpeeds,
                                                          actionObject.height,
                                                          actionObject.gk,
                                                          actionObject.name,
                                                          actionObject.element
                                                          , actionObject.roleId);
            actor.setPosition(new Vector3(actionObject.position.x, 0.0f, actionObject.position.y));
        }
    }

    [RenderActionProcessor(typeof(MainActorCreatedAction))]
    public static class MainActorCreatedActionProcessor
    {
        public static void doDone(MainActorCreatedAction actionObject)
        {
            FBSceneView scene = SceneViews.instance.getCurFBScene() as FBSceneView;
            if (scene != null)
            {
                scene.setMainCharacterData(actionObject.objectID, (int)actionObject.team);
            }
        }
    }

    [RenderActionProcessor(typeof(CreateBallAction))]
    public static class CreateBallActionProcessor
    {
        public static void doDone(CreateBallAction actionObject)
        {
            EntityView se = SceneViews.instance.getCurFBScene().createBall(actionObject.objectID, actionObject.prefab, actionObject.radius);
            se.setPosition(new Vector3(actionObject.position.x, 0.0f, actionObject.position.y));
        }
    }



    [RenderActionProcessor(typeof(BallAttachAction))]
    public static class BallAttachActionProcessor
    {
        public static void doDone(BallAttachAction actionObject)
        {
            SceneViews.instance.getCurFBScene().ballAttach(actionObject.objectID);
        }
    }

    [RenderActionProcessor(typeof(ActorStandCatchingBallAction))]
    public static class ActorStandCatchingBallActionProcessor
    {
        public static void doDone(ActorStandCatchingBallAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.animator.SetBool("footSide", !actionObject.rightFoot);
            actor.animator.SetInteger("stoppingType", 0);
            actor.setAnimatorTrigger("stopping");
        }
    }

    [RenderActionProcessor(typeof(ActorStandCatchingBallBeginAction))]
    public static class ActorStandCatchingBallBeginActionProcessor
    {
        public static void doDone(ActorStandCatchingBallBeginAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("prepareToStop");
        }
    }

    [RenderActionProcessor(typeof(ActorAirCatchingBallAction))]
    public static class ActorAirCatchingBallActionProcessor
    {
        public static void doDone(ActorAirCatchingBallAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("stopping");
            actor.animator.SetInteger("stoppingType", actionObject.catchingType);
        }
    }

    [RenderActionProcessor(typeof(ActorAirCatchingBallBeginAction))]
    public static class ActorAirCatchingBallBeginActionProcessor
    {
        public static void doDone(ActorAirCatchingBallBeginAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("prepareToStop");
        }
    }

    [RenderActionProcessor(typeof(ActorTigerCatchingBallBeginAction))]
    public static class ActorTigerCatchingBallBeginActionProcessor
    {
        public static void doDone(ActorTigerCatchingBallBeginAction actionObject)
        {
            Debuger.Log("ActorTigerCatchingBallBeginAction Done");
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("catch");
        }
    }

    [RenderActionProcessor(typeof(DoorKeeperCatchingBallAction))]
    public static class DoorKeeperCatchingBallActionProcessor
    {
        public static void doDone(DoorKeeperCatchingBallAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("save");
            actor.animator.SetBool("footSide", !actionObject.rightSide);
            actor.animator.SetInteger("zoneIndex", actionObject.zoneIndex);
        }
    }

    [RenderActionProcessor(typeof(DoorKeeperBeginToGetupAction))]
    public static class DoorKeeperBeginToGetupActionProcessor
    {
        public static void doDone(DoorKeeperBeginToGetupAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("rise");
        }
    }

    [RenderActionProcessor(typeof(DoorKeeperCatchingBallBeginAction))]
    public static class DoorKeeperCatchingBallBeginActionProcessor
    {
        public static void doDone(DoorKeeperCatchingBallBeginAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.animator.SetBool("block", actionObject.canCatchBall);
            actor.setAnimatorTrigger("prepareToStop");
        }
    }

    [RenderActionProcessor(typeof(SlowGetPassingBallAction))]
    public static class SlowGetPassingBallActionProcessor
    {
        public static void doDone(SlowGetPassingBallAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("prepareToStop");
            if (SceneViews.instance.getCurFBScene().mainActorFrameID == actionObject.objectID)
            {
                if (InputEventTranslator.instance != null)
                    InputEventTranslator.instance.clearLastInput();
            }
        }
    }

    [RenderActionProcessor(typeof(ActorEndMoveAction))]
    public static class ActorEndMoveActionProcessor
    {
        public static void doDone(ActorEndMoveAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.resetAnimtorSpeed();
            actor.hideRunEffect();
        }
    }


    [RenderActionProcessor(typeof(PassBeginAction))]
    public static class PassBeginActionProcessor
    {
        public static void doDone(PassBeginAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.animator.SetBool("footSide", !actionObject.rightFoot);
            actor.setAnimatorTrigger(actionObject.shortPassBall ? "shortPass" : "longPass");
        }
    }


    [RenderActionProcessor(typeof(ShootBeginAction))]
    public static class ShootBeginActionProcessor
    {
        public static void doDone(ShootBeginAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            var actor = scene.getActor(actionObject.objectID);
            Animator animator = actor.animator;
            animator.SetBool("footSide", !actionObject.rightFoot);
            animator.SetBool("shoot", true);
            scene.ball.showChargeEffect();
        }
    }


    [RenderActionProcessor(typeof(ShootBallReadyAction))]
    public static class ShootBallReadyActionProcessor
    {
        public static void doDone(ShootBallReadyAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            var actor = scene.getActor(actionObject.objectID);
            var animator = actor.animator;
            animator.SetInteger("shootType", (int)actionObject.shootType);
            animator.SetBool("shoot", false);

            if (actionObject.shootType == (int)ShootType.Killer)
            {
                scene.requestFocus(actor);
            }
        }
    }

    [RenderActionProcessor(typeof(WarmUpAction))]
    public static class WarmUpActionProcessor
    {
        public static void doDone(WarmUpAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            if (actor != null && actor.animator != null)
            {
                actor.animator.SetBool("warmUp", true);
            }
        }
    }


    [RenderActionProcessor(typeof(CheerStandAction))]
    public static class CheerStandActionProcessor
    {
        public static void doDone(CheerStandAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.animator.SetBool("cheerStand", true);
        }
    }

    [RenderActionProcessor(typeof(DismayAction))]
    public static class DismayActionProcessor
    {
        public static void doDone(DismayAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.animator.SetBool("dismay", true);
        }
    }



    [RenderActionProcessor(typeof(IdleAction))]
    public static class IdleActionProcessor
    {
        public static void doDone(IdleAction actionObject)
        {
            //Debuger.Log(processor.renderTimeElasped + " SetState Idle " + actionObject.physicalFrameNumber);
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            if (actor.animator != null)
            {
                actor.animator.SetInteger("state", (int)ActorAnimatorState.Idle);

                actor.resetAnimtorSpeed();
                actor.hideRunEffect();
            }
        }
    }


    [RenderActionProcessor(typeof(RunAction))]
    public static class RunActionProcessor
    {
        public static void doDone(RunAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            if (actor.animator != null)
            {
                //Debuger.LogError(actionObject.physicalFrameNumber + "   ActorAnimatorState.Run " + processor.cache.ToString());
                actor.animator.SetInteger("state", (int)ActorAnimatorState.Run);
                actor.resetAnimtorSpeed();
                actor.hideRunEffect();
            }
        }
    }

    [RenderActionProcessor(typeof(MovingToIdleAction))]
    public static class MovingToIdleActionProcessor
    {
        public static void doDone(MovingToIdleAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            //Debuger.Log(processor.renderTimeElasped + " SetState MovingToIdle " + actionObject.physicalFrameNumber);
            if (actor.animator != null)
            {
                actor.animator.SetInteger("state", (int)ActorAnimatorState.MovingToIdle);
            }
        }
    }

    [RenderActionProcessor(typeof(OtherAction))]
    public static class OtherActionProcessor
    {
        public static void doDone(OtherAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            if (actor.animator != null)
            {
                //Debuger.LogError(actionObject.physicalFrameNumber + "   ActorAnimatorState.Other " + processor.cache.ToString());
                actor.animator.SetInteger("state", (int)ActorAnimatorState.Other);
                actor.resetAnimtorSpeed();
                actor.hideRunEffect();
            }
        }
    }

    [RenderActionProcessor(typeof(AnimatorIntAction))]
    public static class AnimatorIntActionProcessor
    {
        public static void doDone(AnimatorIntAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).animator.SetInteger(actionObject.name, actionObject.value);
        }
    }

    [RenderActionProcessor(typeof(AnimatorFloatAction))]
    public static class AnimatorFloatActionProcessor
    {
        public static void doDone(AnimatorFloatAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).animator.SetFloat(actionObject.name, actionObject.value);
        }
    }

    [RenderActionProcessor(typeof(AnimatorTriggerAction))]
    public static class AnimatorTriggerActionProcessor
    {
        public static void doDone(AnimatorTriggerAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).setAnimatorTrigger(actionObject.name);
        }
    }

    [RenderActionProcessor(typeof(AnimatorBoolAction))]
    public static class AnimatorBoolActionProcessor
    {
        public static void doDone(AnimatorBoolAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).animator.SetBool(actionObject.name, actionObject.value);
        }
    }

    [RenderActionProcessor(typeof(AnimatorScaleAction))]
    public static class AnimatorScaleActionProcessor
    {
        public static void doDone(AnimatorScaleAction actionObject)
        {
            SceneViews.instance.getCurFBScene().animatorTimeScale = actionObject.animatorScale;
        }
    }




    [RenderActionProcessor(typeof(TurnAction))]
    public static class TurnActionProcessor
    {
        public static void doDone(TurnAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);

            Quaternion end = Quaternion.LookRotation(new Vector3(actionObject.direction.x, 0, actionObject.direction.y));
            actor.setRotation(end);

            actor.setAnimatorTrigger("turn");
        }
    }

    [RenderActionProcessor(typeof(DribbleAction))]
    public static class DribbleActionProcessor
    {
        public static void doDone(DribbleAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).setAnimatorTrigger("dribble");
        }
    }

    [RenderActionProcessor(typeof(FallAction))]
    public static class FallActionProcessor
    {
        public static void doDone(FallAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).setAnimatorTrigger("fall");
        }
    }

    [RenderActionProcessor(typeof(SlideAction))]
    public static class SlideActionProcessor
    {
        public static void doDone(SlideAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).setAnimatorTrigger("slide");
        }
    }

    [RenderActionProcessor(typeof(TauntAction))]
    public static class TauntActionProcessor
    {
        public static void doDone(TauntAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            if (actor != null && actor.animator != null)
            {
                actor.animator.SetBool("warmUp", false);
                actor.setAnimatorTrigger("taunt");
            }
        }
    }

    [RenderActionProcessor(typeof(CheerUniqueAction))]
    public static class CheerUniqueActionProcessor
    {
        public static void doDone(CheerUniqueAction actionObject)
        {
            var actor = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);
            actor.setAnimatorTrigger("cheerUnique");

        }
    }

    [RenderActionProcessor(typeof(BallCollidedNetAction))]
    public static class BallCollidedNetActionProcessor
    {
        public static void doDone(BallCollidedNetAction actionObject)
        {
            var ball = SceneViews.instance.getCurFBScene().ball;
            ball.hitNet(actionObject.point, actionObject.preVelocity, actionObject.kickerElement);
            ball.setRotateType(actionObject.curVelocity);

        }
    }

    [RenderActionProcessor(typeof(BallCollidedWallAction))]
    public static class BallCollidedWallActionProcessor
    {
        public static void doDone(BallCollidedWallAction actionObject)
        {
#if ARTIEST_MODE  
        LogicEvent.fire("editor_onBallHit", actionObject.point);
#else
            var ball = SceneViews.instance.getCurFBScene().ball;
            //jlx2017.07.05-log:这里加负号是为了将墙的法线反向
            ball.hitWall(actionObject.point, -actionObject.normal, actionObject.kickerElement);
            ball.setRotateType(actionObject.velocity);
#endif
        }
    }

    [RenderActionProcessor(typeof(BallDetachAction))]
    public static class BallDetachActionProcessor
    {
        public static void doDone(BallDetachAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            scene.ballDetach(actionObject.objectID);
        }
    }

    [RenderActionProcessor(typeof(BallLastDetachAction))]
    public static class BallLastDetachActionProcessor
    {
        public static void doDone(BallLastDetachAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            scene.ballDetach(actionObject.objectID);
            LogicEvent.fire2Rendering("onBallLastDetached", actionObject.objectID);
        }
    }

    [RenderActionProcessor(typeof(BallEnergyLevelChangedAction))]
    public static class BallEnergyLevelChangedActionProcessor
    {
        public static void doDone(BallEnergyLevelChangedAction actionObject)
        {
            SceneViews.instance.getCurFBScene().ball.energyLevelChanged(actionObject.oldLV, actionObject.newLV);
        }
    }

    [RenderActionProcessor(typeof(BallPassAction))]
    public static class BallPassActionProcessor
    {
        public static void doDone(BallPassAction actionObject)
        {
#if ARTIEST_MODE
        var scene = SceneViews.instance.getCurFBScene();
        var actor = scene.getActor(actionObject.objectID);
        LogicEvent.fire("editor_onActorPassBallOut", actor);
#else
            //SceneViews.instance.getCurFBScene().ball.showTrail(true);
#endif
            var scene = SceneViews.instance.getCurFBScene();
            scene.getActor(actionObject.objectID).pass();
            scene.ball.pass();
            scene.ball.setRotateType(actionObject.velocity);
        }
    }

    [RenderActionProcessor(typeof(BallShootAction))]
    public static class BallShootActionProcessor
    {
        public static void doDone(BallShootAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            var actor = scene.getActor(actionObject.objectID);
            var shootType = (ShootType)actionObject.shootType;
            actor.shoot(shootType);
            scene.ball.shoot(shootType,
                            actionObject.velocity,
                            actionObject.angle,
                            actionObject.target,
                            actor);

            if (shootType == ShootType.Killer)
            {
                scene.releaseFocus(actor);
            }
        }
    }


    [RenderActionProcessor(typeof(ChangeActorAnimatorSpeedAction))]
    public static class ChangeActorAnimatorSpeedActionProcessor
    {
        public static void doDone(ChangeActorAnimatorSpeedAction actionObject)
        {
            SceneViews.instance.getCurFBScene().getActor(actionObject.objectID).setAnimtorSpeed(actionObject.speed);
        }
    }

    [RenderActionProcessor(typeof(GameReadyAction))]
    public static class GameReadyActionProcessor
    {
        public static void doDone(GameReadyAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            scene.setActorsAnimatorController(false);
            scene.ball.hideEnergyEffect();
            scene.recordingAnimatorState = true;
            LogicEvent.fire2Lua("onGameReady");
        }
    }

    [RenderActionProcessor(typeof(GameBeginAction))]
    public static class GameBeginActionProcessor
    {
        public static void doDone(GameBeginAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            LogicEvent.fire2Lua("onGameBegin");
        }
    }

    [RenderActionProcessor(typeof(GoalAction))]
    public static class GoalActionProcessor
    {
        public static void doDone(GoalAction actionObject)
        {
            LogicEvent.fire2Rendering("onBallGoal", actionObject.team, actionObject.door, actionObject.objectID);
            LogicEvent.fire2Lua("onGoal", actionObject.door == Location.kLeftDoor);
            var scene = SceneViews.instance.getCurFBScene();
            scene.recordingAnimatorState = false;
            scene.setActorsAnimatorController(true);

        }
    }

    [RenderActionProcessor(typeof(GameOverAction))]
    public static class GameOverActionProcessor
    {
        public static void doDone(GameOverAction actionObject)
        {
            SceneViews.instance.getCurFBScene().setActorsAnimatorController(true);

        }
    }

    [RenderActionProcessor(typeof(ProfilerAction))]
    public static class ProfilerActionProcessor
    {
        public static void doDone(ProfilerAction actionObject)
        {
            LogicEvent.fire("onPingActionDone", actionObject.stamp);
        }
    }

    [RenderActionProcessor(typeof(SettlementAction))]
    public static class SettlementActionProcessor
    {
        public static void doDone(SettlementAction actionObject)
        {
            LogicEvent.fire2Lua("onSettlement", actionObject.winner);
        }
    }

    [RenderActionProcessor(typeof(ShowReplayLogoAction))]
    public static class ShowReplayLogoActionProcessor
    {
        public static void doDone(ShowReplayLogoAction actionObject)
        {
            LogicEvent.fire2Lua("onShowReplayLogo");
        }
    }

    [RenderActionProcessor(typeof(UpdateCountdownAction))]
    public static class UpdateCountdownActionProcessor
    {
        public static void doDone(UpdateCountdownAction actionObject)
        {
            LogicEvent.fire2Lua("onUpdateCountdown", actionObject.time);
        }
    }

    [RenderActionProcessor(typeof(UpdateMatchTimeAction))]
    public static class UpdateMatchTimeActionProcessor
    {
        public static void doDone(UpdateMatchTimeAction actionObject)
        {
            //Debuger.Log("UpdateMatchTime frame" + actionObject.frame + " time:" + actionObject.time);
            //SceneViews.instance.getCurFBScene().recordFrameWithTime(actionObject.frame, actionObject.time);
            LogicEvent.fire2Lua("onUpdateMatchTime", actionObject.time);
        }
    }

    [RenderActionProcessor(typeof(UpdateScoreAction))]
    public static class UpdateScoreActionProcessor
    {
        public static void doDone(UpdateScoreAction actionObject)
        {
            LogicEvent.fire2Lua("onUpdateScore", actionObject.blue, actionObject.red);
        }
    }

    [RenderActionProcessor(typeof(ReplayBeginAction))]
    public static class ReplayBeginActionProcessor
    {
        public static void doDone(ReplayBeginAction actionObject)
        {
            RealTimeRAProcessCenter.instance.controlCenter.beginReplay(
                            actionObject.beginFrame,
                            actionObject.endFrame);

            var scene = SceneViews.instance.getCurFBScene();
            scene.clearEntityStartSamplePosition();
            scene.setActorsAnimatorController(false);
            scene.restoreActorsAnimator((int)actionObject.beginFrame);
            scene.ball.hideTrailEffect();
            LogicEvent.fire2Lua("onBeginReplay",
                            (uint)actionObject.goaler,
                            actionObject.blueScore,
                            actionObject.redScore,
                            actionObject.goalTime,
                            actionObject.replayTime,
                            (byte)actionObject.goalTeam);
        }
    }


    [RenderActionProcessor(typeof(ReplayEndAction))]
    public static class ReplayEndActionProcessor
    {
        public static void doDone(ReplayEndAction actionObject)
        {
            RealTimeRAProcessCenter.instance.controlCenter.endReplay();
            LogicEvent.fire2Lua("onEndReplay");
        }
    }

    [RenderActionProcessor(typeof(BallLandedAction))]
    public static class BallLandedActionProcessor
    {
        public static void doDone(BallLandedAction actionObject)
        {
            var ball = SceneViews.instance.getCurFBScene().ball;
            if (actionObject.times == 1)
            {
                ball.hitLand(new Vector3 { x = actionObject.point.x, z = actionObject.point.y }, actionObject.preHeightVelocity);
            }
            ball.setRotateType(actionObject.velocity);

        }
    }

    [RenderActionProcessor(typeof(BeginHitAction))]
    public static class BeginHitActionProcessor
    {
        public static void doDone(BeginHitAction actionObject)
        {
            var scene = SceneViews.instance.getCurFBScene();
            LogicEvent.fire2Rendering("onBeginHit", scene.getActor(actionObject.objectID), scene.getActor(actionObject.victim));
        }
    }

    [RenderActionProcessor(typeof(EndHitAction))]
    public static class EndHitActionProcessor
    {
        public static void doDone(EndHitAction actionObject)
        {
            LogicEvent.fire2Rendering("onEndHit");
        }
    }

    [RenderActionProcessor(typeof(HitCompletedAction))]
    public static class HitCompletedActionProcessor
    {
        public static void doDone(HitCompletedAction actionObject)
        {
            LogicEvent.fire2Rendering("onHitCompleted");
        }
    }
}
