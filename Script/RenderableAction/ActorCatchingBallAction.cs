using Cratos;

namespace RAL
{
    public class ActorStandCatchingBallAction : InstantAction
    {
        public bool rightFoot = true;

        public void init(uint id, bool rightFoot)
        {
            base.init(id);
            this.rightFoot = rightFoot;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(rightFoot);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            rightFoot = stream.ReadBoolean();
        }
    }


    public class ActorStandCatchingBallBeginAction : InstantAction
    {
    }



    public class ActorAirCatchingBallAction : InstantAction
    {
        public byte catchingType = 1;

        public void init(uint id, byte type)
        {
            base.init(id);
            catchingType = type;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(catchingType);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            catchingType = stream.ReadByte();
        }
    }



    public class ActorAirCatchingBallBeginAction : InstantAction
    {
    }

    public class ActorTigerCatchingBallBeginAction : InstantAction
    {

    }


    public class DoorKeeperCatchingBallAction : InstantAction
    {
        public byte zoneIndex = 0;
        public bool rightSide = true;

        public void init(uint id, byte zoneIndex, bool rightSide)
        {
            base.init(id);
            this.zoneIndex = zoneIndex;
            this.rightSide = rightSide;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(zoneIndex);
            stream.Write(rightSide);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            zoneIndex = stream.ReadByte();
            rightSide = stream.ReadBoolean();
        }
    }



    public class DoorKeeperBeginToGetupAction : InstantAction
    {
        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
        }
    }


    public class DoorKeeperCatchingBallBeginAction : InstantAction
    {
        public bool canCatchBall = false;

        public void init(uint id, bool canCatchBall)
        {
            base.init(id);
            this.canCatchBall = canCatchBall;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(canCatchBall);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            canCatchBall = stream.ReadBoolean();
        }
    }


    public class SlowGetPassingBallAction : InstantAction
    {
    }


  
}