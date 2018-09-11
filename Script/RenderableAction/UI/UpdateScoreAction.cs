using Cratos;

namespace RAL
{
    public class UpdateScoreAction : InstantAction
    {
        public ushort red;
        public ushort blue;

        public void init(ushort red, ushort blue)
        {
            this.red = red;
            this.blue = blue;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(red);
            stream.Write(blue);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            red = stream.ReadUInt16();
            blue = stream.ReadUInt16();
        }
    }
}

