using UnityEngine;
using System.Collections;
using FixMath.NET;
using DG.Tweening;

namespace FBCamera
{
    class EnterState : CameraStateBase
    {
        //readonly uint seldId = 2;
        //readonly uint enemyId = 5;

        Tweener lookAtTweener;
        float lookAtTime;
        Animation cameraAnimation;
        Transform animationCameraTransform;

        public EnterState(CameraCtrl ctrl) : base(ctrl) { }

        public override void enter()
        {
            base.enter();
            LogicEvent.add("onShowEnemy", this, "onShowEnemy");
            //jlx 2017.06.22-log:查找Tag为CameraRoot的物体
            ModelResourceLoader.inst.loadModel("EnterCameraAnimation", go =>
            {
                go.transform.localEulerAngles = Vector3.zero;
                cameraAnimation = go.GetComponent<Animation>();
                animationCameraTransform = go.transform.Find("Camera");
            });
        }

        public override void execute()
        {
            base.execute();
            if (cameraAnimation != null)
            {
                if (cameraAnimation.isPlaying)
                {
                    cameraCtrl.transform.position = animationCameraTransform.position;
                    cameraCtrl.transform.eulerAngles = animationCameraTransform.eulerAngles;
                    cameraCtrl.transform.Rotate(new Vector3 { y = 180 }, Space.Self);
                    cameraCtrl.cam.fieldOfView = animationCameraTransform.localScale.z * cameraCtrl.config.defaultFOV;
                }
                else
                {
                    Object.Destroy(cameraAnimation.transform.root.gameObject);
                    cameraCtrl.cam.DOFieldOfView(cameraCtrl.config.defaultFOV, cameraCtrl.config.showEnemyMoveTime);
                }
                return;
            }

            if (lookAtTweener != null && lookAtTime > 0)
            {
                lookAtTweener.ChangeEndValue(curLookAtPosition, lookAtTime, true);
                lookAtTime -= Time.deltaTime;
            }
        }

        public override void exit()
        {
            base.exit();
            if (cameraAnimation != null)
            {
                Object.Destroy(cameraAnimation.transform.root.gameObject);
            }
        }

        void onShowEnemy(uint id)
        {
            if (cameraAnimation != null)
            {
                repositionCamera();
            }
            var enemy = SceneViews.instance.getCurFBScene().getActor(id);
            var position = calculateOffsetPosition(enemy, cameraCtrl.config.showActorOffset);
            lookAtTime = cameraCtrl.config.showEnemyMoveTime;
            cameraCtrl.transform.DOMove(position, lookAtTime)
                .OnStart(() =>
                {
                    curLookAtPosition = enemy.getCenterPosition();
                    lookAtTweener = cameraCtrl.transform.DOLookAt(curLookAtPosition, lookAtTime);
                })
                .OnComplete(() =>
                {
                    lookAtTweener.Kill();
                    lookAtTweener = null;
                    playEnemyDefiance(enemy);
                });
        }

        void playEnemyDefiance(ActorView enemy)
        {
            LogicEvent.fire("onPlayTaunt", enemy.id);
            var self = SceneViews.instance.getCurFBScene().getMainActor();
            around(enemy, self);
        }

        void around(ActorView enemy, ActorView self)
        {
            var enemyPosition = enemy.getCenterPosition();
            var selfPosition = self.getCenterPosition();
                
            curLookAtPosition = Vector3.Lerp(enemyPosition, selfPosition, cameraCtrl.config.aroundRatio);
            var matrix = Matrix4x4.TRS(curLookAtPosition, Quaternion.LookRotation((selfPosition - enemyPosition).normalized), Vector3.one);
            var targetPosition = matrix.MultiplyPoint(cameraCtrl.config.aroundOffset);
            lookAtTime = cameraCtrl.config.aroundMoveTime;
            cameraCtrl.transform.DOMove(targetPosition, lookAtTime)
                .SetDelay(cameraCtrl.config.enemyTauntTime)
                .OnStart(() =>
                {
                    lookAtTweener = cameraCtrl.transform.DOLookAt(curLookAtPosition, lookAtTime);
                })
                .OnComplete(() =>
                {
                    lookAtTweener.Kill();
                    lookAtTweener = null;
                    playSelfDefiance(self);
                });
        }

        void playSelfDefiance(ActorView self)
        {
            var targetPosition = calculateOffsetPosition(self, cameraCtrl.config.showActorOffset);
            lookAtTime = cameraCtrl.config.showSelfMoveTime;
            cameraCtrl.transform.DOMove(targetPosition, cameraCtrl.config.showSelfMoveTime)
                .SetDelay(cameraCtrl.config.aroundStayTime)
                .OnStart(() =>
                {
                    curLookAtPosition = self.getCenterPosition();
                    lookAtTweener = cameraCtrl.transform.DOLookAt(curLookAtPosition, lookAtTime);
                })
                .OnComplete(() =>
                {
                    lookAtTweener.Kill();
                    lookAtTweener = null;
                    LogicEvent.fire("onPlayTaunt", self.id);
                });
        }

        void repositionCamera()
        {
            cameraCtrl.transform.localPosition = cameraCtrl.config.startPosition;
            cameraCtrl.transform.localEulerAngles = new Vector3 { x = cameraCtrl.config.defaultAngle };
            cameraCtrl.cam.fieldOfView = cameraCtrl.config.defaultFOV;
            Object.Destroy(cameraAnimation.transform.root.gameObject);
        }
    }
}
