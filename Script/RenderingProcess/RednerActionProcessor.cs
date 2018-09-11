using UnityEngine;
using RAL;
namespace RenderingProcess 
{
    public static class MovingProcessorUtility
    {
        //根据时长获取应该移动位置
        public static Vector3 getMovePosition(Vector3? startPosition, Vector2 targetPosition, float percent)
        {
            if (startPosition.HasValue)
            {
                Vector3 moveDest = new Vector3();
                var start = startPosition.Value;
                moveDest.x = (targetPosition.x - start.x) * percent + start.x;
                moveDest.z = (targetPosition.y - start.z) * percent + start.z;
                moveDest.y = 0.0f;
                return moveDest;
            }
            else
            {
                return new Vector3(targetPosition.x, 0.0f, targetPosition.y);
            }
        }

        //根据时长获取应该移动位置
        public static Vector3 getMovePosition(Vector3? startPosition, Vector3 targetPosition, float percent)
        {
            if (startPosition.HasValue)
            {
                Vector3 moveDest = new Vector3();
                var start = startPosition.Value;
                moveDest.x = (targetPosition.x - start.x) * percent + start.x;
                moveDest.y = (targetPosition.y - start.y) * percent + start.y;
                moveDest.z = (targetPosition.z - start.z) * percent + start.z;
                return moveDest;
            }
            else
            {
                return new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
            }
        }


    }

    [RenderActionProcessorAttribute(typeof(TurningAction))]
    public static class TurningActionProcessor
    {
        static Quaternion getRotatingDirection(Quaternion start, Quaternion end, float progress)
        {
            return Quaternion.SlerpUnclamped(start, end, progress);
        }
        public static void doProgress(TurningAction actionObject, float progress)
        {
            if (actionObject.ignoreTimeAction)
                return;

            ActorView entity = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);

            Quaternion start = Quaternion.LookRotation(new Vector3(actionObject.startDirection.x, 0, actionObject.startDirection.y));
            Quaternion end = Quaternion.LookRotation(new Vector3(actionObject.endDirection.x, 0, actionObject.endDirection.y));

            //获取本次移动位置
            Quaternion thisEndRotation = getRotatingDirection(start, end, progress);

            entity.setRotation(thisEndRotation);
        }
        public static void doDone(TurningAction actionObject)
        {
            ActorView entity = SceneViews.instance.getCurFBScene().getActor(actionObject.objectID);

            Quaternion end = Quaternion.LookRotation(new Vector3(actionObject.endDirection.x, 0, actionObject.endDirection.y));

            entity.setRotation(end);
        }
    };





    [RenderActionProcessorAttribute(typeof(ActorMovingAction))]
    public static class ActorMovingActionProcessor
    {
        public static void doProgress(ActorMovingAction actionObject, float progress)
        {
            if (actionObject.ignoreTimeAction)
                return;

            ActorView entity = SceneViews.instance.getCurFBScene().getSceneEntity<ActorView>(actionObject.objectID);

            if (entity.startSamplePosition.HasValue)
            {
                if (entity.isRunState())
                {
                    var speed = (new Vector3(actionObject.targetPosition.x, 0.0f, actionObject.targetPosition.y) - entity.startSamplePosition.Value).magnitude * 60.0f;
                    entity.updateAnimtorSpeed(speed, actionObject.moveType);
                    entity.animator.SetFloat("moveType", (float)actionObject.moveType);
                    entity.showRunEffect();
                }
            }

            //获取本次移动位置
            Vector3 thisEndPostion = MovingProcessorUtility.getMovePosition(entity.startSamplePosition, actionObject.targetPosition, progress);
            entity.setPosition(thisEndPostion);
        }
        public static void doDone(ActorMovingAction actionObject)
        {
            EntityView entity = SceneViews.instance.getCurFBScene().getSceneEntity<EntityView>(actionObject.objectID);
            entity.setPosition(actionObject.targetPosition);
            entity.startSamplePosition = new Vector3(actionObject.targetPosition.x, 0.0f, actionObject.targetPosition.y);
        }
    };



    [RenderActionProcessorAttribute(typeof(ActorMovingAction3D))]
    public static class ActorMovingAction3DProcessor
    {
        public static void doProgress(ActorMovingAction3D actionObject, float progress)
        {
            if (actionObject.ignoreTimeAction)
                return;

            ActorView entity = SceneViews.instance.getCurFBScene().getSceneEntity<ActorView>(actionObject.objectID);

            if (entity.startSamplePosition.HasValue)
            {
                if (entity.isRunState())
                {
                    var speed = (actionObject.targetPosition - entity.startSamplePosition.Value).magnitude * 60.0f;
                    entity.updateAnimtorSpeed(speed, actionObject.moveType);
                    entity.animator.SetFloat("moveType", (float)actionObject.moveType);
                    entity.showRunEffect();
                }
            }

            //获取本次移动位置
            Vector3 thisEndPostion = MovingProcessorUtility.getMovePosition(entity.startSamplePosition, actionObject.targetPosition, progress);
            entity.setPosition(thisEndPostion);
        }
        public static void doDone(ActorMovingAction3D actionObject)
        {
            EntityView entity = SceneViews.instance.getCurFBScene().getSceneEntity<EntityView>(actionObject.objectID);
            entity.setPosition(actionObject.targetPosition);
            entity.startSamplePosition = actionObject.targetPosition;
        }
    };



    [RenderActionProcessorAttribute(typeof(BallMovingAction))]
    public static class BallMovingActionProcessor
    {
        public static void doProgress(BallMovingAction actionObject, float progress)
        {
            BallView entity = SceneViews.instance.getCurFBScene().ball;
            //获取本次移动位置
            Vector3 thisEndPostion = MovingProcessorUtility.getMovePosition(entity.startSamplePosition, actionObject.targetPosition, progress);
            entity.setPosition(thisEndPostion);
        }
        public static void doDone(BallMovingAction actionObject)
        {
            BallView entity = SceneViews.instance.getCurFBScene().ball;
            entity.setPosition(actionObject.targetPosition);
            entity.startSamplePosition = actionObject.targetPosition;
        }
    }


    [RenderActionProcessorAttribute(typeof(InstantBallMovingAction))]
    public static class InstantBallMovingActionProcessor
    {
        public static void doDone(InstantBallMovingAction actionObject)
        {
            BallView entity = SceneViews.instance.getCurFBScene().ball;
            entity.setPosition(actionObject.targetPosition);
            entity.startSamplePosition = actionObject.targetPosition;
        }
    }


    [RenderActionProcessorAttribute(typeof(BallSlerpMoveAction))]
    public static class BallSlerpMoveActionProcessor
    {
        public static void doDone(BallSlerpMoveAction actionObject)
        {
            SceneViews.instance.getCurFBScene().ball.slerp(actionObject.target, actionObject.totoalSlerpTime);
        }
    }




}
