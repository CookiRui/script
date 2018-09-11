using FixMath.NET;
using System;

class ReplayState : GameStateBase
{
    Fix64 replayTime;
    enum SubState { Replay, Wait }
    SubState subState;
    public ReplayState(FBGame game) : base(game) { }
    public override void enter()
    {
        game.fbWorld.ball.vanish();

        var beginTime = (game.goalTime - game.gameBeginTime) <= game.fbWorld.config.replayTimeBeforeGoal ? game.gameBeginTime : (game.goalTime - game.fbWorld.config.replayTimeBeforeGoal);
        var endTime = game.goalTime + game.fbWorld.config.replayTimeAfterGoal;
        replayTime = endTime - beginTime;
        var beginFrame = game.getFrameByTime((ushort)beginTime);
        var endFrame = game.getFrameByTime((ushort)endTime);
        game.onBeginReplay(beginFrame,
                            endFrame,
                            (ushort)game.goalTime,
                            (ushort)replayTime,
                            game.getPlayerUid(game.goaler),
                            game.goalDoor,
                            game.blueScore,
                            game.redScore,
                            game.fbWorld.getEnemyGKId(game.goalTeam),
                            (float)game.fbWorld.randomUnit,
                            (float)game.fbWorld.randomUnit,
                            (float)game.fbWorld.randomUnit,
                            game.goalTeam == game.mainActorTeam);

        game.playersAITakeOver(false);

        game.fbWorld.setEnableBT(false);
        subState = SubState.Replay;
        game.onEnableRecordInput(false);


        Debuger.Log("Replay totalTime: " + (float)replayTime);
    }

    public override void execute(Fix64 deltaTime)
    {
        base.execute(deltaTime);
        switch (subState)
        {
            case SubState.Replay:
                if (timer < replayTime) return;
                game.onEndReplay();
                subState = SubState.Wait;
                resetTimer();
                return;
            case SubState.Wait:
                if (timer < (Fix64)0.3) return;
                game.changeState(GameState.Ready);
                return;
        }

    }
}
