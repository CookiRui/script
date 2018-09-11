using FixMath.NET;
using Cratos;

public interface ILogicUpdater
{
    void LogicStart();

    void LogicOver();

    void update(float time);

    void preUpdate();
};

public interface IFrameSyncEventHandler
{
    void handleFrameInputEvent(ServerFrameInputEvent evt);
}
