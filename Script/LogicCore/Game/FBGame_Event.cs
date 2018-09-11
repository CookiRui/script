using FixMath.NET;

public partial class FBGame
{
    public void onFBGameDestroyed()
    {
        LogicEvent.fire("onFBGameDestroyed");
    }

    public void onFBGameCreated()
    {
        LogicEvent.fire("onFBGameCreated");
    }

    public void onFBGameEnter()
    {
        LogicEvent.fire("onFBGameEnter");
    }

    void onMainActorCreated(uint playerID, FBTeam team)
    {
        generateRenderActionToTargetList<RAL.MainActorCreatedAction>(FBGame.RenderActionListType.kLogicBefore, playerID, team);
    }

    public void onGoal(uint id, FBTeam team, Location door)
    {
        generateRenderAction<RAL.GoalAction>(id, team, door);
    }

    public void onGameBegin( )
    {
        generateRenderAction<RAL.GameBeginAction>();
    }

    public void onUpdateMatchTime(ushort time,uint frame)
    {
        generateRenderAction<RAL.UpdateMatchTimeAction>(time, frame);
    }

    public void onGameOver(uint tableId, ushort redScore, ushort blueScore)
    {
        LogicEvent.fire2Lua("onGameOver", tableId, redScore, blueScore);
        generateRenderAction<RAL.GameOverAction>();
    }

    public void onUpdateCountdown(byte time)
    {
        generateRenderAction<RAL.UpdateCountdownAction>(time);
    }

    public void onGameReady(CampType camp)
    {
        LogicEvent.fire2Rendering("onGameReady", camp);
        if (m_currentRenderActionList != null)
            generateRenderAction<RAL.GameReadyAction>();
        else
        {
            generateRenderActionToTargetList<RAL.GameReadyAction>(RenderActionListType.kLogicBefore);
        }
    }

    public void onUpdateScore(ushort redScore, ushort blueScore)
    {
        generateRenderAction<RAL.UpdateScoreAction>(redScore, blueScore);
    }

    public void onBeginReplay(uint beginFrame,
                        uint endFrame,
                        ushort goalTime,
                        ushort replayTime,
                        ulong goaler,
                        Location door,
                        ushort blueScore,
                        ushort redScore,
                        uint gkId,
                        float positionRandomValue,
                        float shootRandomValue,
                        float goalRandomValue,
                        bool selfGoal)
    {
        generateRenderAction<RAL.ReplayBeginAction>(beginFrame,
                                                endFrame,
                                                goalTime,
                                                replayTime,
                                                goaler,
                                                door,
                                                blueScore,
                                                redScore,
                                                gkId,
                                                positionRandomValue,
                                                shootRandomValue,
                                                goalRandomValue,
                                                goalTeam);

        LogicEvent.fire2Rendering("onBeginReplay", door, gkId, positionRandomValue, shootRandomValue, goalRandomValue);
    }

    public void onEndReplay()
    {
        generateRenderAction<RAL.ReplayEndAction>();
    }

    public void onShowReplayLogo()
    {
        generateRenderAction<RAL.ShowReplayLogoAction>();
    }

    public void onEnableRecordInput(bool enable)
    {
        LogicEvent.fire2Rendering("onEnableRecordInput", enable);
    }

    public void onGameInit()
    {
        generateRenderAction<RAL.GameInitAction>();
    }
}