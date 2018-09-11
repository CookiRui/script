using UnityEngine;
using Cratos;

namespace RAL
{
    public class TurningAction : ContinuousAction
    {
        public Vector2 startDirection;   //开始角度
        public Vector2 endDirection;      //终点角度
        public bool ignoreTimeAction;

        public void init(uint id, Vector2 start, Vector2 end, bool ignore)
        {
            base.init(id);
            startDirection = start;
            endDirection = end;
            ignoreTimeAction = ignore;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.write(startDirection);
            stream.write(endDirection);
            stream.Write(ignoreTimeAction);

        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            startDirection = stream.readVector2();
            endDirection = stream.readVector2();
            ignoreTimeAction = stream.ReadBoolean();
        }
    };

}