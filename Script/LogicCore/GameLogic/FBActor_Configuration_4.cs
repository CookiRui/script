
using BW31.SP2D;
using FixMath.NET;

public partial class FBActor
{

	public class Configuration_4
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

						radius = (Fix64)0.25f,
						//球员的碰撞体半径，要注意这个半径加上足球半径不能小于截球范围，否则无法截球。
						catchingRadius = (Fix64)0.66f,
						//这里应该配置无球跑动动画时足球的偏移，并且该动画的足球偏移应该稳定。获球前是在播放无球跑动的，获球后融合为持球跑动，并且持球跑动会继承无球跑动的播放进度，在融合过程中，足球从无球跑动的足球点开始融合。
						maxCatchingBallHeight = (Fix64)0.5f,
                        maxCatchingBallHelperHeight = (Fix64)0.5f,
                        maxBallActorCollideAngle = (Fix64)(30.0f / 180 * 3.14f),
						//截球区最大高度，适度放大，要注意足球贴地时就已经有0.225米高了。
						catchingAngle = (Fix64)0.95f,
						//截球扇形半角，不需要太大

						normalSpeed = new Fix64[] { (Fix64)5.12f, (Fix64)1.44f, (Fix64)1.44f, (Fix64)1.44f, (Fix64)1.44f },

						m1_maxSpeed = (Fix64)6,
						m1_moveForce = (Fix64)18,
						//m1_normalDampingAcceleration = (Fix64)24,
						m1_angularSpeed = (Fix64)8,

						m1_stopDampingAcceleration = (Fix64)24,

						dm1_angleA = (Fix64)(45.0f / 180 * 3.14f),
						dm1_angleB = (Fix64)(135.0f / 180 * 3.14f),
						dm1_maxSpeed = new Fix64[] { (Fix64)3, (Fix64)3, (Fix64)3, (Fix64)3 },

						//带球
						normalSpeed_ball = (Fix64)5.12f,

						m1_maxSpeed_ball = (Fix64)6,
						m1_moveForce_ball = (Fix64)18,
						//m1_normalDampingAcceleration_ball = (Fix64)3,
						m1_angluarSpeed_ball = (Fix64)4,

						m1_stopDampingAcceleration_ball = (Fix64)24,

						m2_minAngleCos = (Fix64)(-0.85f), //145°+
						m2_minSpeedAndWaitTime = new Fix64[] { (Fix64)0.1f, (Fix64)0.1f, (Fix64)(-0.1f), (Fix64)0.1f },
						m2_minSpeedAndWaitTime_ball = new Fix64[] { (Fix64)0.1f, (Fix64)0.15f, (Fix64)(-0.1f), (Fix64)0.15f },

						m2_movingTime = (Fix64)0.2f,

						//kb_dampingToZeroTime = new Fix64[] { (Fix64)0.3f, (Fix64)0.4f },
						//kb_standWaitingTime = new Fix64[] { (Fix64)0.15f, (Fix64)0.2f },
						//kb_beforeKickingTime = new Fix64[] { (Fix64)0.1f, (Fix64)0.1f },
						//kb_afterKickingTime = new Fix64[] { (Fix64)0.4f, (Fix64)0.4f },

						st_initialSpeed = (Fix64)20,
						st_dampingToZeroTime = (Fix64)0.8f,
						st_waitingTime = (Fix64)0.4f,

						bst1_dampingToZeroTime = (Fix64)0.375f,
						bst1_standWaitingTime = (Fix64)0.25f,
						bst1_waitingTime = (Fix64)0.125f,

						bst2_speed = (Fix64)1,
						bst2_waitingTime = (Fix64)0.25f,

						bst3_speed = (Fix64)1,
						bst3_waitingTime = (Fix64)0.25f,

						moveStateWaitingTime = (Fix64)0.05f,

						pb_beforePassingTime = new Fix64[] { (Fix64)0.1f, (Fix64)0.12f },
						pb_afterPassingTime = new Fix64[] { (Fix64)0.15f, (Fix64)0.25f },

						sb_angularSpeed = (Fix64)6,
						sb_beforeShootingTime = new Fix64[] { (Fix64)0.15f, (Fix64)0.1f, (Fix64)0.075f, (Fix64)2.3f },
						sb_afterShootingTime = new Fix64[] { (Fix64)0.35f, (Fix64)0.25f, (Fix64)0.55f, (Fix64)0.8f },
						sb_ShootBallZone = new byte[] { 0, 0, 0, 1 },
						
						curveBallRadius = (Fix64)15.0f,
						curveBallMaxAngle = (Fix64)(30.0f / 180 * 3.14f),

						defautKickBallFoot = 1,

						bodyHeight = (Fix64)1.9f,

						passBallFov = new Fix64[] { (Fix64)(60.0f / 180 * 3.14f), (Fix64)(45.0f / 180 * 3.14f) },
						passBallMaxR = new Fix64[] { (Fix64)25.0f, (Fix64)35.0f },
						passBallMinR = new Fix64[] { (Fix64)2.0f, (Fix64)10.0f },
						passBallBestR = new Fix64[] { (Fix64)15.0f, (Fix64)25.0f },
						passBallAngleTorelance = new Fix64[] { (Fix64)(30.0f / 180 * 3.14f), (Fix64)(30.0f / 180 * 3.14f) },


						passingBallSpeedWhenNoTarget = new Fix64[] { (Fix64)30.0f, (Fix64)25.0f },
						passingBallVerticleSpeedWhenNoTarget = new Fix64[] { (Fix64)3.0f, (Fix64)14.0f },

						getPassingBallStandOrMovingSpeed = new Fix64[] { (Fix64)6.0f, (Fix64)6.0f },
						getPassingBallStandOrMovingAngle = new Fix64[] { (Fix64)(15.0f / 180 * 3.14f), (Fix64)(15.0f / 180 * 3.14f) },

                        
                        shootBallAngleTorelance = (Fix64)(30.0f / 180 * 3.14f),
						maxGoalSpeed = new Fix64[] { (Fix64)40f, (Fix64)50f, (Fix64)60f, (Fix64)70f },

						catchBallHelper_Raidus = (Fix64)2,		
						catchBallHelper_MaxAngleCos = (Fix64)(-1.1f), // 145+ degree
						catchBallHelper_MaxFanAngleCos = (Fix64)0.5f, // 60 degree
						catchBallHelper_BallMaxSpeed = (Fix64)20,

						scb_maxBallSpeed = (Fix64)30.0f,
						scb_maxActorSpeed = (Fix64)5.0f,
						scb_maxRadius = (Fix64)1.0f,
						scb_maxCathingTime = (Fix64)0.3f,
						scb_catchingAniTime = new Fix64[]{ (Fix64)0.0f, (Fix64)0.17f, (Fix64)0.17f, (Fix64)0.17f},
						scb_catchingHeightLimit = new Fix64[]{ (Fix64)0.0f, (Fix64)1.0f, (Fix64)2.0f, (Fix64)5.0f},
						scb_catchingOffset =  new Fix64[] { (Fix64)0.0f, (Fix64)1.0f, (Fix64)0.2f, (Fix64)0.6f},
						scb_catchingOffsetH = new Fix64[] { (Fix64)0.0f, (Fix64)0.225f, (Fix64)1.62f, (Fix64)2.65f },
						scb_lockTimeAfterCatching = new Fix64[] { (Fix64)0.0f, (Fix64)0.5f, (Fix64)0.5f, (Fix64)0.5f },


                        //scb_maxBallHeight = (Fix64)1,
                        //scb_maxSpeed = (Fix64)3,
                        //scb_offset = (Fix64)0.8f,
                        //scb_minTime = (Fix64)0.15f,
                        //scb_maxTime = (Fix64)0.3f,
                        //scb_lockTimeAfterCatching = (Fix64)0.867f,

                        //ccb_maxBallHeight = (Fix64)2,
                        //ccb_maxSpeed = (Fix64)3,
                        //ccb_offset = (Fix64)0.8f,
                        //ccb_offset2 = (Fix64)0.3f,
                        //ccb_minTime = (Fix64)0.1f,
                        //ccb_maxTime = (Fix64)0.3f,
                        //ccb_lockTimeAfterCatching = (Fix64)1.2f,
                        //ccb_ballHeight = (Fix64)1.76f,

                        //hcb_maxBallHeight = (Fix64)30,
                        //hcb_maxSpeed = (Fix64)3,
                        //hcb_offset = (Fix64)0.8f,
                        //hcb_offset2 = (Fix64)0.3f,
                        //hcb_minTime = (Fix64)0.1f,
                        //hcb_maxTime = (Fix64)0.3f,
                        //hcb_lockTimeAfterCatching = (Fix64)0.15f,
                        //hcb_ballHeight = (Fix64)2.5f,

						tcb_normalTime = (Fix64)0.2f,
						tcb_lockTimeAfterCatching = (Fix64)0.2f,
                        tcb_glideTimeAfterCatching = (Fix64)0.2f,

						ballAttachPositions = new FixVector3[]
						{
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.66 },
							//站立接短传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.4 },
							//跑步接短传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.4 },
							//站立接长传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.4 },
							//跑步接长传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.4 },
						},

                        ballDetachPositions = new FixVector3[]
                        {
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.48 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.48 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.48 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.48 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.48 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)12, z = (Fix64)1 },
                        },

                        ballDetachPositionsAttacked = new FixVector3[]
                        {
                            new FixVector3 { x = (Fix64)0.28, y = (Fix64)0.225, z = (Fix64)0.98 },
                        },
                        ballDetachSpeedAttacked = new Fix64[] { (Fix64)0 },
                        ballDetachSlerpTimeAttacked = new Fix64[] { (Fix64)1.0f },

						lastBallSamplePositions = new FixVector3[]
						{
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.8 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.225, z = (Fix64)0.66 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)1.76, z = (Fix64)0.3 },
							new FixVector3 { x = (Fix64)0, y = (Fix64)0, z = (Fix64)0 },
						},

                        flyCatchingBallFreezingTime = (Fix64)0.1f,
                       //守门员扑球范围0123456对应ABCDEFG参数（详见文档），长度为绝对范围
						dkcb_edgeLimit = new Fix64[] { (Fix64)1.5f, (Fix64)1.0f, (Fix64)1.85f, (Fix64)4.0f, (Fix64)1.5f, (Fix64)4.0f, (Fix64)4.0f },                        
		                //对应 0-a区  1-b区 2-c区 3-d、g区 4-f、e区
		                //守门员在每个区接球时对应的位置偏移 x 对应水平偏移， y对应垂直偏移
		                dkcb_cathingOffset = new FixVector2[] 
                        {
                            new FixVector2{ x = (Fix64)0, y = (Fix64)0 },
                            new FixVector2{ x = (Fix64)0, y = (Fix64)0 },
                            new FixVector2{ x = (Fix64)0, y = (Fix64)1.8f },
                            new FixVector2{ x = (Fix64)1.1f, y = (Fix64)1.031f },
                            new FixVector2{ x = (Fix64)0.696f, y = (Fix64)0.225f },
                        },
		                //守门员接球时动画所需要的时间
						dkcb_animationCathingTime = new Fix64[] { (Fix64)0.167f, (Fix64)0.167f, (Fix64)0.167f, (Fix64)0.133f, (Fix64)0.133f },
                        //守门员空中接球落地后滑行等待时间
						dkcb_afterFallingGlideTime = new Fix64[] { (Fix64)0.0f, (Fix64)0.0f, (Fix64)0.0f, (Fix64)0.4f, (Fix64)0.4f },
		                //守门员接球后等待时间对应 0-a区  1-b区 2-c区 3-d、g区 4-f、e区
						dkcb_afterCathingWaitingTime = new Fix64[] { (Fix64)0.867f, (Fix64)1.2f, (Fix64)0.2f, (Fix64)0.0f, (Fix64)0.0f },
		                //守门员在每个区的接球移动速度 ab区配置为0
		                dkcb_cathingBallMovingVolocity = new FixVector2[] 
                        {
							new FixVector2{ x = (Fix64)8, y = (Fix64)8 },
							new FixVector2{ x = (Fix64)8, y = (Fix64)8 },
							new FixVector2{ x = (Fix64)4, y = (Fix64)8 },
							new FixVector2{ x = (Fix64)8, y = (Fix64)15 },
							new FixVector2{ x = (Fix64)8, y = (Fix64)15 },
                        },

						fallingAcceleration = (Fix64)100.0f,

                        maxSkillCastingTime = (Fix64)100.0f,
                        maxSkillLockTimeAfterCasting = (Fix64)0.2f,

                        tauntTime = (Fix64)2,
                        cheerUniqueTime = (Fix64)4,
                        bst1_initialSpeed = (Fix64)10.0f,
                        element = FiveElements.None,

                    };
				}
				return s_default;
			}
		}

		static Configuration s_default = null;
	}
}