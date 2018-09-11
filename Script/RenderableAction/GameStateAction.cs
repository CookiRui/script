using Cratos;

namespace RAL
{
    public class GameReadyAction : InstantAction {
        public void init() { }
    }
    public class GameBeginAction : InstantAction {
        public void init() { }
    }

    public class GoalAction : InstantAction
    {
        public FBTeam team;
        public Location door;

        public void init(uint id, FBTeam team, Location door)
        {
            base.init(id);
            this.team = team;
            this.door = door;
        }

        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write((byte)team);
            stream.Write((byte)door);
        }

        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            team = (FBTeam)stream.ReadByte();
            door = (Location)stream.ReadByte();
        }
    }

    public class ReplayBeginAction : InstantAction
    {
        public uint beginFrame;
        public uint endFrame;
        public byte replayTime;
        public Location goalDoor;
        public float positionRandomValue;
        public float shootRandomValue;
        public float goalRandomValue;
        public ulong goaler;
        public ushort blueScore;
        public ushort redScore;
        public ushort goalTime;
        public FBTeam goalTeam;

     
        public void init(   uint beginFrame,
                            uint endFrame,
                            ushort   goalTime,
                            ushort replayTime,
                            ulong goaler,
                            Location goalDoor,
                            ushort blueScore,
                            ushort redScore,
                            uint gkId,
                            float positionRandomValue,
                            float shootRandomValue,
                            float goalRandomValue,
                            FBTeam goalTeam)
        {
            this.beginFrame = beginFrame;
            this.endFrame = endFrame;
            this.replayTime = (byte)replayTime;
            this.goalDoor = goalDoor;
            this.positionRandomValue = positionRandomValue;
            this.shootRandomValue = shootRandomValue;
            this.goaler = goaler;
            this.blueScore = blueScore;
            this.redScore = redScore;
            this.goalTime = goalTime;
            this.goalTeam = goalTeam;
        }
        public override void serialize(BytesStream stream)
        {
            base.serialize(stream);
            stream.Write(beginFrame);
            stream.Write(endFrame);
            stream.Write(replayTime);
            stream.Write((byte)goalDoor);
            stream.Write(positionRandomValue);
            stream.Write(shootRandomValue);
            stream.Write(goalRandomValue);
            stream.Write(goaler);
            stream.Write(blueScore);
            stream.Write(redScore);
            stream.Write(goalTime);
            stream.Write((byte)goalTeam);
        }
        public override void unserialize(BytesStream stream)
        {
            base.unserialize(stream);
            beginFrame = stream.ReadUInt32();
            endFrame = stream.ReadUInt32();
            replayTime = stream.ReadByte();
            goalDoor = (Location)stream.ReadByte();
            positionRandomValue = stream.ReadSingle();
            shootRandomValue = stream.ReadSingle();
            goalRandomValue = stream.ReadSingle();
            goaler = stream.ReadUInt64();
            blueScore = stream.ReadUInt16();
            redScore = stream.ReadUInt16();
            goalTime = stream.ReadUInt16();
            goalTeam = (FBTeam)stream.ReadByte();
        }
    }
    public class ReplayEndAction : InstantAction 
    {
        public void init() { }
    }

    public class GameOverAction : InstantAction 
    {
        public void init() { }
    }
}
