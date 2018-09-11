using FixMath.NET;
using BW31.SP2D;
using Cratos;

public partial class FBWorld
{

    void endSample()
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            m_actors[i].endSample();
        }

        if (ball != null)
        {
            ball.endSample();
        }
    }

    void beginSample()
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            m_actors[i].beginSample();
        }

        if (ball != null)
        {
            ball.beginSample();
        }
    }

    public void onOwnerAttached(FBActor owner)
    {
        //UnityEngine.Debug.LogError("onOwnerAttached fire onChangeCampState " + owner.team);
        LogicEvent.fire("onChangeCampState", owner.team);
        fbGame.generateRenderAction<RAL.BallAttachAction>(owner.id, owner.isDoorKeeper());
    }

    public void onOwnerDetached(FBActor owner)
    {
        fbGame.generateRenderAction<RAL.BallDetachAction>(owner.id);
    }

    public void onWorldCreated()
    {
        fbGame.generateRenderActionToTargetList<RAL.CreateWorldAction>(FBGame.RenderActionListType.kLogicBefore, m_mainExtent.toVector2(), m_doorExtent.toVector2(), (float)m_doorHeight);
    }

    public void onBallCreated(uint id, FixVector3 position, Fix64 radius)
    {
        fbGame.generateRenderActionToTargetList<RAL.CreateBallAction>(FBGame.RenderActionListType.kLogicBefore, id, new FixVector2 { x = position.x, y = position.z }.toVector2(), "football_ball", (float)radius);
    }

    public void onActorCreated(FBActor actor)
    {
        fbGame.generateRenderActionToTargetList<RAL.CreateActorAction>(FBGame.RenderActionListType.kLogicBefore, 
            actor.id,
            actor.roleId,
            actor.team,
            actor.name,
            actor.getPosition().toVector2(),
            actor.isDoorKeeper(),
            actor.configuration.element,
            (float)actor.configuration.bodyHeight,
            actor.configuration.normalSpeed.ToFloatArray());
    }

    public void onActorStandCatchingBall(FBActor actor, bool ccw)
    {
        fbGame.generateRenderAction<RAL.ActorStandCatchingBallAction>(actor.id, ccw);
    }

    public void onActorStandCatchingBallBegin(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.ActorStandCatchingBallBeginAction>(actor.id);
    }

    public void onActorAirCatchingBall(FBActor actor, int idx)
    {
        fbGame.generateRenderAction<RAL.ActorAirCatchingBallAction>(actor.id, (byte)idx);
    }

    public void onActorAirCatchingBallBegin(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.ActorAirCatchingBallBeginAction>(actor.id);
    }

    public void onActorTigerCatchingBallBegin(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.ActorTigerCatchingBallBeginAction>(actor.id);
    }

    public void onTurnBack(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.TurnAction>(actor.id, actor.direction.toVector2());
    }

    public void onActorMovingStateChanged(FBActor actor)
    {
        switch (actor.movingState)
        {
            case FBActor.MovingState.kIdle:
                fbGame.generateRenderAction<RAL.IdleAction>(actor.id);
                break;
            case FBActor.MovingState.kMoving:
                fbGame.generateRenderAction<RAL.RunAction>(actor.id);
                break;

            case FBActor.MovingState.kAction:
                fbGame.generateRenderAction<RAL.OtherAction>(actor.id);
                break;
            case FBActor.MovingState.kMovingToIdle:
                fbGame.generateRenderAction<RAL.MovingToIdleAction>(actor.id);
                break;
        }
    }


    public void onShootBallReady(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.ShootBallReadyAction>(actor.id, actor.shootingType);
    }

    public void onPassBallBegin(FBActor actor, bool shortPassBall, bool rightFoot)
    {
        fbGame.generateRenderAction<RAL.PassBeginAction>(actor.id, shortPassBall, rightFoot);
    }

    public void onShootBallBegin(FBActor actor, bool rightFoot)
    {
        fbGame.generateRenderAction<RAL.ShootBeginAction>(actor.id, rightFoot);
    }

    public void onPassBallOut(FBActor actor, FixVector3 velocity)
    {
        fbGame.generateRenderAction<RAL.BallPassAction>(actor.id, velocity.toVector3());
    }

    public void onShootBallOut(FBActor actor, FixVector3 velocity, Fix64 angle, FixVector3 target)
    {
        //jlx2017.05.26-log:因为行为树也要使用这个消息，所以使用fire，而不使用fire2Rendering
        LogicEvent.fire("onShootBallOut", actor.id);

        fbGame.generateRenderAction<RAL.BallShootAction>(actor.id, actor.shootingType, velocity.toVector3(), (float)angle, target.toVector3());
    }

    public void onSlidingBegin(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.SlideAction>(actor.id);
    }

    public void onBeSlidingKeepingBall(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.DribbleAction>(actor.id);
    }

    //持球被铲倒开始
    public void onBeSlidDropBallBegin(FBActor actor, bool stand)
    {
        fbGame.generateRenderAction<RAL.FallAction>(actor.id);
    }

    //无球被铲
    public void onBeSliding(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.FallAction>(actor.id);
    }

    public void onBeSlidDropBallWaiting(FBActor actor)
    {
        //开始爬起来
    }

    public void onBeSlidDropBallEnd(FBActor actor)
    {

    }

    /// <summary>
    /// 向队友要球
    /// </summary>
    /// <param name="actor"></param>
    public void onAskBall(FBActor actor)
    {
    }

    /// <summary>
    /// 嘲讽对手
    /// </summary>
    /// <param name="actor"></param>
    public void onRidicule(FBActor actor)
    {
    }

    /// <summary>
    /// 瞎BB
    /// </summary>
    /// <param name="actor"></param>
    public void onShowOff(FBActor actor)
    {
    }

    public void onDoorKeeperCatchingBallReady(FBActor actor, bool canCatchBall)
    {
        fbGame.generateRenderAction<RAL.DoorKeeperCatchingBallBeginAction>(actor.id, canCatchBall);
    }

    public void onSlowGetPassingBallReady(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.SlowGetPassingBallAction>(actor.id);
    }

    public void onDoorKeeperCatchingBall(FBActor actor, int zoneIndex, bool rightSide)
    {
        fbGame.generateRenderAction<RAL.DoorKeeperCatchingBallAction>(actor.id,(byte)zoneIndex, rightSide);
    }

    public void onDoorKeeperBeginToGetup(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.DoorKeeperBeginToGetupAction>(actor.id);
    }

    public void onActorAction(FBActor actor, string name, int value)
    {
        fbGame.generateRenderAction<RAL.AnimatorIntAction>(actor.id, name,value);
    }

    public void onActorAction(FBActor actor, string name, float value)
    {
        fbGame.generateRenderAction<RAL.AnimatorFloatAction>(actor.id, name, value);
    }

    public void onActorAction(FBActor actor, string name, bool value)
    {
        fbGame.generateRenderAction<RAL.AnimatorBoolAction>(actor.id, name, value);
    }

    public void onActorAction(FBActor actor, string name)
    {
        fbGame.generateRenderAction<RAL.AnimatorTriggerAction>(actor.id, name);
    }

    public void onActorWarmUp(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.WarmUpAction>(actor.id);
    }

    public void onActorTaunt(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.TauntAction>(actor.id);
    }

    public void onActorCheerStand(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.CheerStandAction>(actor.id);
    }

    public void onActorCheerUnique(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.CheerUniqueAction>(actor.id);
    }

    public void onActorDismay(FBActor actor)
    {
        fbGame.generateRenderAction<RAL.DismayAction>(actor.id);
    }

    public void onBallCollidedWall(FixVector3 point, FixVector3 normal, FixVector3 velocity, FiveElements kickerElement)
    {
        fbGame.generateRenderAction<RAL.BallCollidedWallAction>(point.toVector3(),normal.toVector3(),velocity.toVector3(),kickerElement);
    }

    public void onBallLanded(FixVector2 point, bool pass, int times, FixVector3 velocity, Fix64 preHeightVelocity)
    {
        fbGame.generateRenderAction<RAL.BallLandedAction>(point.toVector2(), pass, times, velocity.toVector3(), (float)preHeightVelocity);
    }

    public void onBallCollidedNet(FixVector3 point, FixVector3 preVelocity, FixVector3 curVelocity, FiveElements kickerElement)
    {
        fbGame.generateRenderAction<RAL.BallCollidedNetAction>(point.toVector3(), preVelocity.toVector3(), curVelocity.toVector3(), kickerElement);
    }

    public void onBallEnergyLevelChanged(byte oldLevel, byte newLevel)
    {
        fbGame.generateRenderAction<RAL.BallEnergyLevelChangedAction>(oldLevel, newLevel);
    }

    public void onBeginHit(uint attacker, uint victim)
    {
        fbGame.generateRenderAction<RAL.BeginHitAction>(attacker, victim);
    }

    public void onEndHit()
    {
        fbGame.generateRenderAction<RAL.EndHitAction>();
    }

    public void onHitCompleted()
    {
        fbGame.generateRenderAction<RAL.HitCompletedAction>();
    }

}