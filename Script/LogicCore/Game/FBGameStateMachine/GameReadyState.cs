using FixMath.NET;
using System;

class GameReadyState : GameStateBase
{
    enum SubState { Wait, Countdown }
    SubState subState;
    Fix64 waitTime;
    Fix64 countdownTime;

    public GameReadyState(FBGame game) : base(game) { }
    public override void enter()
    {
        game.changeCampState(game.goalTeam);
        FBActor mainActor = game.fbWorld.getMainActor();
        var team = mainActor == null ? FBTeam.kBlue : mainActor.team;
        var camp = game.getCampType(team);
        game.onGameReady(camp);
        game.playersAITakeOver(false);
        game.resetPlayerState();

        game.resetPosition();

        game.changeCampState(game.goalTeam);
        game.fbWorld.setEnableBT(false);
        game.onEnableRecordInput(false);
        subState = SubState.Wait;
        if (game.preStateType == GameState.Replay)
        {
            waitTime = game.fbWorld.config.replayWaitTime;
            countdownTime = Fix64.Zero;
        }
        else
        {
            waitTime = Fix64.One;
            countdownTime = game.fbWorld.config.readyTime;
            if (WithoutEnterShow_4Test_EditorOnly.instance != null)
            {
                countdownTime = Fix64.Zero;
            }
            LogicEvent.fire2Lua("onFirstRound");
        }
    }

    public override void execute(Fix64 deltaTime)
    {
        base.execute(deltaTime);
        switch (subState)
        {
            case SubState.Wait:
                if (timer < waitTime) return;
                subState = SubState.Countdown;
                timer = Fix64.One;
                break;
            case SubState.Countdown:
                if (timer < Fix64.One) return;

                if (countdownTime > -Fix64.One)
                {
                    game.onUpdateCountdown((byte)countdownTime);
                }
                else
                {
                    if (game.preStateType == GameState.Enter)
                    {
                        game.onGameInit();
                    }
                    game.changeState(GameState.Gaming);
                }
                countdownTime -= timer;
                resetTimer();
                break;
        }
    }
}
