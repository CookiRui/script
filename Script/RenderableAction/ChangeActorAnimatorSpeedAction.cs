using Cratos;
namespace RAL
{
    public class ChangeActorAnimatorSpeedAction : InstantAction
    {
        public float speed;
        public void init(uint id, float speed)
        {
            base.init(id);

            this.speed = speed;
        }
        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(speed);
        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            speed = stream.ReadSingle();
        }
    }


}
