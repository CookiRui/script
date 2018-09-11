using FixMath.NET;
using Cratos;

using RAL;

class GameEnterState : GameStateBase
{
    enum SubState { Wait, ShowEnemy, WaitForSync }
    SubState subState;
    public GameEnterState(FBGame game) : base(game) { }

    public override void enter()
    {
        game.onFBGameEnter();
        game.changeCampState(game.goalTeam);
        game.resetPosition();
        game.playersAITakeOver(true);
        game.fbWorld.setEnableBT(true);
        game.onEnableRecordInput(false);

        if (WithoutEnterShow_4Test_EditorOnly.instance != null)
        {
            LogicEvent.fire2Lua("onEnterShowFinished", (int)game.mainActorTeam);
            LogicEvent.fire("onWaitForSync");
            subState = SubState.WaitForSync;
        }
        else
        {
            subState = SubState.Wait;
        }
    }

    public override void execute(Fix64 deltaTime)
    {
        base.execute(deltaTime);
        switch (subState)
        {
            case SubState.Wait:
                if (timer < game.fbWorld.config.showEnemyMoment) return;
                var enemys = game.fbWorld.getEnemys(game.mainActorTeam, false);
                if (enemys != null && enemys.Count > 0)
                {
                    var randomIdx = (int)(enemys.Count * (float)game.fbWorld.randomUnit);
                    if (randomIdx >= enemys.Count)
                    {
                        randomIdx = enemys.Count - 1;
                    }
                    var randomEnemy = enemys[randomIdx];
                    LogicEvent.fire2Rendering("onShowEnemy", randomEnemy.id);
                }
                subState = SubState.ShowEnemy;
                return;
            case SubState.ShowEnemy:
                if (timer < game.fbWorld.config.enterShowTime) return;
                LogicEvent.fire2Lua("onEnterShowFinished", (int)game.mainActorTeam);
                LogicEvent.fire("onWaitForSync");
                subState = SubState.WaitForSync;
                return;
            case SubState.WaitForSync:
                return;
        }
    }
}
