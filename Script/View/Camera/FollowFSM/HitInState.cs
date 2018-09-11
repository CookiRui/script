using UnityEngine;
namespace FBCamera
{
    class HitInState : CameraFollowStateBase
    {
        float acceleration;
        float t;
        float y;
        float timer;
        Vector3 lookAtPos;

        public HitInState(CameraCtrl ctrl) : base(ctrl)
        {
            t = cameraCtrl.config.hitInTime;
        }

        public override void enter()
        {
            base.enter();
            cameraCtrl.hitStartY = startY;
            y = startY - cameraCtrl.config.hitYMin;
            acceleration = y / (t * 0.5f * t * 0.5f);
            timer = 0;
            cameraCtrl.swithXZVelocityCompleted = false;
        }

        public override void exit()
        {
            base.exit();
            cameraCtrl.attacker = null;
            cameraCtrl.victim = null;
        }

        public override bool smoothXZ { get { return true; } }
        public override bool smoothY { get { return false; } }
        public override float xzVelocity { get { return cameraCtrl.config.hitInXZVelocity; } }

        protected override Vector3 getLookAtPos()
        {
            return viewport2WorldPoint((getActorViewport(cameraCtrl.attacker) + getActorViewport(cameraCtrl.victim)) * 0.5f);
        }

        protected override float calculateAngle()
        {
            var t = 0f;
            if (y == 0)
            {
                t = 0.1f;
            }
            else
            {
                t = (startY - cameraCtrl.transform.position.y) / y;
            }
            return Mathf.Lerp(startAngle, cameraCtrl.config.hitAngle, t);
        }

        protected override float calculateY()
        {
            var targetY = 0f;
            if (timer < t * 0.5f)
            {
                targetY = startY - 0.5f * acceleration * timer * timer;
            }
            else
            {
                targetY = cameraCtrl.config.hitYMin + 0.5f * acceleration * (t - timer) * (t - timer);
            }
            timer += Time.unscaledDeltaTime;
            return targetY;
        }

        protected override float clampY(float y) { return Mathf.Clamp(y, cameraCtrl.config.hitYMin, cameraCtrl.config.yMaxBorder); }

        protected override float clampX(float x) { return x; }
    }
}