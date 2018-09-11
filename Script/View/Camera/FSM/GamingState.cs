using BW31.SP2D;
using FixMath.NET;
using System.Collections;
using UnityEngine;

partial class CameraCtrl
{
    ActorView m_player;
    public ActorView player
    {
        get
        {
            if (m_player == null)
            {
                m_player = SceneViews.instance.getCurFBScene().getMainActor();
                if (m_player == null)
                {
                    m_player = SceneViews.instance.getCurFBScene().getActor(1);
                }
                if (m_player != null)
                {
                    doorPosition = SceneViews.instance.getCurFBScene().getEnemyDoorPosition(m_player.team);
                    doorOnLeft = m_player.team == FBTeam.kBlue;
                }

            }
            return m_player;
        }

    }
    public bool doorOnLeft { get; private set; }
    public float xzVelocity { get; set; }
    public Vector3 doorPosition { get; private set; }
    public bool swithXZVelocityCompleted { get; set; }
    public CampType camp { get; set; }
    public ActorView shooter { get; set; }
    public ActorView attacker { get; set; }
    public ActorView victim { get; set; }
    public float hitStartY { get; set; }
}

namespace FBCamera
{
    class GamingState : CameraStateBase
    {
        CameraFollowFSM fsm;
        Coroutine switchXZVelocityCoroutine;
        Coroutine chargeCoroutine;

        public GamingState(CameraCtrl ctrl) : base(ctrl)
        {
            fsm = new CameraFollowFSM(ctrl);
        }

        public override void enter()
        {
            base.enter();
            LogicEvent.add("onOwnerAttached", this, "onOwnerAttached");
            LogicEvent.add("onOwnerDetached", this, "onOwnerDetached");
            LogicEvent.add("onShootBallBegin", this, "onShootBallBegin");
            LogicEvent.add("onShootBallOut", this, "onShootBallOut");
            LogicEvent.add("onBallGoal", this, "onBallGoal");
            LogicEvent.add("onBallCollidedWall", this, "onBallCollidedWall");
            LogicEvent.add("onBallLanded", this, "onBallLanded");
            LogicEvent.add("onBeginKillerSkill", this, "onBeginKillerSkill");
            LogicEvent.add("onEndKillerSkill", this, "onEndKillerSkill");
            LogicEvent.add("onBeginHit", this, "onBeginHit");
            LogicEvent.add("onEndHit", this, "onEndHit");
            LogicEvent.add("onHitCompleted", this, "onHitCompleted");
            cameraCtrl.StartCoroutine(delayEnter());
        }

        public override void execute()
        {
            fsm.execute();
        }

        public override void exit()
        {
            base.exit();
            fsm.clear();
            stopSwitchXZVelocityCoroutine();
            fsm.useSmooth = true;
        }

        #region private methods

        IEnumerator delayEnter()
        {
            yield return new WaitForSeconds(0.1f);
            reposition();
            cameraCtrl.xzVelocity = cameraCtrl.config.xzVelocityMax;
            fsm.useSmooth = false;
            fsm.changeState(CameraFollowState.WidthoutBall);
            yield return new WaitForSeconds(1f);
            fsm.useSmooth = true;
        }


        void reposition()
        {
            //cameraCtrl.transform.position = cameraCtrl.config.defaultPos;
            cameraCtrl.transform.localEulerAngles = new Vector3 { x = cameraCtrl.config.defaultAngle };
            cameraCtrl.cam.fieldOfView = cameraCtrl.config.defaultFOV;
        }

        IEnumerator switchXZVelocity()
        {
            cameraCtrl.swithXZVelocityCompleted = false;
            var timer = 0f;
            while (timer < cameraCtrl.config.switchDuration)
            {
                cameraCtrl.xzVelocity = Mathf.Lerp(cameraCtrl.config.xzVelocityMin,
                                                    cameraCtrl.config.xzVelocityMax,
                                                    timer / cameraCtrl.config.switchDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            cameraCtrl.xzVelocity = cameraCtrl.config.xzVelocityMax;
            switchXZVelocityCoroutine = null;
            cameraCtrl.swithXZVelocityCompleted = true;
        }

        IEnumerator delayCharge(float delay)
        {
            yield return new WaitForSeconds(delay);
            fsm.changeState(CameraFollowState.Charging);
            chargeCoroutine = null;
        }

        bool isFollow(uint id)
        {
            return SceneViews.instance.getCurFBScene().mainActorFrameID == id;
        }

        void endShoot()
        {
            if (fsm.curStateType != CameraFollowState.Shooting) return;

            fsm.changeState(CameraFollowState.WidthoutBall);
            cameraCtrl.xzVelocity = cameraCtrl.config.xzVelocityMax;
        }

        void stopSwitchXZVelocityCoroutine()
        {
            if (switchXZVelocityCoroutine != null)
            {
                cameraCtrl.StopCoroutine(switchXZVelocityCoroutine);
                switchXZVelocityCoroutine = null;
            }
        }

        void stopCharge()
        {
            if (chargeCoroutine != null)
            {
                cameraCtrl.StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }
        }
        #endregion

        #region events

        void onShootBallBegin(uint id, int kickBallFoot)
        {
            if (isFollow(id))
            {
                chargeCoroutine = cameraCtrl.StartCoroutine(delayCharge(cameraCtrl.config.chargingDelay));
            }
        }

        void onShootBallOut(uint id)
        {
            stopSwitchXZVelocityCoroutine();
            cameraCtrl.xzVelocity = cameraCtrl.config.xzVelocityMax;
            cameraCtrl.swithXZVelocityCompleted = true;

            if (isFollow(id))
            {
                stopCharge();
                fsm.changeState(CameraFollowState.Shooting);
            }
        }

        void onOwnerAttached(uint id, bool gk)
        {
            if (isFollow(id))
            {
                fsm.changeState(CameraFollowState.KeepingBall);
            }

            endShoot();
            var actor = SceneViews.instance.getCurFBScene().getActor(id);
            if (cameraCtrl.player.team == actor.team)
            {
                if (cameraCtrl.camp == CampType.Attack)
                {
                    return;
                }
                cameraCtrl.camp = CampType.Attack;
            }
            else
            {
                if (cameraCtrl.camp == CampType.Defence)
                {
                    return;
                }
                cameraCtrl.camp = CampType.Defence;
            }
            if (switchXZVelocityCoroutine != null)
            {
                cameraCtrl.StopCoroutine(switchXZVelocityCoroutine);
            }
            switchXZVelocityCoroutine = cameraCtrl.StartCoroutine(switchXZVelocity());
        }

        void onOwnerDetached(uint id)
        {
            if (isFollow(id))
            {
                stopCharge();
                fsm.changeState(CameraFollowState.WidthoutBall);
            }
        }

        void onBallGoal(FBTeam team, Location door, uint id)
        {
            endShoot();
            cameraCtrl.goaler = id;
            cameraCtrl.fsm.changeState(GameState.Goal);
        }

        void onBallCollidedWall(FixVector3 point, FixVector3 normal, FixVector3 velocity, FiveElements kickerElement)
        {
            endShoot();
        }

        void onBallLanded(FixVector2 point, bool pass, int times, FixVector3 velocity, Fix64 preHeightVelocity)
        {
            if (times == 1)
            {
                endShoot();
            }
        }

        void onBeginKillerSkill(ActorView actor)
        {
            if (actor == null)
            {
                Debug.LogError("actor is null");
                return;
            }
            cameraCtrl.shooter = actor;
            fsm.changeState(CameraFollowState.KillerSkill);
        }

        void onEndKillerSkill(ActorView actor)
        {
            if (actor == null)
            {
                Debug.LogError("actor is null");
                return;
            }
            if (isFollow(actor.id))
            {
                fsm.changeState(CameraFollowState.Shooting);
            }
            else
            {
                fsm.changeState(CameraFollowState.WidthoutBall);
            }
        }

        void onBeginHit(ActorView attacker, ActorView victim)
        {
            if (attacker == null)
            {
                Debug.LogError("attacker is null");
                return;
            }
            if (victim == null)
            {
                Debug.LogError("victim is null");
                return;
            }

            cameraCtrl.attacker = attacker;
            cameraCtrl.victim = victim;
            fsm.changeState(CameraFollowState.HitIn);
        }

        void onEndHit()
        {
            fsm.changeState(CameraFollowState.HitOut);
        }

        void onHitCompleted()
        {
            fsm.changeState(CameraFollowState.WidthoutBall);
        }

        #endregion
    }
}