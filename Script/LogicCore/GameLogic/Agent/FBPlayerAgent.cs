using FixMath.NET;
using BW31.SP2D;
using behaviac;
using Cratos;

public class FBPlayerAgent : FBAgentBase
{
    public FBActor actor { get; private set; }
    public PlayerState state { get; protected set; }
    public CoachCmd curCmd { get; private set; }

    protected override FBWorld world { get { return actor.world; } }

    protected override FBTeam team { get { return actor.team; } }

    protected override string btPath { get { return world.btConfig.playerBTPath; } }
    protected override int updateInterval { get { return 3; } }

    FixVector2 movingTarget;

    Fix64 chargeTime;
    protected Fix64 timer;
    bool playTaunt;
    Fix64 arriveThreshold;

    public FBPlayerAgent(FBActor actor, behaviac.Workspace workspace)
    {
        this.actor = actor;
        behaviour = createBehaviour(workspace);
        LogicEvent.add("onShootBallOut", this, "onShootBallOut");
        LogicEvent.add("onPlayTaunt", this, "onPlayTaunt");

    }
    ~FBPlayerAgent()
    {
        LogicEvent.remove(this);
    }

    protected virtual Agent createBehaviour(behaviac.Workspace workspace)
    {
        var behaviour = new BTPlayer();
        behaviour.Init(workspace);
        behaviour.agent = this;
        return behaviour;
    }

    #region private methods

    protected void onShootBallOut(uint id)
    {
        if (actor.id != id) return;
        if (state != PlayerState.Shoot) return;
        behaviour.btexec();
        behaviour.FireEvent("onShootBallOut");
    }

    Fix64 getChargeTime(ShootType type)
    {
        switch (type)
        {
            case ShootType.Normal:
                return world.randomUnit * ConstTable.HardShootPressTime;
            case ShootType.Power:
                return ConstTable.HardShootPressTime + (Fix64)0.9 * world.randomUnit * (ConstTable.SuperShootPressTime - ConstTable.HardShootPressTime);
            case ShootType.Super:
            case ShootType.Killer:
                return ConstTable.SuperShootPressTime;
        }
        return Fix64.Zero;
    }

    protected void onPlayTaunt(uint id)
    {
        if (id != actor.id) return;
        if (playTaunt) return;
        playTaunt = true;
    }

    #endregion

    #region public methods

    public void changeBehaviour(CoachCmd cmd)
    {
        //if (IdTest.instance != null)
        //{
        //    if (actor.id == IdTest.instance.id)
        //    {
        //        UnityEngine.Debug.LogError("changeBehaviour old " + curCmd + "  cur  " + cmd);
        //    }
        //}
        if (cmd == curCmd) return;

        curCmd = cmd;

        stopDefense();

        if (cmd == CoachCmd.Idle)
        {
            stop();
            behaviour.SetActive(false);
        }
        else
        {
            setBehaviour(cmd.ToString());
            behaviour.SetActive(true);
        }
    }

    public override void update(Fix64 deltaTime)
    {
        //if (IdTest.instance != null)
        //{
        //    if (actor.id == IdTest.instance.id)
        //    {
        //        UnityEngine.Debug.LogError("   state   " + state + "   cmd " + curCmd);
        //    }
        //}
        switch (state)
        {
            case PlayerState.Move:
                if (!isRunning()) return;


                var forward = movingTarget - actor.particle.position;
                var distance = forward.length;
                if (distance > Fix64.One)
                {
                    forward /= distance;
                }
                //if (actor.id == 5)
                //{
                //    UnityEngine.Debug.LogError(movingTarget + "    Move distance " + distance);
                //}
                actor.move(forward);
                break;
            case PlayerState.Charge:
                if (!isLessMinChargeTime() &&  chargeTime <= timer)
                {
                    if (isArrivedPowerChargeTime())
                    {
                        shoot(ShootType.Power);
                    }
                    else
                    {
                        shoot(ShootType.Normal);
                    }
                    return;
                }
                timer += deltaTime;
                break;
            case PlayerState.PassBall:
                timer += deltaTime;
                break;
        }
    }

    public void stop()
    {
        //if (actor.id == 5)
        //{
        //    UnityEngine.Debug.LogError("5555 stop  " + state);
        //}
        actor.stop();
        actor.particle.velocity = FixVector2.kZero;
        state = PlayerState.Idle;
    }

    public void setMoveTargetPosition(BT.Vector3 position)
    {
        behaviour.SetVariable("moveTargetPosition", position);
    }

    public void setPlayerId(uint id)
    {
        behaviour.SetVariable("playerId", id);
    }

    public override void clear()
    {
        LogicEvent.remove(this);
    }

    public void setPlayerTaunt()
    {
        behaviour.SetVariable("isTaunt", true);
    }

    #endregion

    #region 行为树调用的接口

    public FixVector2 calculateDefenceTargetPosition(FixVector2 position, Fix64 distance)
    {
        var doorPos = actor.getSelfDoorPosition();
        var dir = doorPos - position;
        if (dir.length < distance)
        {
            return doorPos;
        }
        return position + dir.normalized * distance;
    }

    public void defenseBall()
    {
        actor.startDefend(ball);
    }

    public void defensePlayer(uint playerId)
    {
        if (playerId <= 0)
        {
            Debuger.LogError("playerId <= 0 " + playerId);
            return;
        }
        actor.startDefend(world.getActor(playerId));
    }

    public Fix64 getTargetDistance(FixVector2 target)
    {
        var distance = actor.getPosition().distance(target);
        //Debuger.Log("distance : " + distance);
        return distance;
    }

    public bool isArrivedPosition(FixVector2 target, Fix64 threshold)
    {
        var distance = getTargetDistance(target);
        var isArrived = distance < (threshold < Fix64.Zero ? Fix64.Zero : threshold);
        //Debuger.Log("isArrivedPosition distance " + distance);
        return isArrived;
    }

    public EBTStatus passBall(uint playerId, PassBallType type)
    {
        //UnityEngine.Debug.LogError("passBall....id " + actor.id +"  target "+playerId );
        // UnityEngine.Debug.LogError("passBall....ball   Owner " + ball.owner.id);
        if (state != PlayerState.PassBall)
        {
            if (playerId <= 0)
            {
                Debuger.LogError("passBall playerId <= 0 " + playerId);
                return EBTStatus.BT_FAILURE;
            }

            var index = 0;
            if (type == PassBallType.Random)
            {
                index = world.randomValue % 2;
            }
            else
            {
                index = (int)type;
            }
            timer = Fix64.Zero;
            if (world.passBallToTarget(world.getActor(playerId), index))
            {
                state = PlayerState.PassBall;
            }
            else
            {
                return EBTStatus.BT_RUNNING;
            }
        }
        return actor.checkPassBallState() ? EBTStatus.BT_RUNNING : EBTStatus.BT_SUCCESS;
    }

    public FixVector2 getPlayerPosition(uint playerId)
    {
        if (playerId <= 0)
        {
            // Debuger.LogError("playerId <= 0 " + playerId);
            return FixVector2.kZero;
        }
        return world.getActor(playerId).getPosition();
    }

    public FixVector3 getBallPosition()
    {
        return ball.get3DPosition();
    }

    public EBTStatus shoot(ShootType type)
    {
        if (!actor.shootBallEvent)
        {
            actor.world.shootBallDirectly(actor, type);
            state = PlayerState.Shoot;
        }
        return EBTStatus.BT_RUNNING;
    }

    public EBTStatus attack()
    {
        if (state != PlayerState.Attack)
        {
            actor.doSliding(actor.direction);
            state = PlayerState.Attack;
        }
        return actor.checkSlidingState() ? EBTStatus.BT_RUNNING : EBTStatus.BT_SUCCESS;
    }

    public EBTStatus charge(ShootType type)
    {
        if (state != PlayerState.Charge)
        {
            world.beginCheckShootBall(FixVector2.kZero);
            state = PlayerState.Charge;
            chargeTime = getChargeTime(type);
            timer = Fix64.Zero;
        }
        return (actor.checkShootBallState() && timer < chargeTime) ? EBTStatus.BT_RUNNING : EBTStatus.BT_SUCCESS;
    }

    public virtual bool isAroundSafe(Fix64 range)
    {
        if (range <= Fix64.Zero)
        {
            return true;
        }
        var enemys = world.getEnemys(team);
        if (enemys == null || enemys.Count == 0) return true;

        var range2 = range * range;
        for (int i = 0; i < enemys.Count; ++i)
        {

            var distance = actor.getPosition().squareDistance(enemys[i].getPosition());
            if (distance < range2)
            {
                return false;
            }
        }
        return true;
    }


    public bool isArrivedPowerChargeTime()
    {
        return timer >= ConstTable.HardShootPressTime;
    }

    public Fix64 getRandomValue()
    {
        return world.randomUnit;
    }

    public bool hasBall()
    {
        return ball.owner == actor;
    }

    public EBTStatus run(FixVector2 position, Fix64 startDistance, Fix64 stopDistance)
    {
        //if (actor.id == 5)
        //{
        //    UnityEngine.Debug.LogError("run position:" + position);
        //}
        if (state != PlayerState.Move)
        {
            //if (state == PlayerState.Shoot && actor.id == 9)
            //{
            //    Debuger.LogError("  shoot to move !!!");
            //}
            if (isArrivedPosition(position, startDistance))
            {
                //if (actor.id == 5)
                //{
                //    UnityEngine.Debug.Log("run  BT_SUCCESS");
                //}
                return EBTStatus.BT_SUCCESS;
            }
            state = PlayerState.Move;
        }
        if (isArrivedPosition(position, stopDistance))
        {
            //if (actor.id == 5)
            //{
            //    UnityEngine.Debug.Log("run  BT_SUCCESS");
            //}
            return EBTStatus.BT_SUCCESS;
        }
        //LogicEvent.fire("onTestDrawPosition", position, stopDistance);
        movingTarget = position;
        arriveThreshold = stopDistance;
        return EBTStatus.BT_RUNNING;
    }

    public EBTStatus turnToPlayer(uint id)
    {
        var target = world.getActor(id);
        var dir = (target.getPosition() - actor.getPosition()).normalized;
        //Debuger.Log(FixVector2.dot(actor.direction, dir).ToString());
        if (FixVector2.dot(actor.direction, dir) > (Fix64)0.95)
        {
            stopDefense();
            return EBTStatus.BT_SUCCESS;
        }
        actor.startDefend(target);
        state = PlayerState.Turn;
        return EBTStatus.BT_RUNNING;
    }

    public EBTStatus turnToBall()
    {
        if (state != PlayerState.Turn)
        {
            actor.startDefend(ball);
            state = PlayerState.Turn;
        }
        var dir = ball.getPosition() - actor.getPosition();
        if (FixVector2.dot(actor.direction, dir) < Fix64.One)
        {
            return EBTStatus.BT_RUNNING;
        }
        stopDefense();
        return EBTStatus.BT_SUCCESS;
    }

    public bool isBallFree()
    {
        return world.isBallFree();
    }

    public EBTStatus warmUp()
    {
        if (state != PlayerState.WarmUp)
        {
            actor.doWarmUp();
            state = PlayerState.WarmUp;
        }
        if (playTaunt)
        {
            playTaunt = false;
            return EBTStatus.BT_SUCCESS;
        }
        return actor.checkWarmUpState() ? EBTStatus.BT_RUNNING : EBTStatus.BT_SUCCESS;
    }

    public EBTStatus taunt()
    {
        if (state != PlayerState.Taunt)
        {
            actor.doTaunt();
            state = PlayerState.Taunt;
        }
        return actor.checkTauntState() ? EBTStatus.BT_RUNNING : EBTStatus.BT_SUCCESS;
    }

    public FixVector2 calculateRandomMovePosition(Fix64 radius)
    {
        const int safeNumber = 10;
        int counter = 0;
        var curPosition = actor.getPosition();
        while (counter++ < safeNumber)
        {
            var randomRad = world.randomUnit * Fix64.PiTimes2;
            var sin = Fix64.FastSin(randomRad);
            var cos = Fix64.FastCos(randomRad);
            var position = new FixVector2 { x = cos, y = sin } * radius + curPosition;
            if (world.isInRange(position))
            {
                return position;
            }
        }
        return curPosition;
    }

    public FixVector2 calculateGoalTeamMovePosition(FixVector2 targetPosition, Fix64 minRadius, Fix64 maxRadius)
    {
        var direction = (actor.getPosition() - targetPosition).normalized;
        var randomRadius = minRadius + (maxRadius - minRadius) * world.randomUnit;
        return targetPosition + direction * randomRadius;
    }

    public void cheerStand()
    {
        if (state != PlayerState.CheerStand)
        {
            stop();
            actor.doCheerStand();
            state = PlayerState.CheerStand;
        }
    }

    public EBTStatus cheerUnique()
    {
        if (state != PlayerState.CheerUnique)
        {
            stop();
            actor.doCheerUnique();
            state = PlayerState.CheerUnique;
        }
        return actor.checkCheerUniqueState() ? EBTStatus.BT_RUNNING : EBTStatus.BT_SUCCESS;
    }

    public void dismay()
    {
        if (state != PlayerState.Dismay)
        {
            stop();
            actor.doDismay();
            state = PlayerState.Dismay;
        }
    }

    public void stopDefense()
    {
        actor.stopDefend();
    }

    public bool isRunning()
    {
        return state == PlayerState.Move && !isArrivedPosition(movingTarget, arriveThreshold);
    }

    public bool isLessMinChargeTime()
    {
        if (state != PlayerState.Charge) return false;
        return timer < world.btConfig.minChargeTime;
    }

    #endregion
}