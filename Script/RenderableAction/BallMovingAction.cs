using UnityEngine;
using Cratos;

namespace RAL
{
    //可移动动作
    public class BallMovingAction : ContinuousAction
    {
        public Vector3 targetPosition;      //终点位置

        public void init(Vector3 position)
        {
            this.targetPosition = position;
        }

        public override void serialize(BytesStream stream)
        {
            stream.write(targetPosition);

        }
        public override void unserialize(BytesStream stream)
        {
            targetPosition = stream.readVector3();
        }
    };

    public class InstantBallMovingAction : BallMovingAction { }


    public class BallSlerpMoveAction : InstantAction
    {
        public Vector3 target;
        public float totoalSlerpTime;

        public void init(Vector3 target, float totalSlerp)
        {
            this.target = target;
            this.totoalSlerpTime = totalSlerp;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(target);
            stream.Write(totoalSlerpTime);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            target = stream.readVector3();
            totoalSlerpTime = stream.ReadSingle();
        }

    }
}