using Cratos;
using UnityEngine;
namespace RAL
{
    public class ProfilerAction : InstantAction
    {
        public uint stamp;

        public void init(uint stamp)
        {
            base.init(0);
            this.stamp = stamp;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(stamp);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            stamp = stream.ReadUInt32();
        }
    };
}

