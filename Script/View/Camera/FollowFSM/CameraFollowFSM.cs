
using System.Collections.Generic;
using UnityEngine;
namespace FBCamera
{
    public enum CameraFollowState
    {
        LookAtPlayer,
        WidthoutBall,
        KeepingBall,
        Charging,
        Shooting,
        KillerSkill,
        HitIn,
        HitOut,
    }

    class CameraFollowFSM
    {
        public bool useSmooth { private get; set; }
        Dictionary<CameraFollowState, CameraFollowStateBase> states = new Dictionary<CameraFollowState, CameraFollowStateBase>();
        public CameraFollowState curStateType { get; private set; }
        CameraFollowStateBase curState;

        CameraCtrl cameraCtrl;
        public CameraFollowFSM(CameraCtrl ctrl)
        {
            cameraCtrl = ctrl;
            useSmooth = true;
        }

        Vector3 smooth(Vector3 pos,float tanAngle)
        {
            var targetPos = pos;
            if (curState.smoothXZ)
            {
                var smoothPos = smoothXZ(pos, tanAngle);
                targetPos.x = smoothPos.x;
                targetPos.z = smoothPos.z;
            }

            if (curState.smoothY)
            {
                targetPos.y = smoothY(pos);
            }
            return targetPos;
        }

        Vector3 smoothXZ(Vector3 pos, float tanAngle)
        {
            var currentXZ = new Vector3 { x = cameraCtrl.transform.position.x, z = cameraCtrl.transform.position.z + (cameraCtrl.transform.position.y / tanAngle) };
            var targetXZ = new Vector3 { x = pos.x, z = pos.z };
            Vector3 lerpXZ;
            var xzDistance = Vector3.Distance(currentXZ, targetXZ);
            if (xzDistance < 0.001f)
            {
                lerpXZ = targetXZ;
            }
            else
            {
                //jlx2017.05.10-log:速度计算公式 velocity = a + b * distance
                float xzVelocity;
                if (cameraCtrl.swithXZVelocityCompleted
                    && xzDistance > cameraCtrl.config.xzDistanceThreshold)
                {
                    xzVelocity = curState.xzVelocity + cameraCtrl.config.xzConstB * xzDistance;
                }
                else
                {
                    xzVelocity = curState.xzVelocity;
                }
                lerpXZ = Vector3.Lerp(currentXZ, targetXZ, xzVelocity * Time.deltaTime);
            }
            return lerpXZ;
        }

        float smoothY(Vector3 pos)
        {
            var currentY = cameraCtrl.transform.position.y;
            var targetY = pos.y;
            float lerpY;
            var yDistance = Mathf.Abs(targetY - currentY);
            if (yDistance < 0.001f)
            {
                lerpY = targetY;
            }
            else
            {
                float yVelocity;
                if (yDistance > cameraCtrl.config.yDistanceThreshold)
                {
                    yVelocity = curState.yVelocity + curState.yVelocityConstB * yDistance;
                }
                else
                {
                    yVelocity = curState.yVelocity;
                }
                lerpY = Mathf.Lerp(currentY, targetY, yVelocity * Time.deltaTime);
            }
            return lerpY;
        }

        CameraFollowStateBase getState(CameraFollowState type)
        {
            CameraFollowStateBase state;
            if (states.TryGetValue(type, out state))
            {
                return state;
            }

            switch (type)
            {
                case CameraFollowState.LookAtPlayer: state = new LookAtPlayerState(cameraCtrl); break;
                case CameraFollowState.WidthoutBall: state = new WithoutBallState(cameraCtrl); break;
                case CameraFollowState.KeepingBall: state = new KeepingBallState(cameraCtrl); break;
                case CameraFollowState.Charging: state = new ChargingState(cameraCtrl); break;
                case CameraFollowState.Shooting: state = new ShootingState(cameraCtrl); break;
                case CameraFollowState.KillerSkill: state = new KillerSkillState(cameraCtrl); break;
                case CameraFollowState.HitIn: state = new HitInState(cameraCtrl); break;
                case CameraFollowState.HitOut: state = new HitOutState(cameraCtrl); break;
            }
            states.Add(type, state);
            return state;
        }

        public void execute()
        {
            if (curState == null) return;
            var pos = curState.calculatePosition();
            if (!curState.directlyPosition)
            {
                var tanAngle = Mathf.Tan(cameraCtrl.transform.localEulerAngles.x * Mathf.Deg2Rad);
                if (useSmooth)
                {
                    pos = smooth(pos, tanAngle);
                }
                pos = new Vector3 { x = pos.x, y = pos.y, z = pos.z - (pos.y / tanAngle) };
            }
            cameraCtrl.transform.position = pos;
            cameraCtrl.transform.eulerAngles = curState.calculateAngles();
        }

        public void changeState(CameraFollowState state)
        {
            //Debuger.Log("changeState " + state);
            if (curStateType == state && curState != null) return;

            if (curState != null)
            {
                curState.exit();
            }
            curState = getState(state);
            curState.enter();
            curStateType = state;
        }

        public void clear()
        {
            curState = null;
        }

        public Vector3 calculateCurPosition()
        {
            if (curState == null) return cameraCtrl.transform.position;
            return curState.calculatePosition();
        }
    }
}