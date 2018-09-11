using FixMath.NET;
using Cratos;

class GoalState : GameStateBase
{
    enum SubState { Wait, Show, ShowLogo }
    SubState subState;
    public GoalState(FBGame game) : base(game) { }
    public override void enter()
    {
        game.onUpdateScore(game.redScore,game.blueScore);
        game.onGoal(game.goaler,game.goalTeam, game.goalDoor);
        game.goalTime = game.matchTimer;
        game.onEnableRecordInput(false);
        game.playersAITakeOver(true);
        game.fbWorld.setEnableBT(false);
        subState = SubState.Wait;
    }

    public override void execute(Fix64 deltaTime)
    {
        base.execute(deltaTime);
        switch (subState)
        {
            case SubState.Wait:
                if (timer < game.fbWorld.config.replayTimeAfterGoal) return;
                game.fbWorld.setEnableBT(true);
                LogicEvent.fire2Rendering("onBeginGoalShow");
                resetTimer();
                subState = SubState.Show;
                return;
            case SubState.Show:
                if (timer < game.fbWorld.config.goalShowTime) return;
                game.onShowReplayLogo();
                resetTimer();
                subState = SubState.ShowLogo;
                return;
            case SubState.ShowLogo:
                if (timer <  (Fix64)0.3f) return;
                game.changeState(GameState.Replay);
                return;
        }
    }
}
