using System.Collections;
using UnityEngine;

namespace FBCamera
{
    abstract class CameraStateBase
    {
        protected CameraCtrl cameraCtrl { get; private set; }
        protected EntityView lookAtTarget { get; private set; }
        protected Vector3 curLookAtPosition;
        protected bool useLerpLookAt = true;
        public CameraStateBase(CameraCtrl ctrl)
        {
            cameraCtrl = ctrl;
        }

        public virtual void enter() { }
        public virtual void execute()
        {
            if (lookAtTarget != null)
            {
                var target = getLookAtPosition();
                var position = Vector3.zero;
                if (useLerpLookAt)
                {
                    position = Vector3.Lerp(curLookAtPosition, target, cameraCtrl.config.lookAtLerpSpeed * Time.deltaTime);
                }
                else
                {
                    position = target;
                }
                cameraCtrl.transform.LookAt(position);
                curLookAtPosition = position;
            }
        }
        public virtual void exit()
        {
            LogicEvent.remove(this);
            lookAtTarget = null;
            useLerpLookAt = true;
        }

        protected void setLookAtTarget(EntityView entityView, bool setCurLookAtPosition = true)
        {
            lookAtTarget = entityView;
            if (setCurLookAtPosition && entityView != null)
            {
                curLookAtPosition = entityView.getCenterPosition();
            }
        }

        protected Vector3 calculateOffsetPosition(ActorView actor, Vector3 baseOffset)
        {
            if (actor == null) return Vector3.zero;
            return actor.transform.TransformPoint(baseOffset * actor.height) + new Vector3 { y = actor.height * 0.5f };

        }

        protected IEnumerator delaySetUseLerpLookAt(bool value, float delay)
        {
            yield return new WaitForSeconds(delay);
            useLerpLookAt = value;
        }

        protected virtual Vector3 getLookAtPosition()
        {
            if (lookAtTarget == null) return Vector3.zero;
            return lookAtTarget.getCenterPosition();
        }
    }
}