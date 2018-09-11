using UnityEngine;
using Cratos;

namespace RAL
{
    public class BallLandedAction : InstantAction
    {
        public Vector2 point;
        public bool pass;
        public byte times;
        public Vector3 velocity;
        public float preHeightVelocity;


        public void init(Vector2 point,bool pass, int times,Vector3 velocity,float preHeightVelocity)
        {
           this.point = point;
           this.pass = pass;
           this.times = (byte)times;
           this.velocity = velocity;
           this.preHeightVelocity = preHeightVelocity;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(point);
            stream.Write(pass);
            stream.Write(times);
            stream.write(velocity);
            stream.Write(preHeightVelocity);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            point = stream.readVector2();
            pass = stream.ReadBoolean();
            times = stream.ReadByte();
            velocity = stream.readVector3();
            preHeightVelocity = stream.ReadSingle();
        }
    }
   
}
