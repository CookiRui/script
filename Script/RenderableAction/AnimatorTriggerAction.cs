using UnityEngine;
using Cratos;

namespace RAL
{
    public class AnimatorIntAction : InstantAction
    {
        public string name;
        public int value;

        public void init(uint id, string name, int value)
        {
            base.init(id);
            this.name = name;
            this.value = value;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.WriteStringByte(name);
            stream.Write(value);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            name = stream.ReadString();
            value = stream.ReadInt32();
        }
    }


    public class AnimatorFloatAction : InstantAction
    {
        public string name;
        public float value;


        public void init(uint id, string name, float value)
        {
            base.init(id);
            this.name = name;
            this.value = value;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.WriteStringByte(name);
            stream.Write(value);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            name = stream.ReadString();
            value = stream.ReadSingle();
        }
    }

    public class AnimatorTriggerAction : InstantAction
    {
        public string name;

        public void init(uint id, string name)
        {
            base.init(id);
            this.name = name;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.WriteStringByte(name);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            name = stream.ReadString();
        }
    }


    public class AnimatorBoolAction : InstantAction
    {
        public string name;
        public bool value;

        public void init(uint id, string name, bool value)
        {
            base.init(id);
            this.name = name;
            this.value = value;
        }


        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.WriteStringByte(name);
            stream.Write(value);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            name = stream.ReadString();
            value = stream.ReadBoolean();
        }
    }




    public class TurnAction : InstantAction
    {
        public Vector2 direction;      //终点角度
        public bool ignoreTimeAction;

        public void init(uint actorId, Vector2 direction) {
            objectID = actorId;
            this.direction = direction;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(direction);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            direction = stream.readVector2();
        }
    }


    public class DribbleAction : InstantAction { }


    public class FallAction : InstantAction { }


    public class SlideAction : InstantAction { }


    public class TauntAction : InstantAction { }


    public class CheerUniqueAction : InstantAction { }
}