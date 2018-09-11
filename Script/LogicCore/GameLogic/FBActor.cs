using BW31.SP2D;
using FixMath.NET;

public enum FBTeam
{
    kNone = 0,
    kRed = 1,
    kBlue = 2
}


public enum BallDetachType
{
    ShortPass,  //短传
    LongPass,   //长传
    NormalShoot,//普通射门
    PowerShoot,  //大力射门
    SuperShoot, //超级射门
    KillerShoot,    //必杀射门
}

public enum BallDetachType_Attacked
{
    None = -1,
    Normal,     //正常被攻击掉球
    //
};

public enum BallAttachType
{
    Idle,
    Short_StandGetPassingBall,//站立接短传
    Short_RuningGetPassingBall,//跑步接短传
    Long_StandGetPassingBall,   //站立接长传
    Long_RuningGetPasssingBall, //跑步接长传
}



public enum LastBallSamplePositionType
{
    Idle,
    Run,
    Chest,
    Head,
}

//阵营
public enum CampType
{
    //进攻方
    Attack,

    //防守方
    Defence,
}

//射门类型
public enum ShootType
{
    //普通
    Normal,
    //大力
    Power,
    //超级
    Super,
    //必杀射门
    Killer
}

public enum FiveElements
{
    None = 0,
    Fire,
    Water,
}

public partial class FBActor : ForceGenerator, FBWorld.IElement
{

    public enum MovingState
    {
        kIdle = 0,
        kMoving,
        kAction,
        kMovingToIdle
    }

    public FBActor(Configuration configuration = null)
    {
        ignoreCollision = false;
        sliding = false;
        if (configuration == null)
        {
            //configuration = Configuration.getConfiguration(1);
            configuration = Configuration._default;
        }
        m_configuration = configuration;
        m_particle.addForceGenerator(this);
        m_particle.radius = configuration.radius;
        m_particle.tag = this;
        movingState = MovingState.kIdle;
        initialDefaultState();
    }

    //测试代码-fbactor_configuration
    public FBActor(Configuration configuration, uint baseID, bool mainActor)
    {

        ignoreCollision = false;
        sliding = false;

        roleId = baseID;
        if (configuration == null)
        {
            //configuration = Configuration.getConfiguration(1);
            if (baseID == 1)
            {
                configuration = Configuration._default;
            }
            else if (baseID == 2)
            {
                configuration = Configuration_2._default;
            }
            else if (baseID == 3)
            {
                configuration = Configuration_3._default;
            }
            else if (baseID == 4)
            {
                configuration = Configuration_4._default;
            }
            else if (baseID == 5)
            {
                configuration = Configuration_5._default;
            }
            else if (baseID == 6)
            {
                configuration = Configuration_6._default;
            }
            else
            {
                configuration = null;
            }
        }
        m_configuration = configuration;
        m_particle.addForceGenerator(this);
        m_particle.radius = configuration.radius;
        m_particle.tag = this;
        movingState = MovingState.kIdle;
        this.mainActor = mainActor;
        initialDefaultState();
    }

    //逻辑对象的编号
    protected uint _id;
    public uint id
    {
        get { return _id; }
        set { _id = value; }
    }

    public FBTeam team = FBTeam.kNone;
    public string name { get; set; }

    public Configuration configuration { get { return m_configuration; } }

    public FBWorld world { get { return m_world; } }
    public Particle particle { get { return m_particle; } }
    public FixVector2 direction
    {

        get { return m_direction; }
        set
        {
            if (value != m_direction)
            {
                m_direction = value;
            }
        }

    }

    public bool ignoreCollision { get; private set; }
    public bool sliding { get; private set; }

    public Fix64 kickBallLastingTime = Fix64.Zero;
    private bool _shootBallEvent = false;
    public bool shootBallEvent
    {
        get
        {
            return _shootBallEvent;
        }
        set
        {
            _shootBallEvent = value;
        }
    }

    public MovingState movingState { get; private set; }

    public bool catchBallHelperEnabled = true;

    public FixVector2 moveDirection { get { return m_moveDirection; } }
    public Fix64 movePower { get { return m_movePower; } }

    public ShootType shootingType { get; private set; }

    // 是否在踢球状态，eg.如果是，则忽略和球的碰撞
    public bool isKickingBall { get; private set; }

    public bool mainActor { get; private set; }


    public FixVector2 getPosition() { return particle.position; }

    public FixVector3 getLastBallSamplePosition()
    {
        LastBallSamplePositionType lastSampleType = LastBallSamplePositionType.Idle;
        if (checkMovingState())
        {
            lastSampleType = particle.velocity != FixVector2.kZero ? LastBallSamplePositionType.Run : LastBallSamplePositionType.Idle;
        }
        else if (m_currentState == StandCatchingBall.instance)
        {
            lastSampleType = LastBallSamplePositionType.Idle;
        }
        else if (m_currentState == AirCatchingBall.instance)
        {
            lastSampleType = LastBallSamplePositionType.Chest;
        }
        else if (m_currentState == DoorKeeperCatchingBall.instance)
        {
            return new FixVector3(this.m_stateVector.x, this.m_stateValue2, this.m_stateVector.y);
        }

        return getBallPosition(lastSampleType);
    }

    public FixVector3 getBallPosition(BallAttachType type)
    {
        var ballPos = calculateRelativePosition(configuration.ballAttachPositions[(int)type]);
        return ballPos;
    }

    public FixVector3 getBallPosition(BallDetachType type)
    {
        var ballPos = calculateRelativePosition(configuration.ballDetachPositions[(int)type]);
        return ballPos;
    }
    public FixVector3 getBallPosition(BallDetachType_Attacked type)
    {
        var ballPos = calculateRelativePosition(configuration.ballDetachPositionsAttacked[(int)type]);
        return ballPos;
    }

    public FixVector3 getBallPosition(LastBallSamplePositionType type)
    {
        var ballPos = calculateRelativePosition(configuration.lastBallSamplePositions[(int)type]);
        return ballPos;
    }


    FixVector3 calculateRelativePosition(FixVector3 position)
    {
        var sinA = direction.y;
        var cosA = direction.x;
        var v = new FixVector2 { x = position.z, y = -position.x };
        FixVector3 vec = new FixVector3
        {
            x = v.x * cosA + v.y * (-sinA),
            y = position.y,
            z = v.x * sinA + v.y * cosA
        };

        vec += new FixVector3(particle.position.x, Fix64.Zero, particle.position.y);

        return vec;

    }

    public void reset(FixVector3 pos)
    {
        ignoreDirectionSlerp = true;
        ignorePositionSampleSlerp = true;
        forceCheck = true;
        particle.position = new BW31.SP2D.FixVector2(pos.x, pos.z);
    }

    public void move(FixVector2 direction, Fix64 power)
    {
        if (power > Fix64.Zero && direction != FixVector2.kZero)
        {
            m_moveDirection = direction;
            m_movePower = power;
        }
        else
        {
            stop();
        }
    }

    public void move(FixVector2 direction)
    {
        if (direction != m_moveDirection)
        {
            //Debuger.Log(string.Format("frame:{0} actor:{1} dx:{2:X} dy:{3:X}", world.world.frameCount, id, direction.x.RawValue, direction.y.RawValue));
        }

        if (direction != FixVector2.kZero)
        {
            m_moveDirection = direction;
            m_movePower = Fix64.One;
        }
        else
        {
            stop();
        }
    }

    public void stop()
    {
        if (m_movePower != Fix64.Zero)
        {
            //Debuger.Log(string.Format("frame:{0} actor:{1} stop", world.world.frameCount, id));
        }
        m_moveDirection = FixVector2.kZero;
        m_movePower = Fix64.Zero;
    }

    State m_defautState = Movement.instance;
    void initialDefaultState()
    {
        m_defautState = Movement.instance;
    }

    State m_RunTimeMovementState = null;
    void setToMovementState()
    {
        if (m_RunTimeMovementState == null)
            m_nextState = m_defautState;
        else
            m_nextState = m_RunTimeMovementState;
    }

    public bool isSkilling
    {
        get { return m_currentState == Skilling.instance; }
    }

    public bool doSkill()
    {
        return _setNextState(Skilling.instance);
    }

    public bool doPassBall(int index, FixVector2 direction)
    {
        if (_setNextState(PassBall.instance, false))
        {
            m_stateDataIndex = index;
            m_stateVector = direction;

            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool doShootBall(FixVector2 direction)
    {
        if (_setNextState(ShootBall.instance, false))
        {
            m_stateVector = direction;
            chargeIncreaseEnergyTimer = Fix64.Zero;
            beginCheckShootBall(direction);
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool doCathingGoalBall(FixVector2 ballTargetPosition, Fix64 ballHeight, Fix64 ballFlyTime, int zoneIndex, bool pretendCathingBall)
    {
        if (_setNextState(DoorKeeperCatchingBall.instance, false))
        {
            m_stateVector = ballTargetPosition;
            m_stateValue2 = ballHeight;
            m_stateValue = ballFlyTime;
            m_stateDataIndex = zoneIndex;
            m_stateBool = pretendCathingBall;
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool doGetPassingBallWhenStand(Fix64 passingBallNeedTime, FixVector2 facingDirection)
    {
        if (_setNextState(GetPassingBallWhenStand.instance, false))
        {
            m_stateVector = facingDirection;
            m_stateValue = passingBallNeedTime;
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool doGetPassingBallWhenMoving(Fix64 passingBallNeedTime)
    {
        if (_setNextState(GetPassingBallWhenMoving.instance, false))
        {
            m_stateValue = passingBallNeedTime;
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool startDefend(FBActor target)
    {
        if (target == null)
            return false;
        if (_setNextState(DefendMovement.instance, false))
        {
            m_stateActor = target;
            m_stateBall = null;
            m_currentState.enter(this);
            return true;
        }
        return false;
    }
    public bool startDefend(FBBall target)
    {
        if (target == null)
            return false;
        if (_setNextState(DefendMovement.instance, false))
        {
            m_stateBall = target;
            m_stateActor = null;
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public void stopDefend()
    {
        m_RunTimeMovementState = null;
        m_stateBall = null;
        m_stateActor = null;
    }

    public bool doTigerCatchingBall()
    {
        if (_setNextState(TigerCatchingBall.instance, false))
        {
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool doSliding(FixVector2 direction)
    {
        if (_setNextState(Sliding.instance, false))
        {
            this.direction = direction;
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool beSlid_dropBall()
    {
        return _setNextState(BeSlid_DropBall.instance);
    }

    public bool beAttacked(FBActor attacker, BallDetachType_Attacked type)
    {
        if (_setNextState(BeAttacked.instance, false))
        {
            m_stateInt = (int)type;
            m_stateActor = attacker;
            m_currentState.enter(this);
            return true;
        }
        return false;
    }

    public bool beSlid_keepBall()
    {
        return _setNextState(BeSlid_KeepBall.instance);
    }

    public bool beSlid_noBall()
    {
        return _setNextState(BeSlid_NoBall.instance);
    }

    public bool doWarmUp()
    {
        return _setNextState(WarmUp.instance);
    }

    public bool doTaunt()
    {
        return _setNextState(Taunt.instance);
    }

    public bool doCheerStand()
    {
        return _setNextState(CheerStand.instance);
    }

    public bool doCheerUnique()
    {
        return _setNextState(CheerUnique.instance);
    }

    public bool doDismay()
    {
        return _setNextState(Dismay.instance);
    }

    public void doIdle()
    {
        setToMovementState();
    }

    void ForceGenerator.update(Particle particle, Fix64 deltaTime)
    {
        m_currentState.update(this, deltaTime);
        if (m_nextState != null)
        {
            m_currentState.leave(this);
            m_currentState = m_nextState;
            m_nextState = null;
            m_currentState.enter(this);
        }
    }

    bool _setNextState(State state, bool enter = true)
    {
        if (m_currentState == state)
        {
            return false;
        }

        if (m_currentState != null)
        {
            if (!m_currentState.canBreak(this, state))
            {
                return false;
            }
            m_currentState.leave(this);
        }
        m_currentState = state;
        if (enter && state != null)
        {
            state.enter(this);
        }
        return true;
    }

    bool _updateDirectionByVelocity()
    {
        var d = m_particle.velocity.normalized;
        if (d != FixVector2.kZero)
        {
            direction = d;
            return true;
        }
        return false;
    }

    void _setMovingState(MovingState state)
    {
        if (movingState != state)
        {
            movingState = state;
            world.onActorMovingStateChanged(this);
        }
    }

    void updateMovingState()
    {
        if (m_particle.velocity == FixVector2.kZero)
        {
            _setMovingState(MovingState.kIdle);
        }
        else if (m_movePower == Fix64.Zero)
        {
            _setMovingState(MovingState.kMovingToIdle);
        }
        else
        {
            _setMovingState(MovingState.kMoving);
        }
    }

    abstract class State
    {
        public virtual bool canBreak(FBActor actor, State state) { return true; }
        public virtual void enter(FBActor actor) { }
        public abstract void update(FBActor actor, Fix64 deltaTime);
        public virtual void leave(FBActor actor) { }
    }

    Particle m_particle = new Particle();
    State m_currentState = Movement.instance;
    State m_nextState = null;
    Configuration m_configuration;

    FixVector2 m_direction = FixVector2.kDown;
    Fix64 m_timer = Fix64.Zero;
    int m_stateDataIndex = 0;
    int m_stateSubState = 0;
    FixVector2 m_stateVector;
    FBActor m_stateActor = null;
    FBBall m_stateBall = null;
    Fix64 m_stateValue = Fix64.Zero;
    Fix64 m_stateValue2 = Fix64.Zero;
    bool m_stateBool = false;
    int m_stateInt = 0;




    Fix64 m_originalDamping;

    FixVector2 m_moveDirection = FixVector2.kZero;
    Fix64 m_movePower = Fix64.Zero; // [0, 1]

    FBWorld m_world;

    public bool AIing { get; set; }

    void FBWorld.IElement.setWorld(FBWorld world)
    {
        m_world = world;
    }

    /// <summary>
    /// 计算跑步时的动画速度
    /// </summary>
    /// <returns></returns>
    Fix64 calculateAnimatorSpeedOfRun(int moveType)
    {
        if (m_configuration.normalSpeed[moveType] == Fix64.Zero) return Fix64.Zero;
        return particle.velocity.length / m_configuration.normalSpeed[moveType];
    }

    public uint roleId = 0;

    public FixVector2 getSelfDoorPosition()
    {
        return world.getDoorPosition(team, true);
    }

    public FixVector2 getEnemyDoorPosition()
    {
        return world.getDoorPosition(team, false);
    }

    public bool isDoorKeeper()
    {
        if (configuration != null && configuration.dkcb_edgeLimit != null && configuration.dkcb_edgeLimit.Length > 0)
            return true;
        return false;
    }
}
