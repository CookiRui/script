using System.Collections.Generic;
using FixMath.NET;
using BW31.SP2D;
using Cratos;

public partial class FBActor
{
    HashSet<FBActor> m_slidingTargets = new HashSet<FBActor>();

    public bool checkSlidingTarget(FBActor actor)
    {
        if (actor == this || actor.ignoreCollision)
        {
            return false;
        }
        if (actor.team == this.team)
            return false;
        if (actor.isDoorKeeper())
            return false;

        if (m_slidingTargets.Contains(actor))
        {
            return false;
        }
        //var s = actor.m_particle.radius + m_particle.radius;
        var s = actor.m_particle.radius + m_particle.radius + (Fix64)0.1f;
        if (m_particle.position.squareDistance(actor.m_particle.position) <= s * s)
        {
            m_slidingTargets.Add(actor);
            return true;
        }
        return false;
    }

    public bool checkMovingState()
    {
        return m_currentState == Movement.instance || m_currentState == DefendMovement.instance;
    }
    public bool checkSlidingState()
    {
        return m_currentState == Sliding.instance;
    }

    public bool checkPassBallState()
    {
        return m_currentState == PassBall.instance;
    }

    public bool checkShootBallState()
    {
        return m_currentState == ShootBall.instance;
    }

    public bool checkTigerCatchingBall()
    {
        return m_currentState == TigerCatchingBall.instance;
    }

    public bool checkBallCatchingWhenMoving(FBBall ball, out Fix64 weight)
    {
        weight = Fix64.Zero;
        if (!(m_currentState == Movement.instance || m_currentState == DefendMovement.instance))
        {
            return false;
        }
        //如果没有速度的情况下，不处理此种情况的拿球
        if (particle.velocity == FixVector2.kZero)
            return false;


        if (ball.get3DPosition().y > m_configuration.maxCatchingBallHeight)
        {
            return false;
        }

        var s = this.configuration.catchingRadius;
        var d = m_particle.position.distance(ball.particlePosition);

        if (d > s)
        {
            return false;
        }
        //jlx 2017.04.24-log:在配置角度（catchingAngle）内才能被捕获
        var dir = (ball.particlePosition - m_particle.position).normalized;
        var dot = FixVector2.dot(direction, dir);
        if (dot <= m_configuration.catchingAngle)
        {
            return false;
        }

        if (s != Fix64.Zero)
        {
            weight = Fix64.One - d / s;
        }
        return true;
    }



    public bool checkWarmUpState()
    {
        return m_currentState == WarmUp.instance;
    }

    public bool checkTauntState()
    {
        return m_currentState == Taunt.instance;
    }


    public bool checkCheerUniqueState()
    {
        return m_currentState == CheerUnique.instance;
    }

    /// <summary>
    /// 自己控球
    /// </summary>
    /// <returns></returns>
    public bool isCtrlBall()
    {
        return world.ball.owner == this;
    }

    /// <summary>
    /// 队友控球
    /// </summary>
    /// <returns></returns>
    public bool isMateCtrlBall()
    {
        if (world.ball.hasOwner)
            return world.ball.owner.team == team;

        if (world.ball.kicker == null) return false;

        return world.ball.kicker.team == team;
    }

    Fix64 _passBallTimeSum = Fix64.Zero;
    bool _passBallPressed = false;
    public bool passBallPressed
    {
        get { return _passBallPressed; }
    }

    public void beginCheckPassBall()
    {
        if (_passBallPressed || _shootBallPressed || this.m_currentState != Movement.instance)
            return;

        _passBallPressed = true;
        _passBallTimeSum = Fix64.Zero;
    }

    public void endCheckPassBall(FixVector2 moveDirection)
    {
        if (!_passBallPressed)
            return;

        //Debuger.Log("endCheckPassBall");

        _passBallPressed = false;

        //长传
        if (_passBallTimeSum > ConstTable.LongPassBallPressTime)
        {
            world.beginPassBall(this, moveDirection, 1);
        }
        else//短传
        {
            world.beginPassBall(this, moveDirection, 0);
        }
    }
    public void checkPassBallState(Fix64 timeDelta)
    {
        if (!_passBallPressed)
            return;
        if (!isCtrlBall())
        {
            _passBallPressed = false;
            return;
        }
        _passBallTimeSum += timeDelta;
        if (_passBallTimeSum > ConstTable.LongPassBallPressTime)
        {
            endCheckPassBall(m_moveDirection);
        }

    }


    Fix64 _shootBallTimeSum = Fix64.Zero;
    bool _shootBallPressed = false;
    Fix64 chargeIncreaseEnergyTimer;
    public void checkShootBallState(Fix64 timeDelta)
    {
        if (!_shootBallPressed)
            return;
        _shootBallTimeSum += timeDelta;
        if (_shootBallTimeSum > ConstTable.SuperShootPressTime)
        {
            endCheckShootBall();
        }
        chargeIncreaseEnergyTimer += timeDelta;
        if (chargeIncreaseEnergyTimer >= configuration.chargeIncreaseEnergyInterval)
        {
            world.ball.increaseEnergy();
            chargeIncreaseEnergyTimer -= configuration.chargeIncreaseEnergyInterval;
        }
    }

    //右脚1 ，左脚0
    int getKickBallFoot(FixVector2 dest, FixVector2 cur)
    {
        int defaultFoot = this.configuration.defautKickBallFoot;
        Fix64 cross = FixVector2.cross(dest, this.direction);
        if (cross < Fix64.Zero)
        {
            defaultFoot = 1;
        }
        if (cross > Fix64.Zero)
        {
            defaultFoot = 0;
        }

        return defaultFoot;
    }

    public void beginCheckShootBall(FixVector2 destTurnDirection)
    {
        if (_shootBallPressed || _passBallPressed)
            return;
        _shootBallPressed = true;
        shootBallEvent = false;
        int kickBallFoot = getKickBallFoot(destTurnDirection, this.direction);

        world.onShootBallBegin(this, kickBallFoot == 1);

    }

    public void shootBallDirectly(ShootType shootType, FixVector2 direction)
    {
        if (!this.isCtrlBall())
            return;

        doShootBall(direction);
        m_stateDataIndex = (int)shootType;
        shootingType = shootType;
        shootBallEvent = true;
        _shootBallPressed = false;
        _shootBallTimeSum = Fix64.Zero;
    }

    public void endCheckShootBall()
    {
        if (!_shootBallPressed)
            return;

        //Debuger.Log("endCheckShootBall");
        _shootBallPressed = false;
        //超级射门
        if (_shootBallTimeSum > ConstTable.SuperShootPressTime)
        {
            if ((Fix64)world.ball.getEnergy().value > world.ball.configuration.killerShootEnergy)
            {
                m_stateDataIndex = (int)ShootType.Killer;
            }
            else
            {
                m_stateDataIndex = (int)ShootType.Super;
            }
        }//大力射门
        else if (_shootBallTimeSum > ConstTable.HardShootPressTime)
        {
            m_stateDataIndex = (int)ShootType.Power;

        }//普通射门
        else
        {
            m_stateDataIndex = (int)ShootType.Normal;
        }

        //m_stateDataIndex = (int)ShootType.Killer;

        shootingType = (ShootType)m_stateDataIndex;

        shootBallEvent = true;
        _shootBallTimeSum = Fix64.Zero;
    }

    void changeLockState(bool isLock)
    {
        if (mainActor)
        {
            LogicEvent.fire2Rendering("onEnableRecordInput", isLock);
        }
    }

    public void debugFrameLogic()
    {
        string outputText = string.Format(
            "frameID:{0} actor: {1} velocity:{2:X},{3:X} position:{4:X},{5:X} acceleration:{6:X},{7:X} ",
            world.fbGame.frameSync.currentLogicFrameNum,
            id,
            particle.velocity.x.RawValue, particle.velocity.y.RawValue,
            particle.position.x.RawValue, particle.position.y.RawValue,
            particle.acceleration.x.RawValue, particle.acceleration.y.RawValue
            );
        Debuger.LogLogic(outputText);
    }
}