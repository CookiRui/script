using FixMath.NET;

public partial class FBBall
{
    public struct Energy
    {
        public byte level;
        public byte min;
        public byte max;
        public byte decayTime;
        public byte decayTarget;
        public byte changeTarget;

        public byte value;
        public Fix64 goalRate;
        public Fix64 decayTimer;
        public bool decayTimeup { get { return decayTimer >= (Fix64)decayTime; } }
        public bool setValue(byte value)
        {
            if (value < min || max < value)
            {
                return false;
            }
            this.value = value;
            decayTimer = Fix64.Zero;
            return true;
        }
    }

    public class Configuration
    {
        public Fix64 radius;
        public Fix64 linearDamping_land;
        public Fix64 linearDamping_air;
        public Fix64 landHitVerticleDamping;
        public Fix64 landHitDamping;

        public Fix64 angularDamping;

        public Fix64 gravity;
        public Fix64 dampingAcceleration_land;
        public Fix64 dampingAcceleration_air;

        public Fix64 killerShootEnergy;

        public Energy[] energys;

        public Configuration()
        {
            radius = (Fix64)0.31;
            linearDamping_land = (Fix64)1;
            linearDamping_air = (Fix64)0.2;
            landHitVerticleDamping = (Fix64)0.6;
            landHitDamping = (Fix64)0.6;
            angularDamping = (Fix64)0;
            gravity = (Fix64)20;
            dampingAcceleration_land = (Fix64)10;
            dampingAcceleration_air = (Fix64)2;

            killerShootEnergy = (Fix64)0;

            energys = new Energy[]
            {
                new Energy
                {
                    min = 0,
                    max = 10,
                    decayTime = 4,
                    decayTarget = 0,
                    changeTarget = 0,
                    goalRate = (Fix64)1.0f,
                },
                new Energy
                {
                    min = 11,
                    max = 30,
                    decayTime = 6,
                    decayTarget = 0,
                    changeTarget = 0,
                    goalRate = (Fix64)1.0f,
                },
                new Energy
                {
                    min = 31,
                    max = 80,
                    decayTime = 8,
                    decayTarget = 11,
                    changeTarget = 11,
                    goalRate = (Fix64)1.0f,
                },
                new Energy
                {
                    min = 81,
                    max = 100,
                    decayTime = 10,
                    decayTarget = 31,
                    changeTarget = 31,
                    goalRate = (Fix64)1.0f,
                }
            };
        }

        public byte maxEnergy
        {
            get
            {
                if (energys == null || energys.Length == 0)
                    return 0;
                return energys[energys.Length - 1].max;
            }
        }

        public byte maxEnergyLevel
        {
            get
            {
                if (energys == null || energys.Length == 0)
                    return 0;
                return (byte)(energys.Length - 1);
            }
        }

        public Energy getEnergy(byte value)
        {
            if (energys == null || energys.Length == 0)
                return default(Energy);
            for (byte i = 0; i < energys.Length; i++)
            {
                var energy = energys[i];
                if (energy.min <= value && value <= energy.max)
                {
                    energy.level = i;
                    return energy;
                }
            }
            return default(Energy);
        }

    }
}
