using FixMath.NET;
using System.Collections.Generic;
using BW31.SP2D;

public class FBGKCoachAgent : FBAgentBase
{
    protected override string btPath { get { return world.btConfig.gkBTPath; } }
    protected override int updateInterval { get { return world.btConfig.gkCoachUpdateInterval; } }

    public FBGKAgent gk { private get; set; }
    IGameInfo gameInfo;

    public FBGKCoachAgent(IGameInfo gameInfo,FBWorld world, FBTeam team, behaviac.Workspace workspace)
    {
        var behaviour = new BTGKCoach();
        behaviour.Init(workspace);
        behaviour.agent = this;
        base.behaviour = behaviour;
        this.gameInfo = gameInfo;
        this.world = world;
        this.team = team;
    }

    #region private methods
    #endregion

    #region public methods

    public override void updateBehaviour()
    {
        if (updateInterval > 1 && updateFrameCounter++ % updateInterval != 0) return;
        base.updateBehaviour();
        if (gk == null) return;
        gk.updateBehaviour();
    }


    public override void update(Fix64 deltaTime)
    {
        if (gk == null) return;
        gk.update(deltaTime);
    }

    public override void clear()
    {
        if (gk == null) return;

        gk.clear();
    }

    public override void reset()
    {
        base.reset();
        setPlayerBehaviour(CoachCmd.Idle);
    }

    #endregion

    #region 行为树调用的接口

    public void setPlayerBehaviour(CoachCmd cmd)
    {
        if (gk == null) return;
        //if (IdTest.instance != null)
        //{
        //    if (IdTest.instance.team == team)
        //    {
        //        UnityEngine.Debug.LogError("setPlayerBehaviour " + cmd);
        //    }
        //}
        gk.changeBehaviour(cmd);
    }

    public uint getNearEnemy()
    {
        if (gk == null) return 0;

        return world.getNearEnemy(gk.actor.id);
    }

    public uint getPlayerHasBall()
    {
        return ball.hasOwner ? ball.owner.id : 0;
    }

    public bool hasBall()
    {
        if (gk == null) return false;

        return gk.hasBall();
    }

    public bool isAroundSafe(Fix64 range)
    {
        if (gk == null) return false;

        return gk.isAroundSafe(range);
    }

    public bool isBallFree()
    {

        return world.isBallFree();
    }

    public bool isBallInPenaltyArea()
    {
        if (gk == null) return false;

        return gk.isInPenaltyArea(ball.getPosition());
    }

    public bool isEnemyHasBall()
    {
        return ball.hasOwner && ball.owner.team != team;
    }

    public void setPlayerAttack(uint id)
    {
        if (gk == null) return;

        gk.setPlayerId(id);
    }

    public Fix64 getBallSpeedSquare()
    {
        // Debuger.Log(ball.particleVelocity.squareLength.ToString());
        return ball.particleVelocity.squareLength;
    }

    public CoachCmd getPlayerBehaviour()
    {
        if (gk == null) return CoachCmd.Idle;

        //Debuger.Log("getPlayerBehaviour " + gk.curCmd + UnityEngine.Time.realtimeSinceStartup);
        return gk.curCmd;
    }

    public TeamState getTeamState()
    {
        var teamState = gameInfo.getTeamState(team);

        //if (IdTest.instance != null)
        //{
        //    if (IdTest.instance.team == team)
        //    {
        //        UnityEngine.Debug.LogError("getTeamState " + teamState);
        //    }
        //}
        return teamState;
    }


    public bool isArrivedPassBallWaitTime()
    {
        if (gk == null) return false;
        return gk.isArrivedPassBallWaitTime();
    }

    #endregion
}
