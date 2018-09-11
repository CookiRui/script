using UnityEngine;
using Cratos;

namespace RAL
{
    public class BallCollidedWallAction : InstantAction
    {
        public Vector3 point;
        public Vector3 normal;
        public Vector3 velocity;
        public FiveElements kickerElement;

        public void init(Vector3 point, Vector3 normal, Vector3 velocity, FiveElements kickerElement)
        {
            this.point = point;
            this.normal = normal;
            this.velocity = velocity;
            this.kickerElement = kickerElement;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(point);
            stream.write(normal);
            stream.write(velocity);
            stream.Write((byte)kickerElement);

        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            point = stream.readVector3();
            normal = stream.readVector3();
            velocity = stream.readVector3();
            kickerElement = (FiveElements)stream.ReadByte();

        }
    }

}