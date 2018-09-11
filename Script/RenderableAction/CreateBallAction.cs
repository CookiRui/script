using UnityEngine;
using Cratos;

namespace RAL
{
    public class CreateBallAction : InstantAction
    {
        public Vector2 position;
        public string prefab;
        public float radius;


        public void init(uint id, Vector2 position, string prefab, float radius)
        {
            base.init(id);

            this.position = position;
            this.prefab = prefab;
            this.radius = radius;            
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(position);
            stream.WriteStringByte(prefab);
            stream.Write(radius);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            position = stream.readVector2();
            prefab = stream.ReadString();
            radius = stream.ReadSingle();
        }
    };
}

