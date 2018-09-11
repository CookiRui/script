using UnityEngine;
using Cratos;

namespace RAL
{
    public class PassBeginAction : InstantAction
    {
        public BitStream passBallType = new BitStream(1);

        public void init(uint id, bool shortPassBall, bool rightFoot )
        {
            base.init(id);
            this.shortPassBall = shortPassBall;
            this.rightFoot = rightFoot;
        }

        public bool rightFoot
        {
            get
            {
                return passBallType.getBit(7) == 1;
            }
            set
            {
                passBallType.setBit(7, value ? 1 : 0);
            }
        }
        public bool shortPassBall
        {
            get
            {
                return passBallType.getBit(6) == 1;
            }
            set
            {
                passBallType.setBit(6, value ? 1 : 0);
            }
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(passBallType.data);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            passBallType.data = stream.ReadBytes(1);
        }
    }


    public class ShootBeginAction : InstantAction
    {
        public void init(uint id, bool rightFoot)
        {
            base.init(id);
            this.rightFoot =rightFoot;
        }

        public override uint objectID
        {
            get { return _objectID.getBit(0, 3); }
            set { _objectID.setBit(0, 3, value); }
        }

        //使用哪儿个脚射门
        public bool rightFoot
        {
            get { return _objectID.getBit(7) == 1; }
            set { _objectID.setBit(7, value ? 1 : 0); }
        }
    }

    public class ShootBallReadyAction : InstantAction
    {
        public void init(uint id, ShootType shootType)
        {
            base.init(id);
            this.shootType = (uint)shootType;
        }

        public override uint objectID
        {
            get { return _objectID.getBit(0, 3); }
            set { _objectID.setBit(0, 3, value); }
        }

        //普通射门or大力射门
        public uint shootType
        {
            get { return _objectID.getBit(4, 7); }
            set { _objectID.setBit(4, 7, value); }
        }
    }


    public class WarmUpAction : InstantAction { }

    public class CheerStandAction : InstantAction { }

    public class DismayAction : InstantAction { }

}