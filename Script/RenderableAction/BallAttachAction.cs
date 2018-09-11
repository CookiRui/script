using Cratos;

//ÄÃÇò
namespace RAL
{
    public class BallAttachAction : InstantAction
    {
        public bool gk;
        public void init(uint id, bool keeper)
        {
            base.init(id);
            gk = keeper;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(gk);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            gk = stream.ReadBoolean();
        }
    }

}
