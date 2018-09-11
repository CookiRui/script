using Cratos;

namespace RAL
{
    public class BeginHitAction : InstantAction
    {
        public uint victim;
        public void init(uint attacker, uint victim)
        {
            base.init(attacker);
            this.victim = victim;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(victim);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            victim = stream.ReadUInt32();
        }
    }

    public class EndHitAction : InstantAction { public void init() { } }
    public class HitCompletedAction : InstantAction { public void init() { } }

}
