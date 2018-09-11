using FixMath.NET;
using BW31.SP2D;

public partial class FBWorld
{
    public struct GoalZone
    {
        public FixVector2 center;//中心点
        public FixVector2 halfSize;//半宽高
        public int type;       //类型 

    }

    public class Configuration
    {
        public FixVector2 worldSize;
        public FixVector2 penaltyAreaSize;

        /// <summary>
        /// 蓝队开球点
        /// </summary>
        public FixVector3[] blueAttackPositions;

        /// <summary>
        /// 蓝队抢球点
        /// </summary>
        public FixVector3[] blueDefensePositions;

        /// <summary>
        /// 红队开球点
        /// </summary>
        public FixVector3[] redAttackPositions;

        /// <summary>
        /// 红队抢球点
        /// </summary>
        public FixVector3[] redDefensePositions;

        //球和外包围的碰撞
        //0:球V外包围   1:球V门后网   2:球V门左侧网    3:球V门右侧网   4:球V门上网 
        public Fix64[] ballCollisionRestitution;

        //人和外包围的碰撞
        public Fix64 ballCollisionRestitution_actorAndobstacle;
        //人和球的碰撞
        public Fix64 ballCollisionRestitution_actorAndball;
        //人和人的碰撞
        public Fix64 ballCollisionRestitution_actorAndactor;

        /// <summary>
        /// 比赛时间
        /// </summary>
        public Fix64 matchTime;

        /// <summary>
        /// 门大小
        /// </summary>
        public FixVector2 doorHalfSize;
        public Fix64 doorHeight;
        public Fix64 doorHalfSlopeWidth;


        public Fix64 replayTimeBeforeGoal;
        public Fix64 replayTimeAfterGoal;
        public Fix64 replayWaitTime;
        public Fix64 enterShowTime;
        public Fix64 showEnemyMoment;
        public Fix64 goalShowTime;
        public Fix64 readyTime;


        public GoalZone[] goalZones;
        public GoalZone getGoalZone(int type, int zoneIndex=0)
        {
            int currentZoneIndex = -1;

            int idx = 0;
            for (int i = 0; i < goalZones.Length; ++i)
            {
                if (type == goalZones[i].type )
                {
                    ++currentZoneIndex;
                    if (currentZoneIndex == zoneIndex)
                    {
                        idx = i;
                        break; 
                    }
                }
            }

            if (idx < goalZones.Length)
                return goalZones[idx];
            return default(GoalZone);
        }

        public Configuration()
        {
            worldSize = new FixVector2 { x = (Fix64)42, y = (Fix64)21 };
            penaltyAreaSize = new FixVector2 { x = (Fix64)6, y = (Fix64)7.5 };

            //蓝队开球
            blueAttackPositions = new FixVector3[]
            {
                new FixVector3{ x = -(Fix64)2,z =(Fix64)0 },
				new FixVector3{ x = -(Fix64)15,z =-(Fix64)12 },
				new FixVector3{ x = -(Fix64)15,z =(Fix64)12 },
                new FixVector3{ x = -(Fix64)22,z =(Fix64)0 },
				new FixVector3{ x = -(Fix64)35,z =(Fix64)0 },
            };

            //蓝队抢球
            blueDefensePositions = new FixVector3[]
            {     
                new FixVector3{ x = -(Fix64)10,z =(Fix64)12 },
				new FixVector3{ x = -(Fix64)10,z =-(Fix64)12 },
                new FixVector3{ x = -(Fix64)22.5,z =(Fix64)6 },
				new FixVector3{ x = -(Fix64)22.5,z =-(Fix64)6 },
                new FixVector3{ x = -(Fix64)35,z =(Fix64)0 },
            };

            //红队开球
            redAttackPositions = new FixVector3[]
            {
                new FixVector3{ x = (Fix64)2,z =(Fix64)0 },
				new FixVector3{ x = (Fix64)15,z =-(Fix64)12 },
				new FixVector3{ x = (Fix64)15,z =(Fix64)12 },
                new FixVector3{ x = (Fix64)22,z =(Fix64)0 },
				new FixVector3{ x = (Fix64)35,z =(Fix64)0 },
            };

            //红队抢球
            redDefensePositions = new FixVector3[]
            {
                new FixVector3{ x = (Fix64)10,z =(Fix64)12 },
				new FixVector3{ x = (Fix64)10,z =-(Fix64)12 },
				new FixVector3{ x = (Fix64)22.5,z =(Fix64)6 },
				new FixVector3{ x = (Fix64)22.5,z =-(Fix64)6 },
				new FixVector3{ x = (Fix64)35,z =(Fix64)0 },
            };

            matchTime = (Fix64)180;
            doorHalfSize = new FixVector2 { x = (Fix64)1.5, y = (Fix64)4.5 };
            doorHeight = (Fix64)5.25;
            doorHalfSlopeWidth = (Fix64)1.8;

            string ballCollisionRestitutionString = "0.5|0.1|0.1|0.1|0.1";
            string[] ballCollisionRestitutions = ballCollisionRestitutionString.Split('|');
            ballCollisionRestitution = new Fix64[ballCollisionRestitutions.Length];
            for (int i = 0; i < ballCollisionRestitution.Length; ++i)
            {
                ballCollisionRestitution[i] = (Fix64)float.Parse(ballCollisionRestitutions[i]);
            }

            ballCollisionRestitution_actorAndobstacle = (Fix64)0.4;
            ballCollisionRestitution_actorAndball = (Fix64)0.3;
            ballCollisionRestitution_actorAndactor = (Fix64)0.4;

            var enableGoal = true;
            if (enableGoal)
            {
                goalZones = new GoalZone[]
                {
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)2.8 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 0
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)2.8 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 1
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)2.8 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 2
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)2.8},
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 3
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)2.8 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 3
                },
                };
            }
            else
            {
                goalZones = new GoalZone[]
                {
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)7 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 0
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)7 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 1
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)7 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 2
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)7},
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 3
                },
                new GoalZone
                {
                    center = new FixVector2 {x = (Fix64)2.3, y = (Fix64)7 },
                    halfSize = new FixVector2{ x = (Fix64)2,y = (Fix64)2.2},
                    type = 3
                },
                };
            }
            replayTimeBeforeGoal = (Fix64)5;
            replayTimeAfterGoal = (Fix64)1;
            enterShowTime = (Fix64)15.1;
            showEnemyMoment = (Fix64)6;
            goalShowTime = (Fix64)4;
            replayWaitTime = (Fix64)1.5;
            readyTime = (Fix64)3;
        }
    }
}
