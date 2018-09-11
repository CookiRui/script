
using BW31.SP2D;
using FixMath.NET;

public partial class FBActor
{

	public class Configuration_5
	{
		public static Configuration getConfiguration(uint id)
		{
			return null;
		}

		public static Configuration _default
		{
			get
			{
				if (s_default == null)
				{
					s_default = new Configuration() {

						radius = (Fix64)0.5f,
						//球员的碰撞体半径，要注意这个半径加上足球半径不能小于截球范围，否则无法截球。
						bodyHeight = (Fix64)2.4f,
						//球员身高

						catchingRadius = (Fix64)0.75f,
						//截球半径，仅在跑动中起效。这里应该配置无球跑动动画时足球的偏移，并且该动画的足球偏移应该稳定。获球前是在播放无球跑动的，获球后融合为持球跑动，并且持球跑动会继承无球跑动的播放进度，在融合过程中，足球从无球跑动的足球点开始融合。
						catchingAngle = (Fix64)0.95f,
						//截球扇形半角的余弦值，不需要太大
						maxCatchingBallHeight = (Fix64)0.5f,
						//截球区最大高度，适度放大，要注意足球贴地时就已经有0.225米高了。

						maxCatchingBallHelperHeight = (Fix64)2.5f,
						//自动追球辅助时，足球最大允许高度
						catchBallHelper_Raidus = (Fix64)3,		
						catchBallHelper_MaxAngleCos = (Fix64)(-1.1f), // 145+ degree
						catchBallHelper_MaxFanAngleCos = (Fix64)0.5f, // 60 degree
						catchBallHelper_BallMaxSpeed = (Fix64)20,
						maxBallActorCollideAngle = (Fix64)(45.0f / 180 * 3.14f),
						//当足球和人碰撞时角度小于配置时，足球会反弹向人的前面，方便后续拿球

						//普通移动
						normalSpeed = new Fix64[] { (Fix64)6.75f, (Fix64)0, (Fix64)0, (Fix64)0, (Fix64)0 },
						m1_maxSpeed = (Fix64)7,
						m1_moveForce = (Fix64)21,
						m1_stopDampingAcceleration = (Fix64)28,
						m1_angularSpeed = (Fix64)6,

						//防御姿态移动
						dm1_angleA = (Fix64)(30.0f / 180 * 3.14f),
						dm1_angleB = (Fix64)(120.0f / 180 * 3.14f),
						dm1_maxSpeed = new Fix64[] { (Fix64)0, (Fix64)0, (Fix64)0, (Fix64)0 },

						//带球移动
						normalSpeed_ball = (Fix64)6.75f,
						m1_maxSpeed_ball = (Fix64)6,
						m1_moveForce_ball = (Fix64)18,
						m1_stopDampingAcceleration_ball = (Fix64)24,
						m1_angluarSpeed_ball = (Fix64)3,

						//转身
						m2_minAngleCos = (Fix64)(-0.85f), //145°+
						m2_minSpeedAndWaitTime = new Fix64[] { (Fix64)0.1f, (Fix64)0.15f, (Fix64)(-0.1f), (Fix64)0.15f },
						m2_minSpeedAndWaitTime_ball = new Fix64[] { (Fix64)0.1f, (Fix64)0.2f, (Fix64)(-0.1f), (Fix64)0.2f },

						m2_movingTime = (Fix64)0.2f,

						//铲球
						st_initialSpeed = (Fix64)20,
						st_dampingToZeroTime = (Fix64)0.8f,
						st_waitingTime = (Fix64)0.4f,

						//被铲
						bst1_dampingToZeroTime = (Fix64)0.375f,
						bst1_standWaitingTime = (Fix64)0.25f,
						bst1_waitingTime = (Fix64)0.125f,

						//过人
						bst2_speed = (Fix64)1,
						bst2_waitingTime = (Fix64)0.25f,

						//无球过人
						bst3_speed = (Fix64)1,
						bst3_waitingTime = (Fix64)0.25f,

						//预防错误
						moveStateWaitingTime = (Fix64)0.05f,

						//传球
						pb_beforePassingTime = new Fix64[] { (Fix64)0.1f, (Fix64)0.1f },
						pb_afterPassingTime = new Fix64[] { (Fix64)0.2f, (Fix64)0.3f },

						//射门
						sb_angularSpeed = (Fix64)6,
						sb_beforeShootingTime = new Fix64[] { (Fix64)0.15f, (Fix64)0.1f, (Fix64)0.13f },
						sb_afterShootingTime = new Fix64[] { (Fix64)0.4f, (Fix64)0.3f, (Fix64)0.9f },

						shootBallAngleTorelance = (Fix64)(30.0f / 180 * 3.14f),
						maxGoalSpeed = new Fix64[] { (Fix64)40f, (Fix64)50f, (Fix64)60f },

						curveBallRadius = (Fix64)15.0f,
						curveBallMaxAngle = (Fix64)(30.0f / 180 * 3.14f),

						defautKickBallFoot = 1,

						//传球辅助
						passBallFov = new Fix64[] { (Fix64)(60.0f / 180 * 3.14f), (Fix64)(45.0f / 180 * 3.14f) },
						passBallMaxR = new Fix64[] { (Fix64)25.0f, (Fix64)35.0f },
						passBallMinR = new Fix64[] { (Fix64)2.0f, (Fix64)10.0f },
						passBallBestR = new Fix64[] { (Fix64)15.0f, (Fix64)25.0f },
						passBallAngleTorelance = new Fix64[] { (Fix64)(30.0f / 180 * 3.14f), (Fix64)(30.0f / 180 * 3.14f) },

						//无目标传球速度
						passingBallSpeedWhenNoTarget = new Fix64[] { (Fix64)30.0f, (Fix64)25.0f },
						passingBallVerticleSpeedWhenNoTarget = new Fix64[] { (Fix64)3.0f, (Fix64)14.0f },

						//传球辅助接球时参数
						getPassingBallStandOrMovingSpeed = new Fix64[] { (Fix64)6.0f, (Fix64)6.0f },
						getPassingBallStandOrMovingAngle = new Fix64[] { (Fix64)(15.0f / 180 * 3.14f), (Fix64)(15.0f / 180 * 3.14f) },



						scb_maxBallSpeed = (Fix64)30.0f,
						scb_maxActorSpeed = (Fix64)5.0f,
						scb_maxRadius = (Fix64)0.8f,
						scb_maxCathingTime = (Fix64)0.3f,
						scb_catchingAniTime = new Fix64[]{ (Fix64)0.17f, (Fix64)0.17f, (Fix64)0.1f},
						scb_catchingHeightLimit = new Fix64[]{ (Fix64)1.0f, (Fix64)2.5f, (Fix64)0.0f},
						scb_catchingOffset =  new Fix64[] { (Fix64)0.8f, (Fix64)0.235f, (Fix64)0.0f},
						scb_catchingOffsetH = new Fix64[] { (Fix64)0.225f, (Fix64)1.88f, (Fix64)0.0f },
						scb_lockTimeAfterCatching = new Fix64[] { (Fix64)0.3f, (Fix64)0.6f, (Fix64)0.0f },

						//scb_maxBallHeight = (Fix64)1,
						//scb_maxSpeed = (Fix64)3,
						//scb_offset = (Fix64)0.8f,
						//scb_minTime = (Fix64)0.17f,
						//scb_maxTime = (Fix64)0.3f,

						//ccb_maxBallHeight = (Fix64)2,
						//ccb_maxSpeed = (Fix64)3,
						//ccb_offset = (Fix64)0.8f,
						//ccb_offset2 = (Fix64)0.3f,
						//ccb_minTime = (Fix64)0.2f,
						//ccb_maxTime = (Fix64)0.3f,
						//ccb_lockTimeAfterCatching = (Fix64)0.4f,
						//ccb_ballHeight = (Fix64)1.76f,

						//hcb_maxBallHeight = (Fix64)0,
						//hcb_maxSpeed = (Fix64)3,
						//hcb_offset = (Fix64)0.8f,
						//hcb_offset2 = (Fix64)0.8f,
						//hcb_minTime = (Fix64)0.1f,
						//hcb_maxTime = (Fix64)0.3f,
						//hcb_lockTimeAfterCatching = (Fix64)0.15f,
						//hcb_ballHeight = (Fix64)2.5f,

						tcb_normalTime = (Fix64)1.0f,
						tcb_glideTimeAfterCatching = (Fix64)0.5f,
						tcb_lockTimeAfterCatching = (Fix64)0.5f,



						ballAttachPositions = new FixVector3[]
						{
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.75 },
							//站立接短传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.58 },
							//跑步接短传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.75 },
							//站立接长传
							new FixVector3 { x = (Fix64)0, y = (Fix64)1.88, z = (Fix64)0.235 },
							//跑步接长传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.75 },
						},

                        ballDetachPositions = new FixVector3[]
                        {
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.58 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.58 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.58 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.58 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.58 },
                        },

                        ballDetachPositionsAttacked = new FixVector3[]
                        {
                            new FixVector3 { x = (Fix64)0.58, y = (Fix64)0.225, z = (Fix64)1.88 },
                        },
                        ballDetachSpeedAttacked = new Fix64[] { (Fix64)0 },
                        ballDetachSlerpTimeAttacked = new Fix64[] { (Fix64)1.0f },


						lastBallSamplePositions = new FixVector3[]
						{
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.8 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.75 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)1.88, z = (Fix64)0.235 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0, z = (Fix64)0 },
						},

                        maxSkillCastingTime = (Fix64)100.0f,
                        maxSkillLockTimeAfterCasting = (Fix64)0.2f,
                        tauntTime = (Fix64)2,
                        cheerUniqueTime = (Fix64)2.16,
                        element = FiveElements.None,


                    };
				}
				return s_default;
			}
		}

		static Configuration s_default = null;

	}
}