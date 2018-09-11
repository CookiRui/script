using BW31.SP2D;
using FixMath.NET;

public partial class FBBall : FBWorld.IElement
{
    //逻辑对象的编号
    protected uint _id;
    public uint id
    {
        get { return _id; }
        set { _id = value; }
    }

    public FBWorld world { get { return m_world; } }
    //public NewBallParticle particle { get { return m_particle; } }

    public bool hasOwner { get { return m_owner != null; } }
    public FBActor owner
    {
        get { return m_owner; }
    }

    public FBActor kicker { get { return m_kicker; } }

    public bool willBeCatched
    {
        get { return m_willBeCatched; }
    }

    public FBActor transferTarget = null;

    //public FixVector3 getPosition()
    //{
    //    FixVector3 pos = FixVector3.kZero;
    //    if (m_owner != null)
    //    {
    //        pos = owner.getBallPosition();
    //    }
    //    else
    //    {
    //        pos = new FixVector3(m_particle.position.x, m_particle.height, m_particle.position.y);
    //    }

    //    return pos;
    //}


    public FixVector2 getPosition()
    {
        if (m_owner != null)
        {
            FixVector3 pos = owner.getBallPosition(BallAttachType.Idle);

            return new FixVector2(pos.x, pos.z);
        }
        else
        {
            return m_particle.position;
        }
    }

    public FixVector3 get3DPosition()
    {
        if (m_owner != null)
            return owner.getBallPosition(BallAttachType.Idle);
        else
            return new FixVector3(m_particle.position.x, particleHeight, m_particle.position.y);
    }

    public Fix64 particleHeight { get { return m_particle.height; } }
    public FixVector2 particlePosition { get { return m_particle.position; } }
    public FixVector2 particleVelocity { get { return m_particle.velocity; } }

    public bool landed { get { return m_particle.landing; } }
    ParticleContact? collidedContact;
    FixVector3 collidedVelocity;
    Energy energy;

    public BallState ballState { get; set; }

    public void reset(FixVector3 pos)
    {
        if (m_owner != null)
        {
            world.onOwnerDetached(m_owner);
            m_owner = null;
        }
        m_particle.reset(new FixVector2(pos.x, pos.z), pos.y);
        world.world.addParticle(m_particle);
        m_kicker = null;
        m_willBeCatched = false;
        m_willBeCatchedActor = null;
        transferTarget = null;
        setEnergy(0);
        ballState = BallState.Free;
    }

    public void vanish()
    {
        world.world.removeParticle(m_particle);
    }

    // 球被某个球员直接获得
    public void transfer(FBActor actor)
    {
        if (actor == null || actor == m_owner)
        {
            return;
        }
        var old = m_owner;
        m_owner = actor;

        if (old != null)
        {
            world.onOwnerDetached(old);
            if (old.team != actor.team)
            {
                setEnergy(energy.changeTarget);
            }
        }
        else
        {
            world.world.removeParticle(m_particle);
            m_particle.notify_catched();
            if (m_kicker != null && m_kicker.team != actor.team)
            {
                setEnergy(energy.changeTarget);
            }
        }

        m_willBeCatched = false;
        m_willBeCatchedActor = null;
        transferTarget = null;
        m_kicker = null;
        world.onOwnerAttached(m_owner);
        ballState = m_owner.isDoorKeeper() ? BallState.GoalKeeper : BallState.Player;
    }


    public void freeByAttacked(FixVector3 start, FixVector2 velocity, Fix64 heightVelocity)
    {
        if (m_owner == null)
        {
            return;
        }
        m_particle.position = new FixVector2(start.x, start.z);
        m_particle.velocity = velocity;

        m_particle.notify_kickout(start.y, heightVelocity);

        _free_common(BallState.Free);
    }

    // 球变自由，由物理驱动
    public void freeByKick(FixVector3 start, FixVector2 velocity, Fix64 heightVelocity, bool isPassBall)
    {
        if (m_owner == null)
        {
            return;
        }
        m_particle.position = new FixVector2(start.x, start.z);
        m_particle.velocity = velocity;

        m_particle.notify_kickout(start.y, heightVelocity);
        _free_common(isPassBall ? BallState.PassBall : BallState.Shoot);
    }

    // 短传
    public void freeByShortPass(FixVector3 start, FixVector3 target, Fix64 time)
    {
        if (m_owner == null)
        {
            return;
        }

        m_particle.notify_landBall(new FixVector2(start.x, start.z), new FixVector2(target.x, target.z), time, m_configuration.dampingAcceleration_land);

        _free_common(BallState.PassBall);
    }

    // 中传
    public void freeByNormalPass(FixVector3 start, FixVector2 target, Fix64 airTime, Fix64 landTime)
    {
        if (m_owner == null)
        {
            return;
        }
        m_particle.notify_airLandBall(new FixVector2(start.x, start.z), start.y, target, airTime, landTime, m_configuration.linearDamping_air, m_configuration.linearDamping_land, m_configuration.landHitDamping);

        _free_common(BallState.PassBall);
    }

    // 长传
    public void freeByLongPass(FixVector3 start, FixVector2 target, Fix64 targetHeight, Fix64 time)
    {
        if (m_owner == null)
        {
            return;
        }

        m_particle.notify_airBall(new FixVector2(start.x, start.z), start.y, target, targetHeight, time, m_configuration.dampingAcceleration_air);

        _free_common(BallState.PassBall);
    }

    // 普通射门
    public void freeByNormalShoot(FixVector3 start, FixVector2 target, Fix64 targetHeight, Fix64 time)
    {
        if (m_owner == null)
        {
            return;
        }

        m_particle.notify_airBall(new FixVector2(start.x, start.z), start.y, target, targetHeight, time, m_configuration.dampingAcceleration_air);

        _free_common(BallState.Shoot);
    }

    //超级射门
    public void freeBySuperShoot(FixVector3 start, FixVector2 target, Fix64 targetHeight, Fix64 time, Fix64 bandAngle)
    {
        if (m_owner == null)
        {
            return;
        }

        m_particle.notify_bandBall(new FixVector2(start.x, start.z), start.y, target, targetHeight, time, m_configuration.dampingAcceleration_air, bandAngle);

        _free_common(BallState.Shoot);
    }

    public void collide(FixVector2 normal, Fix64 restitution)
    {
        if (m_owner != null)
        {
            return;
        }
        m_particle.notify_collideWith(normal, restitution);
    }

    public void willCatchBall(FBActor actor, FixVector2 target, Fix64 time)
    {
        m_willBeCatched = true;
        m_willBeCatchedActor = actor;

        m_particle.notify_willCatchInLand(target, time);
    }

    public void willCatchBall(FBActor actor, FixVector2 target, Fix64 time, Fix64 targetHeight)
    {
        m_willBeCatched = true;
        m_willBeCatchedActor = actor;
        m_particle.notify_willCatchInAir(target, time, targetHeight);
    }

    public void willCatchFreeBall(FBActor actor, FixVector2 target, Fix64 time, Fix64 targetHeight, Fix64 dampingAcceleration)
    {
        m_willBeCatched = true;
        m_willBeCatchedActor = actor;
        m_particle.notify_willFreeCatch(target, time, targetHeight, dampingAcceleration);
    }

    public void willCatchPassingBall(FBActor actor)
    {
        m_willBeCatched = true;
        m_willBeCatchedActor = actor;
    }

    public Location checkLocation(Fix64 mainExtentX, Fix64 doorExtentY)
    {

        FixVector2 position = getPosition();

        if (position.y > doorExtentY || position.y < -doorExtentY)
        {
            m_particle.location = Location.kNormal;
        }
        else if (position.x >= mainExtentX)
        {
            m_particle.location = Location.kRightDoor;
        }
        else if (position.x <= -mainExtentX)
        {
            m_particle.location = Location.kLeftDoor;
        }
        else
        {
            m_particle.location = Location.kNormal;
        }

        return m_particle.location;
    }

    public void onCollided(FBActor actor)
    {
        m_particle.notify_collided();
        if (m_willBeCatched)
        {
            if (actor != m_willBeCatchedActor)
            {
                m_willBeCatched = false;
                m_willBeCatchedActor = null;
            }
            else
            {
                ballState = BallState.Free;
            }
            // TODO:
        }
        else
        {
            ballState = BallState.Free;
        }
    }
    public void onCollided(ParticleContact contact)
    {
        m_particle.notify_collided();
        if (m_willBeCatched)
        {
            m_willBeCatched = false;
            m_willBeCatchedActor = null;
        }
        else
        {
            ballState = BallState.Free;
        }
        collidedContact = contact;
        collidedVelocity = get3DVelocity();
    }

    public EstimateHeightInfo estimateHeight(Fix64 time)
    {
        return m_particle.estimateHeight(time);
    }

    public bool estimateHit(FixVector2 edgeN, Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height)
    {
        return m_particle.estimateHit(ref edgeN, ref edgeD, out time, out point, out height);
    }

    public bool estimate(FixVector2 target, out Fix64 time, out Fix64 dampingAcceleration)
    {
        return m_particle.estimate(target, out time, out dampingAcceleration);
    }

    public void willBeFreezed()
    {
        m_particle.notify_willBeFreezed();
    }

    void _free_common(BallState state)
    {
        world.world.addParticle(m_particle);
        m_kicker = m_owner;
        m_owner = null;
        world.onOwnerDetached(m_kicker);
        ballState = state;
    }

    public FixVector3 get3DVelocity()
    {
        return new FixVector3 { x = particleVelocity.x, y = m_particle.heightVelocity, z = particleVelocity.y };
    }

    //public void moveToward(FixVector2 velocity, Fix64 flyVelocity, bool pass)
    //{
    //    m_willBeCatched = false;
    //    m_particle.clearCollidedEventFlag();
    //    m_particle.velocity = velocity;
    //    m_particle.dampingAcceleration_land = landDampingAcceleration;
    //    m_particle.dampingAcceleration_air = airDampingAcceleration;
    //    m_particle.landingDamping = landingDamping;
    //    m_particle.angularVelocity = Fix64.Zero;
    //    m_particle.fly(Fix64.Zero, flyVelocity, gravity);
    //    kickIsPass = pass;
    //}

    //public void moveToward(FixVector2 velocity, Fix64 angularVelocity, Fix64 flyVelocity, bool pass)
    //{
    //    m_willBeCatched = false;
    //    m_particle.clearCollidedEventFlag();
    //    m_particle.velocity = velocity;
    //    m_particle.dampingAcceleration_land = landDampingAcceleration;
    //    m_particle.dampingAcceleration_air = airDampingAcceleration;
    //    m_particle.landingDamping = landingDamping;
    //    m_particle.angularVelocity = angularVelocity;
    //    m_particle.fly(Fix64.Zero, flyVelocity, gravity);
    //    kickIsPass = pass;
    //}

    public FBBall(Configuration config, FBWorld.Configuration worldConfig)
    {
        m_configuration = config;
        m_particle = new NewBallParticle();
        m_particle.radius = config.radius;
        m_particle.linearDamping_land = config.linearDamping_land;
        m_particle.linearDamping_air = config.linearDamping_air;
        m_particle.landHitDamping = config.landHitVerticleDamping;
        m_particle.ceilHitDamping = worldConfig.ballCollisionRestitution[4];
        m_particle.ceilHeight = worldConfig.doorHeight;
        m_particle.angularDamping = config.angularDamping;
        m_particle.gravity = config.gravity;
        m_particle.tag = this;
        energy = config.getEnergy(0);
        m_particle.onLanded += (times, preHeightVelocity) =>
        {
            world.onBallLanded(m_particle.position, ballState == BallState.PassBall, times, get3DVelocity(), preHeightVelocity);
            ballState = BallState.Free;
        };
    }

    public void update(Fix64 deltaTime)
    {
        if (transferTarget != null)
        {
            transfer(transferTarget);
            transferTarget = null;
        }
        if (collidedContact.HasValue)
        {
            var contact = collidedContact.Value;
            if (contact.penetration > Fix64.Zero)
            {
                var hitPoint = particlePosition - contact.normal * m_particle.radius;
                var hitNormal = contact.normal;
                var point = new FixVector3 { x = hitPoint.x, y = particleHeight, z = hitPoint.y };
                var normal = new FixVector3 { x = hitNormal.x, z = hitNormal.y };
                if (contact.tagI == (int)ArenaObstacle.DoorSide.kOutDoor)
                {
                    world.onBallCollidedWall(point, normal, get3DVelocity(), kicker == null ? FiveElements.None : kicker.configuration.element);
                }
                else
                {
                    world.onBallCollidedNet(point, collidedVelocity, get3DVelocity(), kicker == null ? FiveElements.None : kicker.configuration.element);
                }
            }
            collidedContact = null;
        }

        if (energy.value > 0)
        {
            if (energy.decayTimeup)
            {
                setEnergy(energy.decayTarget);
            }
            else
            {
                energy.decayTimer += deltaTime;
            }
        }

#if UNITY_EDITOR
        if (BallView.instance != null)
        {
            while (BallView.instance.logicPosition.Count >= 128)
            {
                BallView.instance.logicPosition.RemoveFirst();
            }
            BallView.instance.logicPosition.AddLast(m_owner != null ? (UnityEngine.Vector3?)null : new UnityEngine.Vector3((float)m_particle.position.x, (float)m_particle.height, (float)m_particle.position.y));
        }
#endif
    }

    Configuration m_configuration = null;
    public Configuration configuration { get { return m_configuration; } }

    NewBallParticle m_particle = null;
    FBWorld m_world = null;
    FBActor m_owner = null, m_kicker = null;
    bool m_willBeCatched = false;
    public FBActor m_willBeCatchedActor = null;

    void FBWorld.IElement.setWorld(FBWorld world)
    {
        m_world = world;
    }

    public void debugFrameLogic()
    {
        string outputText = string.Format(
            "frameID:{0} ball: velocity:{1:X},{2:X} position:{3:X},{4:X} height:{5:X}",
            m_world.fbGame.frameSync.currentLogicFrameNum,
            m_particle.velocity.x.RawValue, m_particle.velocity.y.RawValue,
            m_particle.position.x.RawValue, m_particle.position.y.RawValue,
            m_particle.height.RawValue
            );
        Debuger.LogLogic(outputText);
    }

    public void increaseEnergy(byte value = 1)
    {
        if (value == 0)
            return;

        setEnergy((byte)(energy.value + value));
    }

    void setEnergy(byte value)
    {
        if (value > configuration.maxEnergy)
        {
            value = configuration.maxEnergy;
        }
        //Debuger.LogError("setEnergyValue old : " + energy.value + "   new:" + value);
        if (!energy.setValue(value))
        {
            var oldLevel = energy.level;
            energy = configuration.getEnergy(value);
            energy.setValue(value);
            world.onBallEnergyLevelChanged(oldLevel, energy.level);
        }
    }

    public Energy getEnergy()
    {
        return energy;
    }
}