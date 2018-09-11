using Cratos;

namespace RAL
{
    public class UpdateMatchTimeAction : InstantAction
    {
        public ushort time;
        public uint frame;

        public void init(ushort time,uint frame)
        {
            this.time = time;
            this.frame = frame;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(time);
            stream.Write(frame);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            time = stream.ReadUInt16();
            frame = stream.ReadUInt32();
        }
    }
}

