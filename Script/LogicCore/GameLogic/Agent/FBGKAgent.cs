using FixMath.NET;
using BW31.SP2D;
using behaviac;

public class FBGKAgent : FBPlayerAgent
{
    protected override string btPath { get { return world.btConfig.gkBTPath; } }

    Fix64 passBallTimer;
    public FBGKAgent(FBActor actor, behaviac.Workspace workspace) : base(actor, workspace) { }

    #region private methods

    #endregion

    #region protected methods

    protected override Agent createBehaviour(behaviac.Workspace workspace)
    {
        var behaviour = new BTGoalKeeper();
        behaviour.Init(workspace);
        behaviour.agent = behaviour.gkAgent = this;
        return behaviour;
    }

    #endregion

    #region public methods

    public bool isInPenaltyArea(FixVector2 position)
    {
        var penaltyareaSize = world.config.penaltyAreaSize;
        var doorPosition = actor.getSelfDoorPosition();
        if (-penaltyareaSize.y < position.y
            && position.y < penaltyareaSize.y)
        {
            if (doorPosition.x > Fix64.Zero)
            {
                return doorPosition.x - penaltyareaSize.x * (Fix64)2 < position.x
                     && position.x < doorPosition.x;
            }
            else
            {
                return doorPosition.x < position.x
                     && position.x < doorPosition.x + penaltyareaSize.x * (Fix64)2;
            }
        }
        return false;
    }

    #endregion

    #region 行为树调用的接口

    public EBTStatus pounceBall()
    {
        Debuger.Log("pounceBall");
        if (state != PlayerState.PounceBall)
        {
            //Debuger.Log("pounceBall doTigerCatchingBall");

            actor.doTigerCatchingBall();
            state = PlayerState.PounceBall;
        }
        if (actor.checkTigerCatchingBall())
        {
            return EBTStatus.BT_RUNNING;
        }
        //Debuger.Log("pounceBall BT_SUCCESS");
        return EBTStatus.BT_SUCCESS;
    }

    public FixVector2 getRandomPosition()
    {
        var penaltyareaSize = world.config.penaltyAreaSize;
        var realSize = penaltyareaSize * (Fix64)0.8;
        var randomX = world.randomUnit;
        var randomY = world.randomUnit;
        //Debuger.Log("randomX:" + randomX + " randomY:" + randomX);
        var randomPosition = new FixVector2
        {
            x = -realSize.x + (Fix64)2 * realSize.x * randomX,
            y = -realSize.y + (Fix64)2 * realSize.y * randomY,
        };
        var doorPosition = actor.getSelfDoorPosition();
        var penaltyareaCenter = FixVector2.kZero;
        if (doorPosition.x > Fix64.Zero)
        {
            penaltyareaCenter = doorPosition - new FixVector2 { x = penaltyareaSize.x * (Fix64)1.2 };
        }
        else
        {
            penaltyareaCenter = doorPosition + new FixVector2 { x = penaltyareaSize.x * (Fix64)1.2 };
        }
        return penaltyareaCenter + randomPosition;
    }

    public uint getNearTeamMate()
    {
        var id = world.getNearTeamMate(actor.id);
        //UnityEngine.Debug.LogError("getNearTeamMate:" + id);
        return id;
    }

    public FixVector2 getGKDefensePosition(Fix64 rate, Fix64 baseDistance)
    {
        var ballPosition = ball.getPosition();
        var doorPosition = actor.getSelfDoorPosition();
        var dir = ballPosition - doorPosition;
        var position = doorPosition + dir * rate + dir.normalized * baseDistance;
        //UnityEngine.Debug.LogError("getGKDefensePosition position " + position);
        return position;
    }

    public override bool isAroundSafe(Fix64 range)
    {
        if (hasBall())
        {
            Debug.Log("hasBall()");
        }
        if (range <= Fix64.Zero)
        {
            return true;
        }
        var enemys = world.getEnemys(team);
        if (enemys == null || enemys.Count == 0) return true;

        var range2 = range * range;
        for (int i = 0; i < enemys.Count; ++i)
        {
            var enemy = enemys[i];
            if (!isInPenaltyArea(enemy.getPosition())) continue;
            var distance = actor.getPosition().squareDistance(enemy.getPosition());
            if (distance < range2)
            {
                return false;
            }
        }
        return true;
    }

    public bool isArrivedPassBallWaitTime()
    {
        if (state != PlayerState.PassBall) return true;
        return timer >= world.btConfig.gkPassBallWaitTime;
    }

    #endregion
}