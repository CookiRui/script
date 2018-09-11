
using BW31.SP2D;
using FixMath.NET;

public partial class FBActor
{
    public class Configuration
    {

        //获取球的半径
        public Fix64 catchingRadius;
        //获取球的角度，取值为[-1,1]，即填写[-180,180]之间的余弦值，eg:60° 对应为0.5
        public Fix64 catchingAngle;

        public Fix64 radius;//半径

        /// 美术动画资源的标准移动速度, 0标准移动速度 1防御向前 2 防御向后 3防御向左 4防御向右
        public Fix64[] normalSpeed;

        /// 美术动画资源的标准移动速度(带球)
        public Fix64 normalSpeed_ball;

        // 正常移动
        public Fix64 m1_maxSpeed;
        public Fix64 m1_moveForce;
        //public Fix64 m1_normalDampingAcceleration;
        public Fix64 m1_angularSpeed;
        public Fix64 m1_stopDampingAcceleration;

        //防御状态的移动角度 A之内往前移动 B之外往后移动
        public Fix64 dm1_angleA;
        public Fix64 dm1_angleB;
        public Fix64[] dm1_maxSpeed;

        // 正常移动(带球)
        public Fix64 m1_maxSpeed_ball;
        public Fix64 m1_moveForce_ball;
        //public Fix64 m1_normalDampingAcceleration_ball;
        public Fix64 m1_angluarSpeed_ball;
        public Fix64 m1_stopDampingAcceleration_ball;

        //AI带球移动和不带球移动时的角速度
        public Fix64 m1_angluarSpeed_ball_ai;
        public Fix64 m1_angularSpeed_ai;


        // 急停转身
        //  
        public Fix64 m2_minAngleCos;
        // speed1, time1, speed2, time2, ..., speedN, timeN
        // speed1 > speed2 > ... > speedN
        public Fix64[] m2_minSpeedAndWaitTime;
        public Fix64[] m2_minSpeedAndWaitTime_ball;
        public Fix64 m2_movingTime; // 转身后移动的时间

        // 踢球 [类型]
        public Fix64[] kb_dampingToZeroTime; // 速度非零时的减速+等待时间
        public Fix64[] kb_standWaitingTime; // 速度为零时的等待时间
        public Fix64[] kb_beforeKickingTime; // 踢球前的等待时间
        public Fix64[] kb_afterKickingTime; // 踢球后的等待时间

        // 铲球
        public byte st_beAttackedFaceBackoff;//被攻击时面朝后退方向为1 否则为0
        public Fix64 st_initialSpeed;
        public Fix64 st_dampingToZeroTime;
        public Fix64 st_waitingTime;
        //最大铲球影响范围
        public Fix64 st_maxSlidingTargetDistance;
        public Fix64 st_maxSlidingTargetAngle;


        // 被铲球，丢球
        public Fix64 bst1_initialSpeed;
        public Fix64 bst1_dampingToZeroTime;
        public Fix64 bst1_standWaitingTime;
        public Fix64 bst1_waitingTime;

        // 被铲球，继续带球
        public Fix64 bst2_speed;
        public Fix64 bst2_waitingTime;

        // 无球被铲
        public Fix64 bst3_speed;
        public Fix64 bst3_waitingTime;

        public Fix64 moveStateWaitingTime;


        public int defautKickBallFoot;//0 左， 1右脚

        // 传球 [类型]
        public Fix64[] pb_beforePassingTime;
        public Fix64[] pb_afterPassingTime;

        // 射门 [类型] 0普通 1一般 2超级射门 3必杀射门
        public Fix64 sb_angularSpeed;
        public Fix64[] sb_beforeShootingTime;
        public Fix64[] sb_afterShootingTime;
        public Fix64[] maxGoalSpeed;
        public byte[] sb_ShootBallZone;//射门区域
        //最大射门距离
        public Fix64 maxGoalDistance;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                

        //弧线球半径
        public Fix64 curveBallRadius;
        //弧线球最大角度
        public Fix64 curveBallMaxAngle;


        //定点接球
        //角色能停球时球的最大速度
        public Fix64 scb_maxBallSpeed;
        //能停球时的角色的最大速度
        public Fix64 scb_maxActorSpeed;
        //检测半径
        public Fix64 scb_maxRadius;
        //停球时间极限
        public Fix64 scb_maxCathingTime;
        //
        public Fix64[] scb_catchingAniTime;
        //停球限制高度 0脚下 1胸部 2手 3...元素个数必须和下面几组的一样
        public Fix64[] scb_catchingHeightLimit;
        //停球点距离角色的偏移 0脚下 1胸部 2手, 3....
        public Fix64[] scb_catchingOffset;
        //停球点距离角色的偏移
        public Fix64[] scb_catchingOffsetH;
        //锁定时间
        public Fix64[] scb_lockTimeAfterCatching;



        //// 定点接球
        //public Fix64 scb_maxBallHeight;
        //public Fix64 scb_maxSpeed;
        //public Fix64 scb_offset;
        //public Fix64 scb_minTime;
        //public Fix64 scb_maxTime;
        ////public Fix64 scb_lockTimeAfterCatching;


        ////胸部停球
        //public Fix64 ccb_maxBallHeight;
        //public Fix64 ccb_maxSpeed;
        //public Fix64 ccb_offset;
        //public Fix64 ccb_offset2;
        //public Fix64 ccb_minTime;
        //public Fix64 ccb_maxTime;
        //public Fix64 ccb_lockTimeAfterCatching;
        //public Fix64 ccb_ballHeight;

        ////头顶停球，针对于守门员
        //public Fix64 hcb_maxBallHeight;//非守门员配置0
        //public Fix64 hcb_maxSpeed;
        //public Fix64 hcb_offset;
        //public Fix64 hcb_offset2;
        //public Fix64 hcb_minTime;
        //public Fix64 hcb_maxTime;
        //public Fix64 hcb_lockTimeAfterCatching;
        //public Fix64 hcb_ballHeight;

        //饿虎扑食拿球动画基础时间
        public Fix64 tcb_normalTime;
        //饿虎捕食拿球后滑动时间
        public Fix64 tcb_glideTimeAfterCatching;
        //饿虎捕食拿球后站立等待时间
        public Fix64 tcb_lockTimeAfterCatching;

        public Fix64 flyCatchingBallFreezingTime;//空中接球后停滞时间

        //守门员扑球范围0123456对应ABCDEFG参数（详见文档），长度为绝对范围
        public Fix64[] dkcb_edgeLimit;

        //对应 0-a区  1-b区 2-c区 3-d、g区 4-f、e区
        //守门员在每个区接球时对应的位置偏移 x 对应水平偏移， y对应垂直偏移
        public FixVector2[] dkcb_cathingOffset;
        //守门员接球时动画所需要的时间
        public Fix64[] dkcb_animationCathingTime;        
        //守门员空中接球落地后滑行等待时间对应 0-a区  1-b区 2-c区 3-d、g区 4-f、e区
        public Fix64[] dkcb_afterFallingGlideTime;
        //守门员接球后等待时间对应 0-a区  1-b区 2-c区 3-d、g区 4-f、e区
        public Fix64[] dkcb_afterCathingWaitingTime;

        //守门员在每个区的接球移动速度 ab区配置为0
        public FixVector2[] dkcb_cathingBallMovingVolocity;

        //下坠加速度
        public Fix64 fallingAcceleration;


        // 最大拾球高度,普通接球高度
        public Fix64 maxCatchingBallHeight;

        //在此角度之内，按照球员方向反射，在此角度之外，翻找碰撞法线进行反射
        public Fix64 maxBallActorCollideAngle;

        //追球辅助启动时球的最大高度
        public Fix64 maxCatchingBallHelperHeight;

        //没有目标时传球的水平速度,0短传 1长传
        public Fix64[] passingBallSpeedWhenNoTarget;
        //没有目标时传球的垂直速度 0短传 1长传
        public Fix64[] passingBallVerticleSpeedWhenNoTarget;

        //站立停球和移动停球速度界限 0短传 1长传
        public Fix64[] getPassingBallStandOrMovingSpeed;
        //拿到传球时角度阈值 0短传 1长传
        public Fix64[] getPassingBallStandOrMovingAngle; 

        public Fix64 catchBallHelper_Raidus;
        public Fix64 catchBallHelper_MaxAngleCos;
        public Fix64 catchBallHelper_MaxFanAngleCos;
        public Fix64 catchBallHelper_BallMaxSpeed;

        public Fix64[] passBallFov;
        public Fix64[] passBallMaxR;
        public Fix64[] passBallMinR;
        public Fix64[] passBallBestR;
        public Fix64[] passBallAngleTorelance;

        public Fix64 shootBallAngleTorelance;

        //角色高度
        public Fix64 bodyHeight;

        //拿球绑定点,0Default 1站立接短传 2跑步接短传 3站立接长传 4跑步接长传
        public FixVector3[] ballAttachPositions;

        //球分离点   0 短传  1长传  2普通射门 3大力射门 4超级射门 5必杀射门
        public FixVector3[] ballDetachPositions;

        //被攻击掉球分离点  0默认分离点，后面为特殊化处理
        public FixVector3[] ballDetachPositionsAttacked;
        //被攻击时的球的叠加速度
        public Fix64[] ballDetachSpeedAttacked;
        //被攻击时运动到分离点的时间
        public Fix64[] ballDetachSlerpTimeAttacked;

        //最后一个足球位置取样点
        //0 站立脚下停球
        //1 跑步中脚下停球
        //2 站立胸部停球
        //3 站立头顶停球
        public FixVector3[] lastBallSamplePositions;

        //技能执行时间
        public Fix64 maxSkillCastingTime;
        //技能结束后的锁定时间
        public Fix64 maxSkillLockTimeAfterCasting;

        #region 足球能量值
        public byte shortPassEnergy;
        public byte longPassEnergy;
        public Fix64 chargeIncreaseEnergyInterval;
        #endregion

        #region Performance
        public Fix64 tauntTime;
        public Fix64 cheerUniqueTime;

        #endregion

        public FiveElements element;

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

                        catchingRadius = (Fix64)0.9f,
                        //截球半径，仅在跑动中起效。这里应该配置无球跑动动画时足球的偏移，并且该动画的足球偏移应该稳定。获球前是在播放无球跑动的，获球后融合为持球跑动，并且持球跑动会继承无球跑动的播放进度，在融合过程中，足球从无球跑动的足球点开始融合。
                        catchingAngle = (Fix64)0.95f,
                        //截球扇形半角的余弦值，不需要太大
                        maxCatchingBallHeight = (Fix64)0.5f,
                        //截球区最大高度，适度放大，要注意足球贴地时就已经有0.304米高了。

                        maxCatchingBallHelperHeight = (Fix64)3.0f,
                        //自动追球辅助时，足球最大允许高度
                        catchBallHelper_Raidus = (Fix64)3,
                        catchBallHelper_MaxAngleCos = (Fix64)(-1.1f), // 145+ degree
                        catchBallHelper_MaxFanAngleCos = (Fix64)0.5f, // 60 degree
                        catchBallHelper_BallMaxSpeed = (Fix64)20,
                        maxBallActorCollideAngle = (Fix64)(45.0f / 180 * 3.14f),
                        //当足球和人碰撞时角度小于配置时，足球会反弹向人的前面，方便后续拿球

                        //普通移动
                        normalSpeed = new Fix64[] { (Fix64)7.72f, (Fix64)0, (Fix64)0, (Fix64)0, (Fix64)0 },
                        m1_maxSpeed = (Fix64)8,
                        m1_moveForce = (Fix64)24,
                        m1_stopDampingAcceleration = (Fix64)32,
                        m1_angularSpeed = (Fix64)8,

                        //防御姿态移动
                        dm1_angleA = (Fix64)(30.0f / 180 * 3.14f),
                        dm1_angleB = (Fix64)(120.0f / 180 * 3.14f),
                        dm1_maxSpeed = new Fix64[] { (Fix64)0, (Fix64)0, (Fix64)0, (Fix64)0 },

                        //带球移动
                        normalSpeed_ball = (Fix64)7.72f,
                        m1_maxSpeed_ball = (Fix64)7,
                        m1_moveForce_ball = (Fix64)21,
                        m1_stopDampingAcceleration_ball = (Fix64)28,
                        m1_angluarSpeed_ball = (Fix64)4,
                        
                        //AI转动角速度
                        m1_angluarSpeed_ball_ai = (Fix64)40,
                        m1_angularSpeed_ai = (Fix64)40,

                        //转身
                        m2_minAngleCos = (Fix64)(-0.85f), //145°+
                        m2_minSpeedAndWaitTime = new Fix64[] { (Fix64)0.1f, (Fix64)0.2f, (Fix64)(-0.1f), (Fix64)0.2f },
                        m2_minSpeedAndWaitTime_ball = new Fix64[] { (Fix64)0.1f, (Fix64)0.2f, (Fix64)(-0.1f), (Fix64)0.2f },

                        m2_movingTime = (Fix64)0.2f,

                        //铲球
                        st_initialSpeed = (Fix64)40,
                        st_dampingToZeroTime = (Fix64)0.4f,
                        st_waitingTime = (Fix64)0.2f,
                        st_maxSlidingTargetDistance = (Fix64)3.0f,
                        st_maxSlidingTargetAngle = (Fix64)(180.0f / 180 * 3.14f),
                        

                        //被铲
                        bst1_initialSpeed = (Fix64)10.0f,
                        bst1_dampingToZeroTime = (Fix64)1.0f,
                        //bst1_standWaitingTime = (Fix64)1.25f,
                        bst1_waitingTime = (Fix64)0.8f,
						st_beAttackedFaceBackoff = 0,

                        //过人
                        bst2_speed = (Fix64)1,
                        bst2_waitingTime = (Fix64)0.25f,

                        //无球过人
                        bst3_speed = (Fix64)1,
                        bst3_waitingTime = (Fix64)0.25f,

                        //预防错误
                        moveStateWaitingTime = (Fix64)0.05f,

                        //传球
                        pb_beforePassingTime = new Fix64[] { (Fix64)0.2f, (Fix64)0.16f },
                        pb_afterPassingTime = new Fix64[] { (Fix64)0.3f, (Fix64)0.8f },

                        //射门
                        sb_angularSpeed = (Fix64)6,
                        sb_beforeShootingTime = new Fix64[] { (Fix64)0.13f, (Fix64)0.13f, (Fix64)0.16f, (Fix64)0.024f },
                        sb_afterShootingTime = new Fix64[] { (Fix64)1.2f, (Fix64)0.57f, (Fix64)0.54f, (Fix64)0.73f },
                        sb_ShootBallZone = new byte[] { 0, 0, 0, 0 },

                        shootBallAngleTorelance = (Fix64)(30.0f / 180 * 3.14f),
                        maxGoalSpeed = new Fix64[] { (Fix64)40f, (Fix64)50f, (Fix64)60f, (Fix64)80f },

                        maxGoalDistance = (Fix64)35.0f,


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
                        scb_maxRadius = (Fix64)1.08f,
                        scb_maxCathingTime = (Fix64)0.3f,
                        scb_catchingAniTime = new Fix64[] { (Fix64)0.1f, (Fix64)0.13f, (Fix64)0.1f },
                        scb_catchingHeightLimit = new Fix64[] { (Fix64)1.0f, (Fix64)3.0f, (Fix64)0.0f },
                        scb_catchingOffset = new Fix64[] { (Fix64)1.08f, (Fix64)0.085f, (Fix64)0.0f },
                        scb_catchingOffsetH = new Fix64[] { (Fix64)0.31f, (Fix64)1.768f, (Fix64)0.0f },
                        scb_lockTimeAfterCatching = new Fix64[] { (Fix64)0.4f, (Fix64)0.57f, (Fix64)0.0f },

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
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.75 },
                            //站立接短传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)1.08 },
                            //跑步接短传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.9 },
                            //站立接长传
							new FixVector3 { x = (Fix64)0, y = (Fix64)1.768, z = (Fix64)0.085 },
                            //跑步接长传
							new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.9 },
                        },

                        ballDetachPositions = new FixVector3[]
                        {
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.75 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.75 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.75 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.75 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.75 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)13.58, z = (Fix64)1.08 },
                        },


                        ballDetachPositionsAttacked = new FixVector3[]
                        {
                            new FixVector3 { x = (Fix64)0.0, y = (Fix64)0.31, z = (Fix64)0.9 },
                        },
                        ballDetachSpeedAttacked = new Fix64[] { (Fix64)0 },
                        ballDetachSlerpTimeAttacked = new Fix64[] { (Fix64)0.1f },


                        lastBallSamplePositions = new FixVector3[]
                        {
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.75 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0.31, z = (Fix64)0.9 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)1.768, z = (Fix64)0.085 },
                            new FixVector3 { x = (Fix64)0, y = (Fix64)0, z = (Fix64)0 },
                        },


                        maxSkillCastingTime = (Fix64)100.0f,
                        maxSkillLockTimeAfterCasting = (Fix64)0.2f,


                        shortPassEnergy = 4,
                        longPassEnergy = 10,
                        chargeIncreaseEnergyInterval = (Fix64)0.2,
                        tauntTime = (Fix64)2,
                        cheerUniqueTime = (Fix64)2.16,

                        element = FiveElements.Fire,
                    };
                }
                return s_default;
            }
        }

        static Configuration s_default = null;
        
    }
}