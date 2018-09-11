using UnityEngine;
using Cratos;

namespace RAL
{
    public class BallPassAction : InstantAction
    {
        public Vector3 velocity;

        public void init(uint id, Vector3 velocity)
        {
            base.init(id);
            this.velocity = velocity;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(velocity);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            velocity = stream.readVector3();
        }
    }


    public class BallShootAction : InstantAction
    {
        public Vector3 velocity;
        public float angle;
        public Vector3 target;

        public void init(uint id, ShootType type, Vector3 velocity, float angle, Vector3 target)
        {
            base.init(id);
            shootType = (uint)type;
            this.velocity = velocity;
            this.angle = angle;
            this.target = target;
        }

        public override uint objectID
        {
            get { return _objectID.getBit(0, 3); }
            set { _objectID.setBit(0, 3, value); }
        }

        //普通射门or大力射门
        public uint shootType
        {
            get { return _objectID.getBit(4, 7); }
            set { _objectID.setBit(4, 7, value); }
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(velocity);
            stream.Write(angle);
            stream.write(target);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            velocity = stream.readVector3();
            angle = stream.ReadSingle();
            target = stream.readVector3();
        }

    }
}

