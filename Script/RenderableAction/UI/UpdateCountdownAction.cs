using Cratos;

namespace RAL
{
    public class UpdateCountdownAction : InstantAction
    {
        public byte time;

        public void init(byte time)
        {
            this.time = time;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(time);
        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            time = stream.ReadByte();
        }
    }

}
