using System;
using UnityEngine;

namespace FBCamera
{
    class KillerSkillState : CameraFollowStateBase
    {
        GameObject cameraAniamtionRoot;
        Animation cameraAnimation;
        Transform tracker;

        Coroutine playCoroutine;

        public KillerSkillState(CameraCtrl ctrl) : base(ctrl) { }
        public override bool directlyPosition { get { return true; } }

        public override void enter()
        {
            var animationName = cameraCtrl.shooter.name + "_shoot_ultra_camera";
            ModelResourceLoader.inst.loadAnimation(cameraCtrl.shooter.name, animationName, a =>
            {
                if (a == null)
                {
                    Debug.LogError(animationName + "  为空！检查角色资源包！！！！！");
                    return;
                }
                cameraAniamtionRoot = new GameObject("FocusShooterCameraAnimationWTF");
                cameraAniamtionRoot.transform.SetParent(cameraCtrl.shooter.transform, false);
                cameraAnimation = new GameObject().AddComponent<Animation>();
                cameraAnimation.transform.SetParent(cameraAniamtionRoot.transform);

                tracker = new GameObject().transform;
                tracker.SetParent(cameraAnimation.transform, false);
                tracker.localEulerAngles = new Vector3 { y = 180 };
                cameraAnimation.AddClip(a, animationName);
                playCoroutine = cameraCtrl.StartCoroutine(cameraAnimation.play(animationName));
            });
        }

        public override void exit()
        {
            if (playCoroutine != null)
            {
                cameraCtrl.StopCoroutine(playCoroutine);
                playCoroutine = null;
            }
            if (cameraAnimation != null)
            {
                UnityEngine.Object.Destroy(cameraAniamtionRoot);
            }
        }

        public override Vector3 calculatePosition()
        {
            if (cameraAnimation == null) return cameraCtrl.transform.position;
            if (cameraAnimation.isPlaying)
            {
                cameraCtrl.cam.fieldOfView = cameraAnimation.transform.localScale.z * cameraCtrl.config.defaultFOV;
                return tracker.position;
            }
            UnityEngine.Object.Destroy(cameraAniamtionRoot);
            return cameraCtrl.transform.position;
        }

        public override Vector3 calculateAngles()
        {
            if (cameraAnimation == null || !cameraAnimation.isPlaying) return cameraCtrl.transform.eulerAngles;
            return tracker.eulerAngles;
        }

        protected override Vector3 getLookAtPos() { throw new NotImplementedException(); }

        protected override float calculateY() { throw new NotImplementedException(); }

        protected override float calculateAngle() { throw new NotImplementedException(); }
    }
}