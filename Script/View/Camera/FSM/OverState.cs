using DG.Tweening;

namespace FBCamera
{
    class OverState : CameraStateBase
    {
        public OverState(CameraCtrl ctrl) : base(ctrl) { }
        public override void enter()
        {
            base.enter();
            cameraCtrl.transform.DOMove(cameraCtrl.config.overPosition, cameraCtrl.config.overMoveTime);
            cameraCtrl.cam.DOFieldOfView(cameraCtrl.config.defaultFOV, cameraCtrl.config.overMoveTime);
        }
    }
}