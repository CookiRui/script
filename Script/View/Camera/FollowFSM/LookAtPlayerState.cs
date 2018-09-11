using UnityEngine;

namespace FBCamera
{
    class LookAtPlayerState : CameraFollowStateBase
    {
        public LookAtPlayerState(CameraCtrl ctrl) : base(ctrl) { }
        protected override Vector3 getLookAtPos()
        {
            return cameraCtrl.player.getCenterPosition();
        }

        protected override float calculateY()
        {
            return cameraCtrl.config.yMinBorder;
        }

        protected override float calculateAngle()
        {
            return cameraCtrl.config.defaultAngle;
        }
    }
}