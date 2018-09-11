using UnityEngine;
namespace FBCamera
{
    class ChargingState : CameraFollowStateBase
    {
        float acceleration;
        float t;
        float y;
        float timer;
        Vector3 lookAtPos;

        public ChargingState(CameraCtrl ctrl) : base(ctrl)
        {
            t = cameraCtrl.config.chargingFallTime;
        }

        public override void enter()
        {
            base.enter();
            y = startY - cameraCtrl.config.chargingYMin;
            acceleration = y / (t * 0.5f * t * 0.5f);
            timer = 0;

            if (!rayViewport2WorldPoint(Vector2.one * 0.5f, out lookAtPos))
            {
                Debug.LogError("ChargingState getLookAtPos 没有交点");
            }
        }
        public override bool smoothXZ { get { return smoothY; } }
        public override bool smoothY { get { return false; } }

        protected override Vector3 getLookAtPos()
        {
            return lookAtPos;
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
            //Debug.LogWarning("Angle " + Mathf.Lerp(startAngle, cameraCtrl.config.chargingAngle, t));
            return Mathf.Lerp(startAngle, cameraCtrl.config.chargingAngle, t);
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
                targetY = cameraCtrl.config.chargingYMin + 0.5f * acceleration * (t - timer) * (t - timer);
            }
            timer += Time.deltaTime;
            // Debug.LogWarning(timer + "   " + targetY + "    " + cameraCtrl.transform.position.z);
            return targetY;
        }

        protected override float clampY(float y)
        {
            return Mathf.Clamp(y, cameraCtrl.config.chargingYMin, cameraCtrl.config.yMaxBorder);
        }

        protected override float clampX(float x)
        {
            return x;
        }

    }
}