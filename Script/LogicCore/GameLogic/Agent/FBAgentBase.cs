using behaviac;
using FixMath.NET;

public abstract class FBAgentBase
{
    protected abstract int updateInterval { get; }
    protected int updateFrameCounter;

    protected virtual FBWorld world { get; set; }
    protected virtual FBTeam team { get; set; }
    protected FBBall ball { get { return world.ball; } }

    protected Agent behaviour;
    protected abstract string btPath { get; }

    public virtual void updateBehaviour()
    {
        behaviour.btexec();
    }

    public void setBehaviour(string name)
    {
        if (name == null)
        {
            Debuger.Log("name is null");
            return;
        }
        //UnityEngine.Debug.LogError(btPath + name);
        behaviour.btsetcurrent(btPath + name);
    }

    public abstract void update(Fix64 deltaTime);
    public abstract void clear();

    public virtual void reset()
    {
        behaviour.btresetcurrrent();
    }
}
