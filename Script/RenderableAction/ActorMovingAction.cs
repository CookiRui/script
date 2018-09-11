using UnityEngine;
using Cratos;

namespace RAL
{
    public class ActorMovingAction : ContinuousAction
    {
        public void init(uint id, Vector2 targetPosition, bool ignoreTime, uint moveType)
        {
            this.objectID = id;
            this.moveType = moveType;
            this.targetPosition = targetPosition;
            this.ignoreTimeAction = ignoreTime;
        }
        public Vector2 targetPosition;      //终点位置

        public override uint objectID
        {
            get { return _objectID.getBit(0, 3); }
            set { _objectID.setBit(0, 3, value); }
        }

        public bool ignoreTimeAction
        {
            get { return _objectID.getBit(7) == 1; }
            set
            {
                _objectID.setBit(7, value ? 1 : 0);
            }
        }

        //移动类型0 正常移动 1防御向前 2防御向后 3防御向左 4防御向右
        public uint moveType
        {
            get { return _objectID.getBit(4, 6); }
            set
            {
                _objectID.setBit(4, 6, value);
            }
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(targetPosition);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            targetPosition = stream.readVector2();
        }
    }


    public class ActorMovingAction3D : ContinuousAction
    {
        public Vector3 targetPosition;      //终点位置

        public void init(uint id, Vector3 position, bool ignoreTime, uint moveType )
        {
            this.objectID = id;
            this.ignoreTimeAction = ignoreTime;
            this.moveType = moveType;
            this.targetPosition = position;

        }

        public override uint objectID
        {
            get { return _objectID.getBit(0, 3); }
            set { _objectID.setBit(0, 3, value); }
        }

        public bool ignoreTimeAction
        {
            get { return _objectID.getBit(7) == 1; }
            set
            {
                _objectID.setBit(7, value ? 1 : 0);
            }
        }

        //移动类型0 正常移动 1防御向前 2防御向后 3防御向左 4防御向右
        public uint moveType
        {
            get { return _objectID.getBit(4, 6); }
            set
            {
                _objectID.setBit(4, 6, value);
            }
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(targetPosition);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            targetPosition = stream.readVector3();
        }
    }
}