using behaviac;
using BW31.SP2D;
using FixMath.NET;
using Cratos;

public class FBPlayer
{
    public bool ai { get; set; }
    public FBActor actor = null;
    FBPlayerAgent agent = null;
    FBWorld world = null;
    Workspace btWorkspace;

    public FBPlayer(uint id, uint baseID, FBTeam team, string name, bool mainActor, bool ai, FBWorld world, Workspace btWorkspace)
    {
        this.world = world;
        this.btWorkspace = btWorkspace;

        FBActor.Configuration config = loadConfig(baseID);

        //actor = new FBActor(config,baseID);
        //测试代码-fbactor_configuration
        actor = new FBActor(config, baseID, mainActor);

        actor.id = id;
        actor.team = team;
        actor.name = name;
        world.addActor(actor);
        this.ai = ai;
    }


    public void onCreated()
    {
        world.onActorCreated(actor);
    }

    public void destroy()
    {
        if (agent != null)
        {
            actor.world.removeAgent(agent, actor.team);
        }
        actor.world.removeActor(actor);
        actor = null;
    }

    public void aiTakeOver(bool value)
    {
        if (actor.AIing == value) return;

        actor.AIing = value;
        if (value)
        {
            if (actor.isDoorKeeper())
            {
                if (agent == null)
                {
                    agent = new FBGKAgent(actor, btWorkspace);
                }
                world.addGK(agent as FBGKAgent, actor.team);
            }
            else
            {
                if (agent == null)
                {
                    agent = new FBPlayerAgent(actor, btWorkspace);
                }
                world.addAgent(agent, actor.team);
            }
        }
        else
        {
            if (agent != null)
            {
                if (actor.isDoorKeeper())
                {
                    world.removeGK(actor.team);
                }
                else
                {
                    world.removeAgent(agent, actor.team);
                }
                agent.stop();
            }
        }
    }

    FBActor.Configuration loadConfig(uint baseID)
    {
        //测试代码-fbactor_configuration
        return null;
        return FBActor.Configuration.getConfiguration(baseID);
    }

    public void onServerFrameInputEvent(ServerFrameInputEvent inputEvent)
    {
        FBActor logicActor = actor;

        //aiTakeOver(inputEvent.isAITakeOver);

        //处于接球状态或者AI状态下不做任何处理
        if (inputEvent.isAITakeOver)
            return;

        FixVector2 moveDirection = FixVector2.kZero;

        if (inputEvent.angle != short.MinValue)
        {
            Fix64 radian = (Fix64)inputEvent.angle * Fix64.Pi / (Fix64)180;
            moveDirection.x = Fix64.Cos(radian);
            moveDirection.y = Fix64.Sin(radian);
            logicActor.move(moveDirection);
        }
        else
        {
            logicActor.stop();
        }


        if (inputEvent.keys[0] == 1)
        {
            if (world.ball.hasOwner)
            {
                if (logicActor.isCtrlBall())
                {
                    //自己控球，进行射门
                    world.beginCheckShootBall(moveDirection);
                }
                else if (logicActor.isMateCtrlBall())
                {
                    //队友控球，进行铲球？
                    world.doSliding(logicActor);
                }
                else
                {
                    //对手控球，进行铲球
                    //logicActor.doSliding();
                    world.doSliding(logicActor);
                }
            }
            else
            {
                //没有人控球，进行铲球
                if (ConstTable.DebugStateAction == 0)
                {
                    //logicActor.doSliding();
                    world.doSliding(logicActor);
                }
                else if (ConstTable.DebugStateAction == 1)
                    logicActor.doTigerCatchingBall();
                else if (ConstTable.DebugStateAction == 3)
                    world.doSkill(logicActor, 0);

            }
        }
        else if (inputEvent.keys[0] == 2)
        {
            if (world.ball.hasOwner)
            {
                if (logicActor.isCtrlBall())
                {
                    world.endCheckShootBall();
                }
            }
            else
            {
            }
        }
        else
        {
        }

        if (inputEvent.keys[1] == 1)
        {
            if (world.ball.hasOwner)
            {
                if (logicActor.isCtrlBall())
                {
                    //自己控球，进行传球
                    logicActor.beginCheckPassBall();
                }
            }
        }
        else if (inputEvent.keys[1] == 2)//j//传球弹起
        {
            if (world.ball.hasOwner)
            {
                if (logicActor.isCtrlBall())
                {
                    logicActor.endCheckPassBall(moveDirection);
                }
                else if (logicActor.isMateCtrlBall())
                {
                    //队友控球，进行要球
                    world.onAskBall(logicActor);
                }
                else
                {
                    //对手控球，进行嘲讽
                    world.onRidicule(logicActor);
                }
            }
            else
            {
                if (world.ball.kicker == null)
                {
                    //没有人控球 
                    world.onShowOff(logicActor);
                }
                else if (logicActor.isMateCtrlBall())
                {
                    //队友控球，进行要球
                    world.onAskBall(logicActor);
                }
                else
                {
                    //对手控球，进行嘲讽
                    world.onRidicule(logicActor);
                }
            }
        }
        else
        {
        }
    }

}