using UnityEngine;
using Cratos;

namespace RAL
{
    public class CreateWorldAction : InstantAction
    {
        public ushort mapID;
        public Vector2 mainExtent;
        public Vector2 doorExtent;
        public float doorHeight;

        public void init(Vector2 mainExtent, Vector2 doorExtent, float doorHeight)
        {
            this.mainExtent = mainExtent;
            this.doorExtent = doorExtent;
            this.doorHeight = doorHeight;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(mainExtent);
            stream.write(doorExtent);
            stream.Write(doorHeight);
            stream.Write(mapID);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            mainExtent = stream.readVector2();
            doorExtent = stream.readVector2();
            doorHeight = stream.ReadSingle();
            mapID = stream.ReadUInt16();
        }
    }
}

