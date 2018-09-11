using UnityEngine;
namespace FBCamera
{
    class HitOutState : CameraFollowStateBase
    {
        float acceleration;
        float t;
        float y;
        float timer;
        Vector3 lookAtPos;

        public HitOutState(CameraCtrl ctrl) : base(ctrl)
        {
            t = cameraCtrl.config.hitOutTime;
        }

        public override void enter()
        {
            base.enter();
            y = cameraCtrl.hitStartY - startY;
            acceleration = y / (t * 0.5f * t * 0.5f);
            timer = 0;
        }

        public override void exit()
        {
            base.exit();
            cameraCtrl.swithXZVelocityCompleted = true;
        }

        public override bool smoothXZ { get { return true; } }
        public override bool smoothY { get { return false; } }
        public override float xzVelocity { get { return cameraCtrl.config.hitOutXZVelocity; } }

        protected override Vector3 getLookAtPos()
        {
            return viewport2WorldPoint((getPlayerViewport() + getBallViewport()) * 0.5f + calculateLookAtOffset());
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
                t = (cameraCtrl.transform.position.y - startY) / y;
            }
            return Mathf.Lerp(startAngle, cameraCtrl.config.defaultAngle, t);
        }

        protected override float calculateY()
        {
            var targetY = 0f;
            if (timer < t * 0.5f)
            {
                targetY = startY + 0.5f * acceleration * timer * timer;
            }
            else
            {
                targetY = cameraCtrl.hitStartY - 0.5f * acceleration * (t - timer) * (t - timer);
            }
            timer += Time.unscaledDeltaTime;
            return targetY;
        }

        protected override float clampY(float y)
        {
            return Mathf.Clamp(y, startY, cameraCtrl.config.yMaxBorder);
        }

        protected override float clampX(float x)
        {
            return x;
        }
    }
}