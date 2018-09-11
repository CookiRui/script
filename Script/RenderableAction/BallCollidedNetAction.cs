using UnityEngine;
using Cratos;

namespace RAL
{
    public class BallCollidedNetAction : InstantAction
    {
        public Vector3 point;
        public Vector3 preVelocity;
        public Vector3 curVelocity;
        public FiveElements kickerElement;


        public void init(Vector3 point, Vector3 preVelocity, Vector3 curVelocity, FiveElements kickerElement)
        {
            this.point = point;
            this.preVelocity = preVelocity;
            this.curVelocity = curVelocity;
            this.kickerElement = kickerElement;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(point);
            stream.write(preVelocity);
            stream.write(curVelocity);
            stream.Write((byte)kickerElement);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            point = stream.readVector3();
            preVelocity = stream.readVector3();
            curVelocity = stream.readVector3();
            kickerElement = (FiveElements)stream.ReadByte();
        }
    }
}
