using FixMath.NET;
using System;
using Cratos;

class GamingState : GameStateBase
{
    enum SubState { Normal,Pause}
    SubState subState;

    public GamingState(FBGame game) : base(game)
    {
        LogicEvent.add("onPauseTimer", this, "onPauseTimer");
        LogicEvent.add("onContinueTimer", this, "onContinueTimer");
    }

    public override void enter()
    {
        game.gameBeginTime = game.matchTimer;
        game.onGameBegin( );
        recordTime();
        game.fbWorld.setEnableBT(true);
        game.onEnableRecordInput(true);
        subState = SubState.Normal;
    }

    public override void execute(Fix64 deltaTime)
    {
        switch (subState)
        {
            case SubState.Normal:
                base.execute(deltaTime);
                if (timer < Fix64.One) return;

                game.matchTimer += timer;
                if (game.matchTimer < game.fbWorld.config.matchTime)
                {
                    recordTime();
                    timer -= Fix64.One;
                }
                else
                {
                    game.changeState(GameState.Over);
                }
                break;
            case SubState.Pause:

                break;
        }
    }

    void onPauseTimer()
    {
        subState = SubState.Pause;
    }

    void onContinueTimer()
    {
        subState = SubState.Normal;
    }

    void recordTime()
    {
        game.recordTimeWithFrame((ushort)game.matchTimer, game.frameSync.currentLogicFrameNum);
    }
}