using Cratos;

namespace RAL
{
    public class SettlementAction : InstantAction
    {
        public FBTeam winner;
        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write((byte)winner);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            winner = (FBTeam)stream.ReadByte();
        }
    }
}

