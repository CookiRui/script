class GameOverState : GameStateBase
{
    public GameOverState(FBGame game) : base(game) { }
    public override void enter()
    {
        game.onGameOver((uint)game.tableId,game.redScore,game.blueScore);
        game.playersAITakeOver(true);
        game.fbWorld.setEnableBT(true);
        game.onEnableRecordInput(false);
    }
}
