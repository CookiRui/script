using FixMath.NET;

abstract class GameStateBase
{
    protected FBGame game { get; private set; }
    protected Fix64 timer;
    public GameStateBase(FBGame game) { this.game = game; }
    public virtual void enter() { }
    public virtual void execute(Fix64 deltaTime) { timer += deltaTime; }
    public virtual void exit() { resetTimer(); }
    protected void resetTimer() { timer = Fix64.Zero; }
}
