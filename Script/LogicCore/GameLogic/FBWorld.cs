using BW31.SP2D;
using FixMath.NET;
using System.Collections.Generic;
using behaviac;

public partial class FBWorld
{
    public World world { get { return m_world; } }
    public FBBall ball { get { return m_ball; } }

    public BTConfiguration btConfig { get; private set; }

    public void setup(Configuration config, BTConfiguration btConfig)
    {
        if (config == null)
        {
            Debuger.LogError("config is null");
            return;
        }
        this.config = config;
        this.btConfig = btConfig;
        setup(config.worldSize, config.doorHalfSize, config.doorHeight, config.doorHalfSlopeWidth);
    }
    public void setup(FixVector2 mainExtent, FixVector2 doorExtent, Fix64 doorHeight, Fix64 doorSlopeExtent)
    {
        m_mainExtent = mainExtent;
        m_doorExtent = doorExtent;
        m_doorHeight = doorHeight;
        m_arena.build(mainExtent, doorExtent, doorHeight, doorSlopeExtent);
        onWorldCreated();
    }


    public void addActor(FBActor actor)
    {
        if (actor.world != null)
        {
            actor.world.removeActor(actor);
        }
        m_actors.Add(actor);
        m_world.addParticle(actor.particle);
        (actor as IElement).setWorld(this);
    }

    public FBActor getActor(uint id)
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            if (m_actors[i].id == id)
                return m_actors[i];
        }
        return null;
    }
    public FBActor getActorByTeam(FBTeam team)
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            if (m_actors[i].team == team)
                return m_actors[i];
        }
        return null;
    }

    public List<FBActor> getEnemys(FBTeam team, bool includeGK = true)
    {
        var enemys = new List<FBActor>();
        for (int i = 0; i < m_actors.Count; ++i)
        {
            var actor = m_actors[i];
            if (actor.team == team) continue;
            if (actor.isDoorKeeper() && !includeGK) continue;

            enemys.Add(actor);
        }
        return enemys;
    }

    public List<FBActor> getTeamMates(FBTeam team, bool includeGK = true)
    {
        var enemys = new List<FBActor>();
        for (int i = 0; i < m_actors.Count; ++i)
        {
            var actor = m_actors[i];
            if (actor.team != team) continue;
            if (actor.isDoorKeeper() && !includeGK) continue;

            enemys.Add(actor);
        }
        return enemys;
    }

    public uint getNearEnemy(uint id)
    {
        if (id <= 0)
        {
            Debuger.LogError("id <= 0");
            return 0;
        }
        var actor = getActor(id);
        if (actor == null) return 0;

        var enemys = getEnemys(actor.team);
        if (enemys == null || enemys.Count == 0) return 0;
        var minDistance = Fix64.MaxValue;
        uint nearId = 0;
        for (int i = 0; i < enemys.Count; ++i)
        {
            var enemy = enemys[i];
            var distance = actor.getPosition().squareDistance(enemy.getPosition());
            if (distance < minDistance)
            {
                minDistance = distance;
                nearId = enemy.id;
            }
        }
        return nearId;
    }

    public uint getNearTeamMate(uint id)
    {
        if (id <= 0)
        {
            Debuger.LogError("id <= 0");
            return 0;
        }
        var actor = getActor(id);
        if (actor == null) return 0;

        var teamMates = getTeamMates(actor.team);
        if (teamMates == null || teamMates.Count == 0) return 0;
        var minDistance = Fix64.MaxValue;
        uint nearId = 0;
        for (int i = 0; i < teamMates.Count; ++i)
        {
            var teamMate = teamMates[i];
            if (teamMate.id == id) continue;
            var distance = actor.getPosition().squareDistance(teamMate.getPosition());
            if (distance < minDistance)
            {
                minDistance = distance;
                nearId = teamMate.id;
            }
        }
        return nearId;
    }

    public uint getNearPlayer(FBTeam team, PlayerType type)
    {
        var ballPosition = ball.getPosition();
        var minDistance = Fix64.MaxValue;
        var id = 0u;
        var teamMates = getTeamMates(team);
        for (int i = 0; i < teamMates.Count; i++)
        {
            var teamMate = teamMates[i];
            if ((type & (teamMate.AIing ? PlayerType.AI : PlayerType.Player)) == 0)
            {
                continue;
            }

            var distance = teamMate.getPosition().squareDistance(ballPosition);
            if (distance < minDistance)
            {
                id = teamMate.id;
                minDistance = distance;
            }
        }
        return id;
    }

    public FBActor getMainActor()
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            var actor = m_actors[i];
            if (actor.mainActor) return actor;
        }
        return null;
    }

    public uint getGKId(FBTeam team)
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            var action = m_actors[i];
            if (action.team == team && action.isDoorKeeper())
                return action.id;
        }
        return 0;
    }

    public uint getEnemyGKId(FBTeam team)
    {
        for (int i = 0; i < m_actors.Count; ++i)
        {
            var action = m_actors[i];
            if (action.team != team && action.isDoorKeeper())
                return action.id;
        }
        return 0;
    }

    public FBBall createBall(uint id, FixVector3 position, FBBall.Configuration config)
    {
        m_ball = new FBBall(config, this.config);
        (m_ball as IElement).setWorld(this);
        m_ball.id = id;
        m_ball.reset(position);
        this.onBallCreated(id, position, config.radius);
        return m_ball;
    }

    public void removeActor(FBActor actor)
    {
        if (m_actors.Remove(actor))
        {
            m_world.removeParticle(actor.particle);
            (actor as IElement).setWorld(null);
        }
    }

    public void addAgent(FBPlayerAgent agent, FBTeam team)
    {
        if (agent == null)
        {
            Debuger.LogError("agent is null");
            return;
        }

        FBCoachAgent coach = getCoach(team);
        if (coach != null)
            coach.addPlayer(agent);
    }

    public void removeAgent(FBPlayerAgent agent, FBTeam team)
    {
        if (agent == null)
        {
            Debuger.LogError("agent is null");
            return;
        }
        var coach = getCoach(team);
        if (coach != null)
        {
            coach.removePlayer(agent);
        }
    }

    public void addGK(FBGKAgent agent, FBTeam team)
    {
        if (agent == null)
        {
            Debuger.LogError("agent is null");
            return;
        }
        var gkGoach = getGKCoach(team);
        if (gkGoach != null)
        {
            gkGoach.gk = agent;
        }
    }

    public void removeGK(FBTeam team)
    {
        var gkGoach = getGKCoach(team);
        if (gkGoach != null)
        {
            gkGoach.gk = null;
        }
    }

    public void update(Fix64 deltaTime, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            m_world.updateOneFrame(deltaTime);
            _update(deltaTime);
        }
        //for (int i = 0; i < m_actors.Count; ++i)
        //{
        //    FixVector3 pos = m_actors[i].getPosition();
        //    Debuger.Log(string.Format(
        //        "frameID:{0} actor id:{1} positionX:{2:X} positionY:{3:X}",
        //        FrameSync.instance.currentLogicFrameNum, m_actors[i].id,
        //        pos.x.RawValue,
        //        pos.z.RawValue));
        //}

        lateUpdate(deltaTime);
    }

    public void updateOneFrame(Fix64 deltaTime)
    {
        _willUpdate(deltaTime);
        m_world.updateOneFrame(deltaTime);
        _update(deltaTime);
    }

    public void update(Fix64 deltaTime, List<RAL.RenderAction>[] physics) {

        for (int i = 0; i < physics.Length; ++i) {
            _willUpdate(deltaTime);
            fbGame.setCurrentRenderActionList(physics[i]);
            m_world.updateOneFrame(deltaTime);
            _update(deltaTime);
        }
        lateUpdate(deltaTime);
    }

    public FBGame fbGame { get; private set; }

    public FBWorld(FBGame game)
    {
        fbGame = game;
        m_world.onObstacleCollided = _onObstacleCollided;
        m_world.onParticleCollided = _onParticleCollided;
        m_world.addObstacle(m_arena);
    }

    //创建教练
    public void createCoach(IGameInfo gameInfo, FBTeam team, Workspace workspace,string btName)
    {
        var coach = new FBCoachAgent(gameInfo, this, team, workspace);
        coach.setBehaviour(btName);
        switch (team)
        {
            case FBTeam.kBlue:
                blueCoach = coach;
                break;
            case FBTeam.kRed:
                redCoach = coach;
                break;
        }
    }

    //创建门将教练
    public void createGKCoach(IGameInfo gameInfo,FBTeam team,Workspace workspace, string btName)
    {
        var gkCoach = new FBGKCoachAgent(gameInfo, this, team, workspace);
        gkCoach.setBehaviour(btName);
        switch (team)
        {
            case FBTeam.kBlue:
                blueGKCoach = gkCoach;
                break;
            case FBTeam.kRed:
                redGKCoach = gkCoach;
                break;
        }
    }

    FBCoachAgent getCoach(FBTeam team)
    {
        switch (team)
        {
            case FBTeam.kBlue: return blueCoach;
            case FBTeam.kRed: return redCoach;
        }
        return null;
    }

    FBGKCoachAgent getGKCoach(FBTeam team)
    {
        switch (team)
        {
            case FBTeam.kBlue: return blueGKCoach;
            case FBTeam.kRed: return redGKCoach;
        }
        return null;
    }

    public int randomSeed
    {
        get { return m_randomSeed; }
        set
        {
            m_randomSeed = (int)((uint)value % 2147483647);
            if (m_randomSeed == 0)
            {
                m_randomSeed = 1;
            }
        }
    }

    public int randomValue
    {
        get
        {
            m_randomSeed = (int)((long)m_randomSeed * 48271 % 2147483647);
            return m_randomSeed;
        }
    }

    public int randomMin
    {
        get { return 1; }
    }

    public int randomMax
    {
        get { return 2147483646; }
    }

    public Fix64 randomUnit
    {
        get
        {
            return (Fix64)(randomValue - randomMin) / (Fix64)(randomMax - randomMin);
        }
    }

    public interface IElement
    {
        void setWorld(FBWorld world);
    }

    ArenaObstacle m_arena = new ArenaObstacle();
    World m_world = new World();
    List<FBActor> m_actors = new List<FBActor>();
    FBBall m_ball;

    FixVector2 m_mainExtent, m_doorExtent;
    Fix64 m_doorHeight;

    int m_randomSeed = 1;

    FBCoachAgent redCoach;
    FBCoachAgent blueCoach;
    FBGKCoachAgent redGKCoach;
    FBGKCoachAgent blueGKCoach;

    public Configuration config;
    public void clear()
    {
        Debuger.Log("World clear");
        m_actors.Clear();

        m_mainExtent = FixVector2.kZero;
        m_doorExtent = FixVector2.kZero;
        m_doorHeight = Fix64.Zero;
        config = null;
        redCoach = null;
        blueCoach = null;
        redGKCoach = null;
        blueGKCoach = null;
        m_arena = null;
        m_world = null;
        m_ball = null;
    }

    public FixVector2 getDoorPosition(FBTeam team, bool self)
    {
        switch (team)
        {
            case FBTeam.kBlue: return new FixVector2 { x = m_mainExtent.x * (self ? -Fix64.One : Fix64.One) };
            case FBTeam.kRed: return new FixVector2 { x = m_mainExtent.x * (self ? Fix64.One : -Fix64.One) };
        }
        return FixVector2.kZero;
    }

    public bool isInRange(FixVector2 position)
    {
        return -m_mainExtent.x < position.x
                && position.x < m_mainExtent.x
                && -m_mainExtent.y < position.y
                && position.y < m_mainExtent.y;
    }

}