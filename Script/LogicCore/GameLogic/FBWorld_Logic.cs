using FixMath.NET;
using BW31.SP2D;
using System;
using System.Collections.Generic;
using Cratos;

class FBActorTargetFindCompare : IComparer<FBActor>
{
    FixVector2 checkPoint = FixVector2.kZero;
    public FBActorTargetFindCompare(FixVector2 cp)
    {
        this.checkPoint = cp;
    }
    public int Compare(FBActor x, FBActor y)
    {
        Fix64 a = (x.getPosition() - checkPoint).length;
        Fix64 b = (y.getPosition() - checkPoint).length;
        if (a == b)
            return 0;
        else if (a > b)
            return 1;
        else
            return -1;
    }
}

public partial class FBWorld
{

    public bool passBallEvent = false;
    public uint passBallTargetId { get { return passBallTarget == null ? 0 : passBallTarget.id; } }
    FBActor passBallTarget = null;
    int passBallType = 0;//0短传 1长传
    FixVector2 passBallDir = FixVector2.kZero;


    public bool shootBallOutEvent = false;
    Fix64 positiveDoorSide = Fix64.Zero;

    public event Action<Location> onGoal;
    bool enableBT;

    void goal(Location location)
    {
        //进球
        if (onGoal != null)
        {
            onGoal(location);
        }
        //setBallOwner(null);
        //world.removeParticle(ball.particle);
    }

    void _willUpdate(Fix64 deltaTime) {

        beginSample();
    }

    void _update(Fix64 deltaTime)
    {
        if (ball != null)
        {
            ball.update(deltaTime);
        }

        if (passBallEvent)
        {
            passBallEvent = false;

            passBallOut();
        }
        else if (shootBallOutEvent)
        {
            shootBallOutEvent = false;

            shootBallOut();

        }
        else
        {
            var location = m_ball.checkLocation(m_mainExtent.x, m_doorExtent.y);
            if (location != Location.kNormal)
            {
                goal(location);
            }
            else if (!m_ball.hasOwner)
            {
                _processCatchBallWhenMoving();
            }
        }
        var actorCount = m_actors.Count;
        for (int i = 0; i < actorCount; i++)
        {
            var actor = m_actors[i];
            if (actor.sliding)
            {
                _processActorSliding(actor);
            }

            //检测传球
            actor.checkPassBallState(deltaTime);
            actor.checkShootBallState(deltaTime);
        }

        if (enableBT)
        {
            if (redCoach != null)
                redCoach.update(deltaTime);
            if (blueCoach != null)
                blueCoach.update(deltaTime);
            if (redGKCoach != null)
                redGKCoach.update(deltaTime);
            if (blueGKCoach != null)
                blueGKCoach.update(deltaTime);
        }


        endSample();

    }

    public void lateUpdate(Fix64 deltaTime)
    {
        if (enableBT)
        {
            if (redCoach != null)
                redCoach.updateBehaviour();
            if (blueCoach != null)
                blueCoach.updateBehaviour();
            if (redGKCoach != null)
                redGKCoach.updateBehaviour();
            if (blueGKCoach != null)
                blueGKCoach.updateBehaviour();
        }

        updateSkillContext(deltaTime);
    }

    public void doSliding(FBActor actor)
    {
        FBActor controlBallActor = null;
        List<FBActor> temp = new List<FBActor>();
        for (int i = 0; i < m_actors.Count; ++i)
        {
            FBActor target = m_actors[i];
            if (target == actor || target.ignoreCollision)
                continue;
            if (target.team == actor.team)
                continue;
            if (target.isDoorKeeper())
                continue;

            FixVector2 relativePos = target.getPosition() - actor.getPosition();
            if (relativePos.squareLength > actor.configuration.st_maxSlidingTargetDistance.square)
                continue;

            if (FixVector2.dot(actor.direction, relativePos.normalized) < Fix64.Cos(actor.configuration.st_maxSlidingTargetAngle))
                continue;

            if (target.isCtrlBall())
            {
                controlBallActor = target;
                break;
            }
            temp.Add(target);
        }

        if (controlBallActor != null)
        {
            actor.doSliding((controlBallActor.getPosition() - actor.getPosition()).normalized);
        }
        else if (temp.Count == 0)
        {
            actor.doSliding(actor.direction);
        }
        else
        {
            FixVector2 preferPoint = actor.getPosition();
            temp.Sort(new FBActorTargetFindCompare(preferPoint));
            actor.doSliding((temp[0].getPosition() - actor.getPosition()).normalized);

        }
    }

    void doorKeeperCathingBall(FBActor shooter, FBActor actor)
    {
        //球的轨迹和守门员的交点
        FixVector2 intersectPoint = FixVector2.kZero;
        Fix64 intersectHeight = Fix64.One;
        Fix64 ballFlyTime = Fix64.Zero;

        bool canHit = ball.estimateHit(actor.direction, -FixVector2.dot(actor.getPosition(), actor.direction), out ballFlyTime, out intersectPoint, out intersectHeight);
        if (!canHit)
        {
            Debuger.Log("Can not fly to doorkeeper");
            return;
        }

#if UNITY_EDITOR
        LogicEvent.fire("onDoorKeeperCatchingBallView", actor, intersectPoint, intersectHeight);
#endif

        FixVector2 toward = intersectPoint - actor.getPosition();
        Fix64 distance = toward.length;
        int cathingZoneIndex = -1;
        if (distance < actor.configuration.dkcb_edgeLimit[0])
        {
            if (intersectHeight < actor.configuration.dkcb_edgeLimit[1])
            {
                //a区
                cathingZoneIndex = 0;
            }
            else if (intersectHeight < actor.configuration.dkcb_edgeLimit[2])
            {
                //b区
                cathingZoneIndex = 1;
            }
            else if (intersectHeight < actor.configuration.dkcb_edgeLimit[3])
            {
                //c区
                cathingZoneIndex = 2;
            }
            else
            {
                //H区，直接返回，不处理
                cathingZoneIndex = -1;
            }
        }
        else if (distance < actor.configuration.dkcb_edgeLimit[6])
        {
            if (intersectHeight < actor.configuration.dkcb_edgeLimit[4])
            {
                //fe区
                cathingZoneIndex = 4;
            }
            else if (intersectHeight < actor.configuration.dkcb_edgeLimit[5])
            {
                //gd区
                cathingZoneIndex = 3;
            }
            else
            {
                cathingZoneIndex = -1;
            }
        }
        else
        {
            //H区，直接返回，不处理
            cathingZoneIndex = -1;
        }

        bool pretendCatchingBall = false;

        if ((cathingZoneIndex == 3 || cathingZoneIndex == 4))
        {
            //修改获取球的区域
            Fix64 rightDoorV = shooter.team == FBTeam.kBlue ? new Fix64(1) : new Fix64(-1);
            FixVector2 doorPosition = new FixVector2(config.worldSize.x * rightDoorV, Fix64.Zero);
            Fix64 s = (shooter.getPosition() - doorPosition).length;
            if (s > shooter.configuration.maxGoalDistance)
                s = shooter.configuration.maxGoalDistance;

            Fix64 shootRate = ConstTable.ShootBall_GoalRate[(int)shooter.shootingType]
                * ball.getEnergy().goalRate
                * ((Fix64)1.0f - s / shooter.configuration.maxGoalDistance);
            //失误计算
            Fix64 randV = this.randomUnit;
            pretendCatchingBall = randV >= shootRate;

            Debuger.Log("ShootBall GoalRate：" + (float)shootRate + " random:" + (float)randV);

            if (pretendCatchingBall)
            {
                if (cathingZoneIndex == 3)
                {
                    intersectHeight -= ConstTable.GoallKeeperPretendGetBallMissDistance;
                    if (intersectHeight < actor.configuration.dkcb_edgeLimit[4])
                        cathingZoneIndex = 4;
                }
                if (cathingZoneIndex == 4)
                {
                    intersectHeight += ConstTable.GoallKeeperPretendGetBallMissDistance;
                    if (intersectHeight > actor.configuration.dkcb_edgeLimit[4])
                        cathingZoneIndex = 3;
                }
            }
        }


        if (cathingZoneIndex != -1)
        {
            actor.doCathingGoalBall(new FixVector2(intersectPoint.x, intersectPoint.y), intersectHeight, ballFlyTime, cathingZoneIndex, pretendCatchingBall);
        }
    }

    FixVector3 getActorShootOutPosition(FBActor actor, ShootType shootType)
    {
        BallDetachType bt = BallDetachType.NormalShoot;
        if (shootType == ShootType.Normal)
            bt = BallDetachType.NormalShoot;
        else if (shootType == ShootType.Power)
            bt = BallDetachType.PowerShoot;
        else if (shootType == ShootType.Super)
            bt = BallDetachType.SuperShoot;
        else if (shootType == ShootType.Killer)
            bt = BallDetachType.KillerShoot;
        return actor.getBallPosition(bt);
    }

    void shootBallOut()
    {
        var actor = ball.owner;
        FixVector3 ballStartPosition = getActorShootOutPosition(actor, actor.shootingType);

        FixVector2 frontPosition = new FixVector2(ballStartPosition.x, ballStartPosition.z);
        Fix64 maxPassSpeed = actor.configuration.maxGoalSpeed[(int)actor.shootingType];

        Fix64 rightDoorV = actor.team == FBTeam.kBlue ? new Fix64(1) : new Fix64(-1);

        GoalZone zone = config.getGoalZone((int)actor.shootingType, (int)actor.configuration.sb_ShootBallZone[(int)actor.shootingType]);

        //随机位置
        FixVector2 targetPosition = new FixVector2(config.worldSize.x * rightDoorV, zone.center.x * positiveDoorSide);
        targetPosition += new FixVector2
        {
            x = config.doorHalfSize.x * new Fix64(2) * randomUnit,
            y = zone.halfSize.x * (randomUnit - (Fix64)0.5f)
        };

        //Debuger.Log("targetPosition:" + targetPosition);
        FixVector2 kickBallDirection = (targetPosition - frontPosition).normalized;

        Fix64 needTime = Fix64.Zero;
        Fix64 angle = Fix64.Zero;
        if (actor.shootingType == ShootType.Super)
        {
            Fix64 a = (targetPosition - frontPosition).length / new Fix64(2);
            Fix64 c = actor.configuration.curveBallRadius;
            if (c <= a)
                angle = actor.configuration.curveBallMaxAngle;
            else
            {
                Fix64 b = Fix64.Sqrt(c * c - a * a);
                angle = Fix64.Atan2(a, b);
                if (Fix64.FastAbs(angle) > actor.configuration.curveBallMaxAngle)
                {
                    angle = actor.configuration.curveBallMaxAngle;
                }
            }


            FixVector2 m = FixVector2.kZero - new FixVector2(config.worldSize.x * rightDoorV, Fix64.Zero);
            FixVector2 n = targetPosition - new FixVector2(config.worldSize.x * rightDoorV, Fix64.Zero);
            Fix64 cross_mn = FixVector2.cross(m, n);
            Fix64 angleDir = Fix64.Zero;
            if (cross_mn == Fix64.Zero)
                angleDir = actor.configuration.defautKickBallFoot == 0 ? Fix64.One : -Fix64.One;
            else if (cross_mn > Fix64.Zero)
                angleDir = -Fix64.One;
            else
                angleDir = Fix64.One;

            angle = angle * angleDir;
        }

        bool canShoot = BallParticle.calculateTime(frontPosition
            , targetPosition
            , maxPassSpeed
            , ball.configuration.dampingAcceleration_air
            , angle
            , out needTime);
        Fix64 goalHeight = Fix64.Zero;
        if (!canShoot)
        {
            ball.freeByKick(ballStartPosition, kickBallDirection * maxPassSpeed, maxPassSpeed, false);
        }
        else
        {
            //随机高度
            goalHeight = zone.center.y + zone.halfSize.y * (randomUnit - (Fix64)0.5);
            ball.freeBySuperShoot(ballStartPosition, targetPosition, goalHeight, needTime, angle);
        }

        FBActor doorKeeper = getDoorKeeper(actor.team == FBTeam.kBlue ? FBTeam.kRed : FBTeam.kBlue);
        if (doorKeeper != null && doorKeeper != actor)
        {
            doorKeeperCathingBall(actor, doorKeeper);
        }
        onShootBallOut(actor, ball.get3DVelocity(), angle, new FixVector3 { x = targetPosition.x, y = goalHeight, z = targetPosition.y });
    }

    public void shootBallDirectly(FBActor actor, ShootType type)
    {
        var direction = (actor.getEnemyDoorPosition() - actor.getPosition()).normalized;

        FixVector2 doorPosition = FixVector2.kZero;
        FixVector2 destDirection = FixVector2.kZero;
        positiveDoorSide = getDoorSide(actor, direction, out destDirection);
        actor.shootBallDirectly(type, direction);

    }

    Fix64 getDoorSide(FBActor actor, FixVector2 direction, out FixVector2 dest)
    {
        FixVector2 destDirection = direction;
        if (destDirection == FixVector2.kZero)
        {
            destDirection = actor.direction;
        }

        FixVector2 doorPosition = FixVector2.kZero;
        if (actor.team == FBTeam.kBlue)
            doorPosition = new FixVector2(config.worldSize.x, Fix64.Zero);
        else
            doorPosition = new FixVector2(-config.worldSize.x, Fix64.Zero);

        FixVector2 doorPointA = new FixVector2(doorPosition.x, config.doorHalfSize.y);
        FixVector2 doorPointB = new FixVector2(doorPosition.x, -config.doorHalfSize.y);

        FixVector2 diretionA = doorPointA - actor.getPosition();
        diretionA.normalize();
        FixVector2 diretionB = doorPointB - actor.getPosition();
        diretionB.normalize();


        Fix64 dotA = FixVector2.dot(destDirection, diretionA);
        Fix64 dotB = FixVector2.dot(destDirection, diretionB);
        if (dotA > dotB)
        {
            dest = diretionA;
            positiveDoorSide = Fix64.One;
        }
        else
        {
            positiveDoorSide = -Fix64.One;
            dest = diretionB;
        }

        return positiveDoorSide;
    }

    public void beginCheckShootBall(FixVector2 direction)
    {
        FBActor actor = m_ball.owner;
        if (actor == null) return;
        if (actor.passBallPressed)
            return;

        FixVector2 destDirection = FixVector2.kZero;

        positiveDoorSide = getDoorSide(actor, direction, out destDirection);

        destDirection = getClampedDirection(actor.direction, destDirection, actor.configuration.shootBallAngleTorelance);

        actor.doShootBall(destDirection);
    }

    public void endCheckShootBall()
    {
        m_ball.owner.endCheckShootBall();
    }

    FixVector2 getClampedDirection(FixVector2 direction, FixVector2 expectDirection, Fix64 clampAngle)
    {
        FixVector2 destDirection = expectDirection;
        Fix64 dif = FixVector2.dot(expectDirection, direction);
        //角度过大
        if (dif < Fix64.Cos(clampAngle))
        {
            //旋转angleClamp
            Fix64 c = FixVector2.cross(expectDirection, direction);
            if (c < Fix64.Zero)//逆时针旋转为正
                destDirection = FixVector2.rotate(expectDirection, -clampAngle);
            else
                destDirection = FixVector2.rotate(expectDirection, clampAngle);

        }

        return destDirection;
    }


    //获取调整后的方向
    FixVector2 getAjustedDirection(FixVector2 currentDirection, FixVector2 destDirection, Fix64 angleTolerance)
    {
        Fix64 cosAngle = Fix64.Cos(angleTolerance);
        //不需要旋转，足够了
        if (FixVector2.dot(currentDirection, destDirection) > cosAngle)
            return currentDirection;

        Fix64 crossV = FixVector2.cross(destDirection, currentDirection);
        if (crossV != Fix64.Zero)
        {
            crossV = crossV / Fix64.FastAbs(crossV);
            destDirection = FixVector2.rotate(destDirection, angleTolerance * crossV);
        }

        return destDirection;
    }

    //某个角色开始传球
    //短传0，长传1
    public void beginPassBall(FBActor actor, FixVector2 passBallDirection, int index)
    {
        //Debuger.Log("beginPassBall...........");

        Fix64 maxR = m_ball.owner.configuration.passBallMaxR[index];
        Fix64 minR = m_ball.owner.configuration.passBallMinR[index];
        Fix64 angle = m_ball.owner.configuration.passBallFov[index];
        Fix64 bestR = m_ball.owner.configuration.passBallBestR[index];
        //没有键入方向,使用角色朝向
        if (passBallDirection == FixVector2.kZero)
            passBallDirection = actor.direction;

        FBActor target = findTarget(m_ball.owner, passBallDirection, index, (int)m_ball.owner.team, m_ball.owner);

        if (target != null)
        {
            passBallDirection = target.getPosition() - m_ball.owner.getPosition();
            passBallDirection = passBallDirection.normalized;
        }

        FixVector2 actorFaceDirection = getAjustedDirection(actor.direction, passBallDirection, actor.configuration.passBallAngleTorelance[index]);

        actor.doPassBall(index, actorFaceDirection);

        //保存传球对象
        passBallType = index;
        passBallTarget = target;
        passBallDir = passBallDirection;
    }

    /// <summary>
    /// AI在知道目标点的时候传球
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="passBallDirection"></param>
    /// <param name="index"></param>
    public bool passBallToTarget(FBActor target, int index)
    {
        if (target == null)
        {
            Debuger.LogError(" target is null");
            return false;
        }
        if (index < 0)
        {
            Debuger.LogError(" index < 0" + index);
            return false;
        }
        var owner = m_ball.owner;
        if (owner == null)
        {
            Debuger.LogError(" owner is null");
            return false;
        }

        Fix64 maxR = owner.configuration.passBallMaxR[index];
        Fix64 minR = owner.configuration.passBallMinR[index];
        Fix64 angle = owner.configuration.passBallFov[index];

        var passBallDirection = (target.getPosition() - owner.getPosition()).normalized;

        FixVector2 actorFaceDirection = getAjustedDirection(owner.direction, passBallDirection, owner.configuration.passBallAngleTorelance[index]);

        if (owner.doPassBall(index, actorFaceDirection))
        {
            //保存传球对象
            passBallType = index;
            passBallTarget = target;
            passBallDir = passBallDirection;
            return true;
        }
        return false;
    }


    bool checkSlowGetPassingBall(FBActor src, FBActor target, int idx)
    {
        FixVector2 passBallDirection = (target.getPosition() - src.getPosition()).normalized;
        Fix64 dotD = FixVector2.dot(passBallDirection, target.direction);

        if (target.particle.velocity.squareLength <= target.configuration.getPassingBallStandOrMovingSpeed[idx].square
            || dotD >= Fix64.Cos(target.configuration.getPassingBallStandOrMovingAngle[idx]))
        {
            //检测Target当前的状态是否可以慢速接球


            return true;
        }
        return false;
    }

    bool checkQuickGetPassingBall(FBActor src, FBActor target, int idx)
    {
        Fix64 speed = target.particle.velocity.squareLength;
        if (speed < target.configuration.getPassingBallStandOrMovingSpeed[idx].square)
        {
            return false;
        }

        FixVector2 passBallDirection = (target.getPosition() - src.getPosition()).normalized;
        Fix64 dotD = FixVector2.dot(passBallDirection, target.direction);

        if (dotD > Fix64.Cos(target.configuration.getPassingBallStandOrMovingAngle[idx]))
            return false;

        //检测Target当前的状态是否可以快速接球

        return true;
    }

    void passBallOut()
    {
        var owner = m_ball.owner;
        bool fly = passBallType == 1;
        BallDetachType bt = passBallType == 1 ? BallDetachType.LongPass : BallDetachType.ShortPass;
        FixVector3 ballStartPosition = owner.getBallPosition(bt);
        FixVector2 frontPosition = new FixVector2(ballStartPosition.x, ballStartPosition.z);

        FBActor target = passBallTarget;
        if (target == null)
        {
            //Debuger.Log("PassingBall no target");
            Fix64 passSpeed = owner.configuration.passingBallSpeedWhenNoTarget[passBallType];
            Fix64 verticleSpeed = owner.configuration.passingBallVerticleSpeedWhenNoTarget[passBallType];
            m_ball.freeByKick(ballStartPosition, (FixVector2)passBallDir * passSpeed, verticleSpeed, true);
        }
        else
        {
            //长传
            if (fly)
            {
                if (checkSlowGetPassingBall(owner, target, 1))
                {
                    FixVector3 targetBallP = target.getBallPosition(BallAttachType.Long_StandGetPassingBall);
                    var targetBallPosition = new FixVector2(targetBallP.x, targetBallP.z);

                    //Debuger.Log("Long PassingBall slow get begin");

                    FixVector2 distance = frontPosition - targetBallPosition;

                    Fix64 s = distance.length;
                    Fix64 t = calculateSlowShortPassBallTime(s);

                    //m_ball.freeByLongPass(targetBallPosition, target.configuration.ccb_ballHeight, t);
                    m_ball.freeByLongPass(ballStartPosition, targetBallPosition, targetBallP.y, t);
                    target.doGetPassingBallWhenStand(t, distance / s);
                }
                else if (checkQuickGetPassingBall(owner, target, 1))
                {
                    FixVector3 targetBallP = target.getBallPosition(BallAttachType.Long_RuningGetPasssingBall);
                    var targetBallPosition = new FixVector2(targetBallP.x, targetBallP.z);

                    Debuger.Log("Long PassingBall quick get begin");

                    Fix64 t = ((target.getPosition() - frontPosition).length + ConstTable.GetLongPasingBall_K1[1]) * ConstTable.GetLongPasingBall_K2[1];

                    targetBallPosition = targetBallPosition + target.particle.velocity * t;

                    m_ball.freeByLongPass(ballStartPosition, targetBallPosition, targetBallP.y, t);

                    target.doGetPassingBallWhenMoving(t);
                }
                else
                {
                    //Debuger.Log("Long PassingBall Failed");
                }
            }
            else
            {
                if (checkSlowGetPassingBall(owner, target, 0))
                {
                    FixVector3 targetBallP = target.getBallPosition(BallAttachType.Long_StandGetPassingBall);
                    var targetBallPosition = new FixVector2(targetBallP.x, targetBallP.z);

                    //Debuger.Log("short PassingBall slow get begin");

                    FixVector2 distance = frontPosition - targetBallPosition;

                    Fix64 s = distance.length;
                    Fix64 t = calculateSlowShortPassBallTime(s);
                    target.doGetPassingBallWhenStand(t, distance / s);

                    Fix64 t1 = t * ConstTable.GetShortPasingBall_K4[0];
                    Fix64 t2 = t * (Fix64.One - ConstTable.GetShortPasingBall_K4[0]);
                    m_ball.freeByNormalPass(ballStartPosition, targetBallPosition, t1, t2);
                }
                else if (checkQuickGetPassingBall(owner, target, 0))
                {
                    FixVector3 targetBallP = target.getBallPosition(BallAttachType.Long_StandGetPassingBall);
                    var targetBallPosition = new FixVector2(targetBallP.x, targetBallP.z);

                    //Debuger.Log("short PassingBall quitk get begin");

                    Fix64 t = ((target.getPosition() - frontPosition).length + ConstTable.GetShortPasingBall_K2[1]) * ConstTable.GetShortPasingBall_K3[1];

                    targetBallPosition = targetBallPosition + target.particle.velocity * t;

                    Fix64 t1 = t * ConstTable.GetShortPasingBall_K4[1];
                    Fix64 t2 = t * (Fix64.One - ConstTable.GetShortPasingBall_K4[1]);

                    target.doGetPassingBallWhenMoving(t1 + t2);
                    m_ball.freeByNormalPass(ballStartPosition, targetBallPosition, t1, t2);
                }
                else
                {
                    //Debuger.Log("short PassingBall Failed");
                }
            }

        }
        m_ball.increaseEnergy(fly ? owner.configuration.longPassEnergy : owner.configuration.shortPassEnergy);
        onPassBallOut(owner, m_ball.get3DVelocity());
    }

    // 处理球员铲球
    void _processActorSliding(FBActor actor)
    {
        if (m_ball.owner == actor && !actor.isDoorKeeper())
        {
            return;
        }
        foreach (var p in m_actors)
        {
            if (actor.checkSlidingTarget(p))
            {
                if (m_ball.owner == p)
                {
                    // 铲到持球队员
                    //int rnd = randomValue;
                    //if (rnd % 100 > 50)
                    //{
                    //    p.beSlid_dropBall();
                    //    m_ball.transfer(actor);
                    //}
                    //else if (rnd % 100 > 50)
                    if (true)
                    {
                        _processAttack(actor, m_ball.owner);
                    }
                    else
                    {
                        p.beSlid_keepBall();
                    }
                }
                else
                {

                    // 铲到无球队员
                    _processAttack(actor, p);
                }
            }
        }
    }

    //角色攻击
    void _processAttack(FBActor attacker, FBActor actor)
    {
        //todo bdt 个性化处理部分
        BallDetachType_Attacked bdt = BallDetachType_Attacked.Normal;

        if (!actor.isCtrlBall())
            bdt = BallDetachType_Attacked.None;

        actor.beAttacked(attacker, bdt);
    }

    // 处理获取球
    void _processCatchBallWhenMoving()
    {
        FBActor best = null;
        Fix64 bestWeight = Fix64.MinValue;
        foreach (var actor in m_actors)
        {
            Fix64 weight;
            if (actor.checkBallCatchingWhenMoving(m_ball, out weight))
            {
                if (weight > bestWeight)
                {
                    best = actor;
                    bestWeight = weight;
                }
            }
        }

        if (best != null)
        {

            ball.transfer(best);

            //best.attachPosition = ball.particle.position;
            //best.ballAttached = true;
            //Debuger.Log("=============================================Ball Attached CurrentFrameNumber :" + world.frameCount);
        }
    }

    public FBActor findTarget(FBActor src, FixVector2 direction, int index, int teamMask, FBActor exclude)
    {
        Fix64 maxRadius = src.configuration.passBallMaxR[index];
        Fix64 minRadius = src.configuration.passBallMinR[index];
        Fix64 theta = src.configuration.passBallFov[index];
        Fix64 bestRadius = m_ball.owner.configuration.passBallBestR[index];

        if (theta <= Fix64.Zero)
        {
            return null;
        }

        var cosTheta = Fix64.Cos(theta);

        List<FBActor> preferTargets = new List<FBActor>();
        var actorCount = m_actors.Count;
        for (int i = 0; i < actorCount; i++)
        {
            var actor = m_actors[i];
            if (actor == exclude || ((int)actor.team & teamMask) == 0)
            {
                continue;
            }
            var d = actor.particle.position - src.getPosition();
            var len = d.length;
            if (len < minRadius || len > maxRadius)
            {
                continue;
            }
            var cos = Fix64.Zero;
            if (len == Fix64.Zero)
            {
                cos = Fix64.One;
            }
            else
            {
                cos = FixVector2.dot(d.normalized, direction);
            }
            if (cos < cosTheta)
            {
                continue;
            }

            preferTargets.Add(actor);
        }

        FixVector2 preferPoint = src.getPosition() + direction * bestRadius;
        preferTargets.Sort(new FBActorTargetFindCompare(preferPoint));


        FBActor destTarget = null;
        for (int i = 0; i < preferTargets.Count; ++i)
        {
            if (checkBlocked(src, preferTargets[i]))
                continue;
            destTarget = preferTargets[i];
            break;
        }

#if UNITY_EDITOR
        LogicEvent.fire("onMainCharacterPassingBall", src, direction, index, destTarget, preferTargets);
#endif

        return destTarget;
    }

    //检测源和目标之间是否被阻挡
    bool checkBlocked(FBActor src, FBActor target)
    {
        FixVector2 a = (target.getPosition() - src.getPosition()).normalized;
        for (int i = 0; i < m_actors.Count; i++)
        {
            var actor = m_actors[i];
            if (actor == src || actor == target)
                continue;

            FixVector2 c = actor.getPosition() - src.getPosition();
            Fix64 dotData = FixVector2.dot(c, a);
            if (dotData <= Fix64.Zero)
                continue;
            Fix64 d = c.squareLength - Fix64.FastAbs(dotData * dotData);
            if (d < ball.configuration.radius * ball.configuration.radius)
                return true;
        }

        return false;
    }
    FBActor getDoorKeeper(FBTeam team)
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            var actor = m_actors[i];
            if (actor.isDoorKeeper() && actor.team == team)
                return actor;
        }
        return null;
    }

    public void debugFrameLogic()
    {
        var actorCount = m_actors.Count;
        for (int i = 0; i < actorCount; i++)
        {
            var actor = m_actors[i];
            actor.debugFrameLogic();
        }

        m_ball.debugFrameLogic();
    }
    public void debugLogicStart()
    {
        Debuger.LogLogic(string.Format("MainExtent:{0:X},{1:X}", m_mainExtent.x.RawValue, m_mainExtent.y.RawValue));
        Debuger.LogLogic(string.Format("DoorExtent:{0:X},{1:X} DoorHeight:{2:X}", m_doorExtent.x.RawValue, m_doorExtent.y.RawValue, m_doorHeight.RawValue));
    }
    public bool isBallFree()
    {
        return !ball.hasOwner;
    }

    public Fix64 calculateSlowShortPassBallTime(Fix64 distance)
    {
        return (distance + ConstTable.GetShortPasingBall_K2[0]) * ConstTable.GetShortPasingBall_K3[0];
    }

    public Fix64 calculateSlowLongPassBallTime(Fix64 distance)
    {
        return (distance + ConstTable.GetLongPasingBall_K1[0]) * ConstTable.GetLongPasingBall_K2[0];
    }

    public void setEnableBT(bool enable)
    {
        if (enableBT == enable) return;

        enableBT = enable;
        if (!enable)
        {
            if (redCoach != null)
                redCoach.reset();
            if (blueCoach != null)
                blueCoach.reset();
            if (redGKCoach != null)
                redGKCoach.reset();
            if (blueGKCoach != null)
                blueGKCoach.reset();
        }
    }
}