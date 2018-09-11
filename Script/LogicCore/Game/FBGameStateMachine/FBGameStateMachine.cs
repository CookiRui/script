using FixMath.NET;
using System.Collections.Generic;

public enum GameState
{
    Enter,
    Ready,
    Gaming,
    Goal,
    Replay,
    Over,
}

public interface IGameInfo
{
    ushort blueScore { get; }
    ushort redScore { get; }
    FBTeam goalTeam { get; }
    uint goaler { get; }
    GameState curStateType { get; }
    CampType getCampType(FBTeam team);
    TeamState getTeamState(FBTeam team);
}

public partial class FBGame : IGameInfo
{
    public ushort blueScore { get; private set; }
    public ushort redScore { get; private set; }
    public Fix64 gameBeginTime { get; set; }
    public Fix64 matchTimer { get; set; }
    public ulong tableId { get; private set; }
    public FBTeam goalTeam { get; private set; }
    public Location goalDoor { get; private set; }
    public Fix64 goalTime { get; set; }
    public uint goaler { get; private set; }
    public GameState curStateType { get; private set; }
    public GameState preStateType { get; private set; }

    Dictionary<GameState, GameStateBase> states = new Dictionary<GameState, GameStateBase>();
    GameStateBase curState;
    CampType blueCamp;
    CampType redCamp;
    Dictionary<ushort, uint> timeWithFrames= new Dictionary<ushort, uint>();

    GameStateBase getState(GameState state)
    {
        GameStateBase gameState;
        if (states.TryGetValue(state, out gameState))
        {
            return gameState;
        }

        switch (state)
        {
            case GameState.Enter: gameState = new GameEnterState(this); break;
            case GameState.Ready: gameState = new GameReadyState(this); break;
            case GameState.Gaming: gameState = new GamingState(this); break;
            case GameState.Goal: gameState = new GoalState(this); break;
            case GameState.Replay: gameState = new ReplayState(this); break;
            case GameState.Over: gameState = new GameOverState(this); break;
        }
        states.Add(state, gameState);
        return gameState;
    }

    void goal(Location localtion)
    {
        if (curStateType != GameState.Gaming) return;
        goalDoor = localtion;
        switch (localtion)
        {
            case Location.kLeftDoor:
                redScore++;
                goalTeam = FBTeam.kRed;
                break;
            case Location.kRightDoor:
                blueScore++;
                goalTeam = FBTeam.kBlue;
                break;
            default: return;
        }
        if (fbWorld.ball.owner == null)
        {
            goaler = fbWorld.ball.kicker.id;
        }
        else
        {
            goaler = fbWorld.ball.owner.id;
        }
        changeState(GameState.Goal);
    }

    public void changeState(GameState state)
    {
#if ARTIEST_MODE
        if( state == GameState.Goal )
            state = GameState.Gaming;
#endif

        if (curState != null && curStateType == state) return;

        if (curState != null)
        {
            preStateType = curStateType;
            curState.exit();
        }
        curState = getState(state);
        curStateType = state;
        curState.enter();
    }

    public void fbGameCreateCompleted()
    {
        changeState(GameState.Enter);
    }

    public void changeCampState(FBTeam team)
    {
        //UnityEngine.Debug.LogError("changeCampState " + team);
        //jlx 2017.07.12-log:进球以后变为敌方开球
        switch (team)
        {
            case FBTeam.kBlue:
                blueCamp = CampType.Defence;
                redCamp = CampType.Attack;
                break;
            case FBTeam.kNone:
            case FBTeam.kRed:
                blueCamp = CampType.Attack;
                redCamp = CampType.Defence;
                break;
        }
    }

    public CampType getCampType(FBTeam team)
    {
        switch (team)
        {
            case FBTeam.kBlue: return blueCamp;
            case FBTeam.kRed: return redCamp;
        }
        return CampType.Attack;
    }

    public TeamState getTeamState(FBTeam team)
    {
        switch (curStateType)
        {
            case GameState.Enter:
                return TeamState.BeforeGame;
            case GameState.Gaming:
                var camp = getCampType(team);
                switch (camp)
                {
                    case CampType.Attack: return TeamState.Attack;
                    case CampType.Defence: return TeamState.Defence;
                }
                return TeamState.None;
            case GameState.Goal:
                return goalTeam == team ? TeamState.Goal : TeamState.EnemyGoal;
            case GameState.Over:
                if (blueScore == redScore) return TeamState.AfterGameDraw;
                switch (team)
                {
                    case FBTeam.kBlue:
                        return blueScore < redScore ? TeamState.AfterGameLose : TeamState.AfterGameWin;
                    case FBTeam.kRed:
                        return redScore < blueScore ? TeamState.AfterGameLose : TeamState.AfterGameWin;
                }
                break;
            case GameState.Ready: break;
            case GameState.Replay: break;
        }
        //Debuger.LogError("getTeamState  team: " + team + "  curStateType: " + curStateType + "   TeamState.None");
        return TeamState.None;
    }

    public void recordTimeWithFrame(ushort time, uint frame)
    {
        if (timeWithFrames.ContainsKey(time))
        {
            timeWithFrames[time] = frame;
        }
        else
        {
            timeWithFrames.Add(time, frame);
        }
        onUpdateMatchTime(time, frame);
    }

    public uint getFrameByTime(ushort time)
    {
        uint frame;
        if (timeWithFrames.TryGetValue(time, out frame))
        {
            return frame;
        }
        return 0;
    }
}
