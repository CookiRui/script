
using UnityEngine;

namespace FBCamera
{
    class ShootingState : WithoutBallState
    {
        bool checkDirect;
        public ShootingState(CameraCtrl ctrl) : base(ctrl) { }

        public override void enter()
        {
            base.enter();
            checkDirect = true;
        }

        protected override bool checkMinDistance { get { return false; } }

        protected override Vector3 getLookAtPos()
        {
            var lookAtPos = base.getLookAtPos();
            if (checkDirect)
            {
                if ((cameraCtrl.doorPosition.x > 0 && lookAtPos.x < cameraCtrl.transform.position.x)
                    || (cameraCtrl.doorPosition.x < 0 && lookAtPos.x > cameraCtrl.transform.position.x))
                {
                    return new Vector3
                    {
                        x = cameraCtrl.transform.position.x,
                        y = lookAtPos.y,
                        z = lookAtPos.z
                    };
                }
                checkDirect = false;
            }
            return lookAtPos;
        }
        protected override float calculateAngle()
        {
            var t = 0f;
            if (startY == cameraCtrl.config.yMinBorder)
            {
                t = 0.1f;
            }
            else
            {
                t = Mathf.Min((cameraCtrl.transform.position.y - startY) / (cameraCtrl.config.yMinBorder - startY), cameraCtrl.config.chargingAngleVelocityMax);
            }

            return Mathf.Lerp(startAngle, cameraCtrl.config.defaultAngle, t);
        }
    }
}