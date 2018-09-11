using Cratos;

namespace RAL
{
    public class BallEnergyLevelChangedAction : InstantAction
    {
        public byte oldLV;
        public byte newLV;

        public void init(byte oldLV, byte newLV)
        {
            this.oldLV = oldLV;
            this.newLV = newLV;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(oldLV);
            stream.Write(newLV);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            oldLV = stream.ReadByte();
            newLV = stream.ReadByte();
        }
    }
}

