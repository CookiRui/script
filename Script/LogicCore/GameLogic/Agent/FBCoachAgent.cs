using FixMath.NET;
using System.Collections.Generic;
using BW31.SP2D;
using System;
using Cratos;
using System.Linq;

public class FBCoachAgent : FBAgentBase
{
    protected override string btPath { get { return world.btConfig.coachBTPath; } }
    protected override int updateInterval { get { return world.btConfig.coachUpdateInterval; } }

    List<FBPlayerAgent> players;
    IGameInfo gameInfo;

    /// <summary>
    /// 敌人的球门在左半场
    /// </summary>
    bool enemyDoorOnLeft;

    /// <summary>
    /// 球门中心
    /// </summary>
    FixVector2 enemyDoorCenterPosition;

    /// <summary>
    /// 左门柱
    /// </summary>
    FixVector2 enemyLeftGoalpostPosition;

    /// <summary>
    /// 右门柱
    /// </summary>
    FixVector2 enemyRightGoalpostPosition;

    int beginCol;
    int endCol;
    int frontCol;
    int backCol;
    FixVector2 enemyDoorCenterGrid;
    enum PlayerLocationType { FrontTop, FrontBottom, BackTop, BackBottom }

    List<FBActor> _teamMates;
    List<FBActor> teamMates
    {
        get
        {
            if (_teamMates == null)
            {
                _teamMates = world.getTeamMates(team);
            }
            return _teamMates;
        }
    }

    List<FBActor> _teamMatesExcludeGK;
    List<FBActor> teamMatesExcludeGK
    {
        get
        {
            if (_teamMatesExcludeGK == null)
            {
                _teamMatesExcludeGK = world.getTeamMates(team, false);
            }
            return _teamMatesExcludeGK;
        }
    }

    List<FBActor> _enemys;
    List<FBActor> enemys
    {
        get
        {
            if (_enemys == null)
            {
                _enemys = world.getEnemys(team);
            }
            return _enemys;
        }
    }

    bool hasAiPlayer;

    public FBCoachAgent(IGameInfo gameInfo, FBWorld world, FBTeam team, behaviac.Workspace workspace)
    {
        var behaviour = new BTCoach();
        behaviour.Init(workspace);

        behaviour.agent = this;
        base.behaviour = behaviour;

        players = new List<FBPlayerAgent>();
        this.gameInfo = gameInfo;
        this.world = world;
        this.team = team;
        enemyDoorOnLeft = team == FBTeam.kRed;


        enemyDoorCenterPosition = new FixVector2 { x = world.config.worldSize.x * (Fix64)(enemyDoorOnLeft ? -1 : 1) };
        enemyLeftGoalpostPosition = enemyDoorCenterPosition + new FixVector2 { y = -world.config.doorHalfSize.y };
        enemyRightGoalpostPosition = enemyDoorCenterPosition + new FixVector2 { y = world.config.doorHalfSize.y };

        beginCol = enemyDoorOnLeft ? 0 : world.btConfig.col;
        endCol = beginCol + world.btConfig.col;
        frontCol = enemyDoorOnLeft ? 0 : world.btConfig.col2 - 1;
        backCol = enemyDoorOnLeft ? world.btConfig.col2 - 1 : 0;
        enemyDoorCenterGrid = new FixVector2
        {
            x = (enemyDoorOnLeft ? Fix64.Zero : (Fix64)world.btConfig.col2) - (Fix64)0.5f,
            y = (Fix64)world.btConfig.row - (Fix64)0.5f,
        };
    }

    ~FBCoachAgent()
    {
        LogicEvent.remove(this);
    }

    #region private methods

    /// <summary>
    /// 获取球员
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    FBPlayerAgent getPlayer(uint id)
    {
        if (players.Count == 0) return null;
        for (int i = 0; i < players.Count; i++)
        {
            var agent = players[i];
            if (agent.actor.id == id) return agent;
        }
        return null;
    }

    /// <summary>
    /// 获取持球球员
    /// </summary>
    /// <returns></returns>
    FBPlayerAgent getKeepingBallPlayer()
    {
        if (!ball.hasOwner) return null;
        return getPlayer(ball.owner.id);
    }

    /// <summary>
    /// 计算给定点的危险分数
    /// S = n * K - a * Sum(L)
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    int calculateDangerScore(FixVector2 point)
    {
        int n = 0;
        Fix64 sumDistance = Fix64.Zero;
        for (int i = 0; i < enemys.Count; i++)
        {
            var distance = calculateDistance(point, enemys[i].getPosition());
            if (distance < world.btConfig.aroundDangerRange)
            {
                sumDistance += distance;
                n++;
            }
        }
        return n * world.btConfig.aroundDangerBaseScore - (int)(world.btConfig.aroundDangerDistanceRate * sumDistance);
    }

    /// <summary>
    /// 计算给定点的射门分数
    /// S = K4 - K1 - K2 - K3
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    int calculateShootScore(FixVector2 position)
    {
        var k1 = calculateShootAreaScore(position);
        var k2 = calculateInterferenceAreaScore(position);
        var k3 = calculateDangerScore(position);
        // K4 = M - L + θ
        var k4 = world.btConfig.shootBaseScore - calculateShootDistanceScore(position) + calculateShootAngleScore(position);
        return k4 - k1 - k2 - k3;
    }

    /// <summary>
    /// 计算射门区域分数
    /// S = a * m + b * n 
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    int calculateShootAreaScore(FixVector2 point)
    {
        var a = point;
        var b = enemyLeftGoalpostPosition;
        var c = enemyRightGoalpostPosition;
        return (int)(world.btConfig.shootAreaEnemyEffect * (Fix64)calculatePlayerCountInTriangle(a, b, c, true) + world.btConfig.shootAreaTeamEffect * (Fix64)calculatePlayerCountInTriangle(a, b, c, false));
    }

    /// <summary>
    /// 计算射门干涉区域分数
    /// S = a * m
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    int calculateInterferenceAreaScore(FixVector2 point)
    {
        var a = point;
        var b1 = enemyLeftGoalpostPosition;
        var t1 = calculateDistance(point, b1) / world.btConfig.ballShootSpeed;
        var c1 = b1 + new FixVector2 { y = -t1 * world.btConfig.playerSpeed };

        var b2 = enemyRightGoalpostPosition;
        var t2 = calculateDistance(point, b1) / world.btConfig.ballShootSpeed;
        var c2 = b2 + new FixVector2 { y = t2 * world.btConfig.playerSpeed };
        return (int)(world.btConfig.shootAreaAroundEnemyEffect * (Fix64)(calculatePlayerCountInTriangle(a, b1, c1, true) + calculatePlayerCountInTriangle(a, b2, c2, true)));
    }

    /// <summary>
    /// 计算射门距离分数
    /// S = a * l
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    int calculateShootDistanceScore(FixVector2 point)
    {
        return (int)(world.btConfig.shootPositionDistanceRate * calculateDistance(point, enemyDoorCenterPosition));
    }

    /// <summary>
    /// 计算射门角度分数
    /// S = a * ( 1 - cosθ)
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    int calculateShootAngleScore(FixVector2 point)
    {
        var dir = (enemyDoorCenterPosition - point).normalized;
        var cos = Fix64.Abs(FixVector2.dot(dir, FixVector2.kUp));
        return (int)(world.btConfig.shootPositionAngleRate * (Fix64.One - cos));
    }

    /// <summary>
    /// 计算距离（不要平方！）
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns></returns>
    Fix64 calculateDistance(FixVector2 position1, FixVector2 position2)
    {
        return position1.distance(position2);
    }

    /// <summary>
    /// 计算三角形内的球员数量
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="enemy"></param>
    /// <returns></returns>
    int calculatePlayerCountInTriangle(FixVector2 a, FixVector2 b, FixVector2 c, bool enemy)
    {
        var actors = enemy ? enemys : teamMates;
        if (actors == null) return 0;
        var count = 0;
        for (int i = 0; i < actors.Count; i++)
        {
            if (isPointInTriangle(a, b, c, actors[i].getPosition()))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 点是否在三角形内
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    bool isPointInTriangle(FixVector2 a, FixVector2 b, FixVector2 c, FixVector2 p)
    {
        Func<FixVector2, FixVector2, FixVector2, Fix64> sign = (p1, p2, p3) => { return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y); };
        var b1 = sign(p, a, b) < Fix64.Zero;
        var b2 = sign(p, b, c) < Fix64.Zero;
        var b3 = sign(p, c, a) < Fix64.Zero;
        return ((b1 == b2) && (b2 == b3));
    }

    /// <summary>
    /// 计算圆形内球员数量
    /// </summary>
    /// <param name="o"></param>
    /// <param name="r"></param>
    /// <param name="enemy"></param>
    /// <returns></returns>
    int calculatePlayerCountInCircle(FixVector2 o, Fix64 r, bool enemy)
    {
        var actors = enemy ? enemys : teamMates;
        if (actors == null) return 0;
        var count = 0;
        for (int i = 0; i < actors.Count; i++)
        {
            if (calculateDistance(o, actors[i].getPosition()) < r)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 计算接球点距离分数
    /// S = K - a * | L - L0|
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="playerGrid"></param>
    /// <returns></returns>
    int calculatePassBallDistanceScore(Grid mainGrid, Grid playerGrid)
    {
        return (int)((Fix64)world.btConfig.passBallBaseScore - world.btConfig.passBallDistanceRate * Fix64.Abs(world.btConfig.calculateGridDistance(mainGrid, playerGrid) - world.btConfig.passBallBestDistance));
    }

    /// <summary>
    /// 计算接球点危险分数
    /// S = k * m
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns></returns>
    int calculatePassBallDangerScore(FixVector2 position1, FixVector2 position2)
    {
        var direction = position2 - position1;
        var r = world.btConfig.playerSpeed * getPassBallTime(direction.length);
        var normal = FixVector2.rotate(direction.normalized, Fix64.PiOver2);
        var a = position2 + normal * r;
        var b = position2 + -normal * r;
        var count1 = calculatePlayerCountInTriangle(a, b, position1, true);
        var count2 = calculatePlayerCountInCircle(position2, r, true);
        return (int)(world.btConfig.passBallEnemyEffect * (Fix64)(count1 + count2));
    }

    /// <summary>
    /// 获取传球类型
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    int getPassBallType(Fix64 distance)
    {
        return distance < world.btConfig.passBallTypeDistance ? 0 : 1;
    }

    /// <summary>
    /// 获取传球时间
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    Fix64 getPassBallTime(Fix64 distance)
    {
        var type = getPassBallType(distance);
        switch (type)
        {
            case 0: return world.calculateSlowShortPassBallTime(distance);
            case 1: return world.calculateSlowLongPassBallTime(distance);
        }
        return Fix64.Zero;
    }

    /// <summary>
    /// 计算接球球员的射门分数
    /// S = M - a * L - b * S1
    /// </summary>
    /// <returns></returns>
    int calculateShootScore4BestCatchBallPlayer(Grid grid, Grid playerGrid, FixVector2 playerPosition)
    {
        var l = (int)(world.btConfig.shootPositionDistanceRate * world.btConfig.calculateGridDistance(grid, playerGrid));
        var s = (int)(world.btConfig.shootPlayerAroundDangerScoreRate * (Fix64)calculateDangerScore(playerPosition));
        return world.btConfig.shootBaseScore - l - s;
    }

    /// <summary>
    /// 计算接球球员的传球分数
    /// S = S1 - S2
    /// </summary>
    /// <returns></returns>
    int calculatePassScore4BestCatchBallPlayer(Grid grid1, FixVector2 position1, Grid grid2, FixVector2 position2)
    {
        return calculatePassBallDistanceScore(grid1, grid2) - calculatePassBallDangerScore(position1, position2);
    }

    /// <summary>
    /// 计算接球分数
    /// S = S1 - S2 + a * S3
    /// </summary>
    /// <param name="grid1"></param>
    /// <param name="grid2"></param>
    /// <returns></returns>
    int calculateCatchBallScore(Grid grid1, Grid grid2)
    {
        var position = world.btConfig.getPosition(grid2);
        var s1 = calculatePassBallDistanceScore(grid1, grid2);
        var s2 = calculatePassBallDangerScore(world.btConfig.getPosition(grid1), position);
        var s3 = (int)(world.btConfig.passBallScoreShootScoreEffect * (Fix64)calculateShootScore(position));
        return s1 - s2 + s3;
    }

    /// <summary>
    /// 是否离球门更近
    /// </summary>
    /// <param name="oldGrid"></param>
    /// <param name="newGrid"></param>
    /// <returns></returns>
    bool isCloseDoor(Grid oldGrid, Grid newGrid)
    {
        return isCloseDoor(oldGrid.row, oldGrid.col, newGrid.row, newGrid.col);
    }

    /// <summary>
    /// 是否离球门更近
    /// </summary>
    /// <param name="oldRow"></param>
    /// <param name="oldCol"></param>
    /// <param name="newRow"></param>
    /// <param name="newCol"></param>
    /// <returns></returns>
    bool isCloseDoor(int oldRow, int oldCol, int newRow, int newCol)
    {
        return Fix64.Abs((Fix64)newRow - enemyDoorCenterGrid.y) + Fix64.Abs((Fix64)newCol - enemyDoorCenterGrid.x)
                < Fix64.Abs((Fix64)oldRow - enemyDoorCenterGrid.y) + Fix64.Abs((Fix64)oldCol - enemyDoorCenterGrid.x);
    }

    /// <summary>
    /// 获取排序球员
    /// </summary>
    /// <returns></returns>
    List<uint> getSortActors()
    {
        var actors = teamMatesExcludeGK.OrderBy(a => Fix64.Abs(a.getPosition().x - enemyDoorCenterPosition.x)).ToList();
        var sortActors = new List<uint>();
        if (actors.Count > 1)
        {
            if (actors[0].getPosition().y > actors[1].getPosition().y)
            {
                sortActors.Add(actors[0].id);
                sortActors.Add(actors[1].id);
            }
            else
            {
                sortActors.Add(actors[1].id);
                sortActors.Add(actors[0].id);
            }
        }
        else if (actors.Count > 0)
        {
            sortActors.Add(actors[0].id);
        }

        if (actors.Count > 3)
        {
            if (actors[2].getPosition().y > actors[3].getPosition().y)
            {
                sortActors.Add(actors[2].id);
                sortActors.Add(actors[3].id);
            }
            else
            {
                sortActors.Add(actors[3].id);
                sortActors.Add(actors[2].id);
            }
        }
        else if (actors.Count > 2)
        {
            sortActors.Add(actors[2].id);
        }
        return sortActors;
    }

    /// <summary>
    /// 计算防御的距离和
    /// </summary>
    /// <param name="defenseIds"></param>
    /// <param name="attackIds"></param>
    /// <returns></returns>
    Fix64 calculateDefenseDistaneSum(uint[] defenseIds, uint[] attackIds)
    {
        if (defenseIds == null || defenseIds.Length == 0
            || attackIds == null || attackIds.Length == 0
            || defenseIds.Length != attackIds.Length) return Fix64.Zero;
        var length = defenseIds.Length;
        var sum = Fix64.Zero;
        for (int i = 0; i < length; i++)
        {
            var defenseActor = getTeamMeate(defenseIds[i]);
            var attackActor = getEnemy(attackIds[i]);
            if (defenseActor == null || attackActor == null) continue;
            var defenseActorGrid = world.btConfig.getGrid(defenseActor.getPosition(), enemyDoorOnLeft);
            var attackActorGrid = world.btConfig.getGrid(attackActor.getPosition(), !enemyDoorOnLeft);
            sum += world.btConfig.calculateGridDistance(defenseActorGrid, attackActorGrid);
        }
        return sum;
    }

    FBActor getTeamMeate(uint id)
    {
        return getActor(teamMates, id);
    }

    FBActor getEnemy(uint id)
    {
        return getActor(enemys, id);
    }

    FBActor getActor(List<FBActor> actors, uint id)
    {
        if (actors == null || actors.Count == 0) return null;
        if (id <= 0) return null;
        var count = actors.Count;
        for (int i = 0; i < count; i++)
        {
            var actor = actors[i];
            if (actor.id == id) return actor;
        }
        return null;
    }

    /// <summary>
    /// 判定排列是否计算过
    /// </summary>
    /// <param name="oldDefenseIds"></param>
    /// <param name="oldAttackIds"></param>
    /// <param name="newDefenseIds"></param>
    /// <param name="newAttackIds"></param>
    /// <returns></returns>
    bool isPermutationCalculated(uint[] oldDefenseIds, uint[] oldAttackIds, uint[] newDefenseIds, uint[] newAttackIds)
    {
        if (oldDefenseIds == null || oldAttackIds == null || newDefenseIds == null || newAttackIds == null) return false;
        var length = oldDefenseIds.Length;
        for (int i = 0; i < length - 1; i++)
        {
            var defenseId = oldDefenseIds[i];
            var attackId = oldAttackIds[i];
            var newIndex = getIndex(newDefenseIds, defenseId);
            var newAttackId = newAttackIds[newIndex];
            if (attackId != newAttackId)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 获取索引
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    int getIndex(uint[] ids, uint id)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] == id) return i;
        }
        return -1;
    }

    /// <summary>
    /// 计算最佳接球点
    /// </summary>
    /// <param name="playerMoveGrids"></param>
    /// <param name="mainGrid"></param>
    /// <returns></returns>
    Grid calculateBestMoveGrid(List<Grid> playerMoveGrids, Grid mainGrid)
    {
        if (playerMoveGrids == null || playerMoveGrids.Count == 0) return default(Grid);

        var maxScore = int.MinValue;
        var bestGrid = new Grid();
        for (int i = 0; i < playerMoveGrids.Count; i++)
        {
            var grid = playerMoveGrids[i];
            var score = calculateCatchBallScore(mainGrid, grid);
            if (score == maxScore)
            {
                if (isCloseDoor(bestGrid, grid))
                {
                    bestGrid = grid;
                }
            }
            else if (maxScore < score)
            {
                maxScore = score;
                bestGrid = grid;
            }
        }
        return bestGrid;
    }

    /// <summary>
    /// 计算目标点
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainId"></param>
    /// <param name="excludePlayerLocation"></param>
    /// <param name="playerIds"></param>
    /// <returns></returns>
    Dictionary<PlayerLocationType, BT.Vector3> calculateTargetPosition(Grid mainGrid, Grid ballGrid, uint mainId, PlayerLocationType mainLocation)
    {
        if (mainId <= 0)
        {
            Debuger.LogError("mainId <=0 " + mainId);
            return null;
        }

        var targetPositionDic = new Dictionary<PlayerLocationType, BT.Vector3>();
        var mainGrids = new List<Grid>() { mainGrid };
        for (int i = 0; i < 3; i++)
        {
            List<Grid> grids = null;
            var location = PlayerLocationType.BackBottom;
            switch (i)
            {
                case 0:
                    //上下 
                    switch (mainLocation)
                    {
                        case PlayerLocationType.FrontTop:
                        case PlayerLocationType.BackTop:
                            grids = calculateBottomGrids(mainGrid, mainGrids);
                            location = mainLocation + 1;
                            break;
                        case PlayerLocationType.FrontBottom:
                        case PlayerLocationType.BackBottom:
                            grids = calculateTopGrids(mainGrid, mainGrids);
                            location = mainLocation - 1;
                            break;
                    }
                    break;
                case 1:
                    //前后
                    switch (mainLocation)
                    {
                        case PlayerLocationType.FrontTop:
                        case PlayerLocationType.FrontBottom:
                            grids = calculateBackGrids(mainGrid, mainGrids);
                            location = mainLocation + 2;
                            break;
                        case PlayerLocationType.BackTop:
                        case PlayerLocationType.BackBottom:
                            grids = calculateFrontGrids(mainGrid, mainGrids);
                            location = mainLocation - 2;
                            break;
                    }
                    break;
                case 2:
                    //前后
                    if (mainGrids.Count > 1)
                    {
                        var tmpGrid = mainGrids[1];
                        Action<PlayerLocationType, int> switchLocation = (oldLocation, offset) =>
                        {
                            if (targetPositionDic.ContainsKey(oldLocation))
                            {
                                targetPositionDic.Add(oldLocation + offset, targetPositionDic[oldLocation]);
                                targetPositionDic.Remove(oldLocation);
                            }
                        };
                        switch (mainLocation)
                        {
                            case PlayerLocationType.FrontTop:
                                if (Math.Abs(tmpGrid.col - backCol) > world.btConfig.minColWithBorder)
                                {
                                    grids = calculateBackGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.BackBottom;
                                }
                                else
                                {
                                    grids = calculateFrontGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.FrontBottom;
                                    switchLocation(location, 2);
                                }
                                break;
                            case PlayerLocationType.FrontBottom:
                                if (Math.Abs(tmpGrid.col - backCol) > world.btConfig.minColWithBorder)
                                {
                                    grids = calculateBackGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.BackTop;
                                }
                                else
                                {
                                    grids = calculateFrontGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.FrontTop;
                                    switchLocation(location, 2);
                                }
                                break;
                            case PlayerLocationType.BackTop:
                                if (Math.Abs(tmpGrid.col - frontCol) > world.btConfig.minColWithBorder)
                                {
                                    grids = calculateFrontGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.FrontBottom;
                                }
                                else
                                {
                                    grids = calculateBackGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.BackBottom;
                                    switchLocation(location, -2);
                                }
                                break;
                            case PlayerLocationType.BackBottom:
                                if (Math.Abs(tmpGrid.col - frontCol) > world.btConfig.minColWithBorder)
                                {
                                    grids = calculateBackGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.FrontTop;
                                }
                                else
                                {
                                    grids = calculateFrontGrids(tmpGrid, mainGrids);
                                    location = PlayerLocationType.BackTop;
                                    switchLocation(location, -2);
                                }
                                break;
                        }
                    }
                    break;
            }
            if (grids == null || grids.Count == 0)
            {
                Debuger.LogError(string.Format("计算点为空 mainGrid:({0},{1})  mainId:{2}   i: {3}   mainRealLocation :{4}  location:{5}", mainGrid.row, mainGrid.col, mainId, i, mainLocation, location));
            }
            else
            {
                var grid = calculateBestMoveGrid(grids, ballGrid);
                mainGrids.Add(grid);
                if (targetPositionDic.ContainsKey(location))
                {
                    Debuger.LogError(string.Format("location 已经存在 mainGrid:({0},{1})  mainId:{2}   i: {3}    mainRealLocation :{4}  location: {5}", mainGrid.row, mainGrid.col, mainId, i, mainLocation, location));
                }
                else
                {
                    targetPositionDic.Add(location, BTCoach.convertToVector3(world.btConfig.getPosition(grid)));
                }
            }
        }
        return targetPositionDic;
    }

    /// <summary>
    /// 排序球员
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainId"></param>
    /// <returns></returns>
    Dictionary<uint, PlayerLocationType> sortPlayers(Grid mainGrid, uint mainId)
    {
        if (mainId <= 0)
        {
            Debuger.LogError("mainId <= 0 " + mainId);
            return null;
        }

        var actorIds = getSortActors();
        if (actorIds.Count < 4) return null;

        var mainIdx = actorIds.FindIndex(a => a == mainId);
        var mainIdealLocation = getIdealLocation(mainGrid, mainIdx);
        var mainRealLocation = getRealLocation(mainIdealLocation, mainGrid);

        var players = new Dictionary<uint, PlayerLocationType>();
        Action<int, PlayerLocationType> addPlayer = (idx, location) =>
        {
            if (0 <= idx && idx < actorIds.Count)
            {
                if (location < PlayerLocationType.FrontTop || PlayerLocationType.BackBottom < location)
                {
                    Debuger.LogError(string.Format("location越界 ：index:{0}   location:{1}", idx, location));
                    return;
                }
                var id = actorIds[idx];
                if (players.ContainsKey(id))
                {
                    players[id] = location;
                }
                else
                {
                    players.Add(id, location);
                }
            }
        };
        addPlayer(mainIdx, mainRealLocation);
        switch (mainRealLocation)
        {
            case PlayerLocationType.FrontTop:
                if (mainIdealLocation == mainRealLocation)
                {
                    addPlayer(mainIdx == 0 ? 1 : 0, PlayerLocationType.FrontBottom);
                    addPlayer(2, PlayerLocationType.BackTop);
                    addPlayer(3, PlayerLocationType.BackBottom);
                }
                else
                {
                    addPlayer(mainIdx == 2 ? 3 : 2, PlayerLocationType.BackBottom);
                    addPlayer(0, PlayerLocationType.BackTop);
                    addPlayer(1, PlayerLocationType.FrontBottom);
                }
                break;
            case PlayerLocationType.FrontBottom:
                if (mainIdealLocation == mainRealLocation)
                {
                    addPlayer(mainIdx == 0 ? 1 : 0, PlayerLocationType.FrontTop);
                    addPlayer(2, PlayerLocationType.BackTop);
                    addPlayer(3, PlayerLocationType.BackBottom);
                }
                else
                {
                    addPlayer(mainIdx == 2 ? 3 : 2, PlayerLocationType.BackTop);
                    addPlayer(0, PlayerLocationType.FrontTop);
                    addPlayer(1, PlayerLocationType.BackBottom);
                }
                break;
            case PlayerLocationType.BackTop:
                if (mainIdealLocation == mainRealLocation)
                {
                    addPlayer(mainIdx == 2 ? 3 : 2, PlayerLocationType.BackBottom);
                    addPlayer(0, PlayerLocationType.FrontTop);
                    addPlayer(1, PlayerLocationType.FrontBottom);
                }
                else
                {
                    addPlayer(mainIdx == 0 ? 1 : 0, PlayerLocationType.FrontBottom);
                    addPlayer(2, PlayerLocationType.FrontTop);
                    addPlayer(3, PlayerLocationType.BackBottom);
                }
                break;
            case PlayerLocationType.BackBottom:
                if (mainIdealLocation == mainRealLocation)
                {
                    addPlayer(mainIdx == 2 ? 3 : 2, PlayerLocationType.BackTop);
                    addPlayer(0, PlayerLocationType.FrontTop);
                    addPlayer(1, PlayerLocationType.FrontBottom);
                }
                else
                {
                    addPlayer(mainIdx == 0 ? 1 : 0, PlayerLocationType.FrontTop);
                    addPlayer(2, PlayerLocationType.BackTop);
                    addPlayer(3, PlayerLocationType.FrontBottom);
                }
                break;
        }
        return players;
    }

    /// <summary>
    /// 获取预想位置
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainIdx"></param>
    /// <returns></returns>
    PlayerLocationType getIdealLocation(Grid mainGrid, int mainIdx)
    {
        var location = PlayerLocationType.FrontTop;
        if (mainIdx > 1)
        {
            location += 2;
        }
        if (mainGrid.row < world.btConfig.row)
        {
            location += 1;
        }
        return location;
    }

    /// <summary>
    /// 获取实际位置
    /// </summary>
    /// <param name="idealLocation"></param>
    /// <param name="mainGrid"></param>
    /// <returns></returns>
    PlayerLocationType getRealLocation(PlayerLocationType idealLocation, Grid mainGrid)
    {
        if (idealLocation < PlayerLocationType.FrontTop || PlayerLocationType.BackBottom < idealLocation)
        {
            Debuger.LogError(string.Format("location越界  mainGrid:({0},{1})  location:{2} ", mainGrid.row, mainGrid.col, idealLocation));
            return idealLocation;
        }
        if (idealLocation <= PlayerLocationType.FrontBottom)
        {
            if (Math.Abs(mainGrid.col - backCol) > world.btConfig.minColWithBorder)
            {
                return idealLocation;
            }
            else
            {
                return idealLocation + 2;
            }
        }
        else
        {
            if (Math.Abs(mainGrid.col - frontCol) > world.btConfig.minColWithBorder)
            {
                return idealLocation;
            }
            else
            {
                return idealLocation - 2;
            }
        }
    }

    /// <summary>
    /// 取上面的点
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainGrids"></param>
    /// <returns></returns>
    List<Grid> calculateTopGrids(Grid mainGrid, List<Grid> mainGrids, bool ignoreOccupied = false)
    {
        var beginRow = mainGrid.row + 1;
        var endRow = world.btConfig.row2 - 1;
        var beginCol = (int)Math.Max(0, Math.Ceiling((float)((Fix64)mainGrid.col - world.btConfig.mainGridRadius)));
        var endCol = (int)Math.Min(world.btConfig.col2 - 1, Math.Floor((float)((Fix64)mainGrid.col + world.btConfig.mainGridRadius)));
        var grids = new List<Grid>();
        for (int i = beginRow; i <= endRow; i++)
        {
            for (int j = beginCol; j <= endCol; j++)
            {
                var grid = new Grid() { row = i, col = j };
                if (!ignoreOccupied && isGridOccupied(grid, mainGrids)) continue;
                grids.Add(grid);
            }
        }
        if (grids.Count == 0 && !ignoreOccupied)
        {
            grids = calculateTopGrids(mainGrid, mainGrids, true);
        }
        return grids;
    }

    /// <summary>
    /// 取下面的点
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainGrids"></param>
    /// <returns></returns>
    List<Grid> calculateBottomGrids(Grid mainGrid, List<Grid> mainGrids, bool ignoreOccupied = false)
    {
        var beginRow = 0;
        var endRow = mainGrid.row - 1;
        var beginCol = (int)Math.Max(0, Math.Ceiling((float)((Fix64)mainGrid.col - world.btConfig.mainGridRadius)));
        var endCol = (int)Math.Min(world.btConfig.col2 - 1, Math.Floor((float)((Fix64)mainGrid.col + world.btConfig.mainGridRadius)));
        var grids = new List<Grid>();
        for (int i = beginRow; i <= endRow; i++)
        {
            for (int j = beginCol; j <= endCol; j++)
            {
                var grid = new Grid() { row = i, col = j };
                if (!ignoreOccupied && isGridOccupied(grid, mainGrids)) continue;
                grids.Add(grid);
            }
        }
        if (grids.Count == 0 && !ignoreOccupied)
        {
            grids = calculateBottomGrids(mainGrid, mainGrids, true);
        }
        return grids;
    }

    /// <summary>
    /// 取前面的点
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainGrids"></param>
    /// <returns></returns>
    List<Grid> calculateFrontGrids(Grid mainGrid, List<Grid> mainGrids, bool ignoreOccupied = false)
    {
        var beginRow = 0;
        var endRow = 0;
        if (mainGrid.row < world.btConfig.row)
        {
            endRow = world.btConfig.row - 1;
        }
        else
        {
            beginRow = world.btConfig.row;
            endRow = world.btConfig.row2 - 1;
        }
        var beginCol = enemyDoorOnLeft ? Math.Max(0, mainGrid.col - world.btConfig.sideExtendSize) : (mainGrid.col + 1);
        var endCol = enemyDoorOnLeft ? (mainGrid.col - 1) : Math.Min(world.btConfig.col2 - 1, mainGrid.col + world.btConfig.sideExtendSize);
        var grids = new List<Grid>();
        for (int i = beginRow; i <= endRow; i++)
        {
            for (int j = beginCol; j <= endCol; j++)
            {
                var grid = new Grid() { row = i, col = j };
                if (!ignoreOccupied && isGridOccupied(grid, mainGrids)) continue;
                grids.Add(grid);
            }
        }
        if (grids.Count == 0 && !ignoreOccupied)
        {
            grids = calculateFrontGrids(mainGrid, mainGrids, true);
        }
        return grids;
    }

    /// <summary>
    /// 取后面的点
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainGrids"></param>
    /// <returns></returns>
    List<Grid> calculateBackGrids(Grid mainGrid, List<Grid> mainGrids, bool ignoreOccupied = false)
    {
        var beginRow = 0;
        var endRow = 0;
        if (mainGrid.row < world.btConfig.row)
        {
            endRow = world.btConfig.row - 1;
        }
        else
        {
            beginRow = world.btConfig.row;
            endRow = world.btConfig.row2 - 1;
        }
        var beginCol = enemyDoorOnLeft ? (mainGrid.col + 1) : Math.Max(0, mainGrid.col - world.btConfig.sideExtendSize);
        var endCol = enemyDoorOnLeft ? Math.Min(world.btConfig.col2 - 1, mainGrid.col + world.btConfig.sideExtendSize) : (mainGrid.col - 1);
        var grids = new List<Grid>();
        for (int i = beginRow; i <= endRow; i++)
        {
            for (int j = beginCol; j <= endCol; j++)
            {
                var grid = new Grid() { row = i, col = j };
                if (!ignoreOccupied && isGridOccupied(grid, mainGrids)) continue;
                grids.Add(grid);
            }
        }
        if (grids.Count == 0 && !ignoreOccupied)
        {
            grids = calculateBackGrids(mainGrid, mainGrids, true);
        }
        return grids;
    }

    /// <summary>
    /// 该店是否被占用
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="mainGrids"></param>
    /// <returns></returns>
    bool isGridOccupied(Grid grid, List<Grid> mainGrids)
    {
        if (mainGrids == null || mainGrids.Count == 0) return false;
        for (int i = 0; i < mainGrids.Count; i++)
        {
            var distance = world.btConfig.calculateGridDistance(grid, mainGrids[i]);
            if (distance <= world.btConfig.mainGridWorldRadius)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取门将
    /// </summary>
    /// <returns></returns>
    FBActor getGK()
    {
        return teamMates.FirstOrDefault(a => a.isDoorKeeper());
    }

    #endregion

    #region public methods

    public override void updateBehaviour()
    {
        if (updateInterval > 1 && updateFrameCounter++ % updateInterval != 0) return;
        base.updateBehaviour();
        if (players.Count > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].updateBehaviour();
            }
        }
    }

    public void addPlayer(FBPlayerAgent player)
    {
        if (player == null)
        {
            Debuger.LogError("player is null");
            return;
        }
        if (players.Contains(player)) return;
        players.Add(player);
        //UnityEngine.Debug.LogError("addPlayer " + player.actor.id + "   "  +team );
        hasAiPlayer = true;
    }

    public void removePlayer(FBPlayerAgent player)
    {
        if (player == null)
        {
            Debuger.LogError("agent is null");
            return;
        }
        if (players.Count == 0) return;
        if (!players.Contains(player)) return;
        players.Remove(player);
        hasAiPlayer = players.Count > 0;
        //UnityEngine.Debug.LogError("removePlayer " + player.actor.id+ "   "  +team );
    }

    public override void update(Fix64 deltaTime)
    {
        if (players.Count == 0) return;

        for (int i = 0; i < players.Count; i++)
        {
            players[i].update(deltaTime);
        }
    }

    public override void clear()
    {
        if (players.Count == 0) return;

        for (int i = 0; i < players.Count; i++)
        {
            players[i].clear();
        }
    }

    public override void reset()
    {
        base.reset();
        setPlayersBehaviour(CoachCmd.Idle);
    }

    #endregion

    #region 行为树调用的接口

    /// <summary>
    /// 获取球员Id
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public uint getPlayerId(int index)
    {
        if (index < 0 || teamMatesExcludeGK.Count <= index) return 0;
        return teamMatesExcludeGK[index].id;
    }

    /// <summary>
    /// 获取队伍状态
    /// </summary>
    /// <returns></returns>
    public TeamState getTeamState()
    {
        return gameInfo.getTeamState(team);
    }

    /// <summary>
    /// 设置所有球员的行为树
    /// </summary>
    /// <param name="cmd"></param>
    public void setPlayersBehaviour(CoachCmd cmd)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].changeBehaviour(cmd);
        }
    }

    /// <summary>
    /// 设置球员的行为树
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="id"></param>
    public void setPlayerBehaviour(CoachCmd cmd, uint id)
    {
        //if (IdTest.instance != null)
        //{
        //    if (IdTest.instance.team == team)
        //    {
        //        if (IdTest.instance.id == 23 || IdTest.instance.id == id)
        //        {
        //            UnityEngine.Debug.LogError("setPlayerBehaviour  " + cmd + "   team  " + team + "   id  " + id);
        //        }
        //    }

        //}

        var agent = getPlayer(id);
        if (agent == null) return;
        agent.changeBehaviour(cmd);
    }

    /// <summary>
    /// 计算球员的移动目标
    /// </summary>
    /// <returns></returns>
    public FixVector2 calculatePlayerMovePosition()
    {
        return FixVector2.kZero;
    }

    /// <summary>
    /// 设置球员的移动目标
    /// </summary>
    /// <param name="id"></param>
    /// <param name="position"></param>
    public void setPlayerMovePosition(uint id, BT.Vector3 position)
    {
        var agent = getPlayer(id);
        if (agent == null) return;
        agent.setMoveTargetPosition(position);
    }

    /// <summary>
    /// 是否持球
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool hasBall(uint id)
    {
        var value = false;
        if (ball.hasOwner) value = ball.owner.id == id;
        return value;
    }

    /// <summary>
    /// 获取最近的敌人
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public uint getNearEnemy(uint id)
    {
        return world.getNearEnemy(id);
    }

    /// <summary>
    /// 获取球员状态
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public PlayerState getPlayerState(uint id)
    {
        if (id <= 0)
        {
            Debuger.LogError("id <= 0");
            return 0;
        }
        var agent = getPlayer(id);
        if (agent == null) return PlayerState.Idle;
        return agent.state;
    }

    /// <summary>
    /// 设置参数 playerId
    /// </summary>
    /// <param name="player"></param>
    /// <param name="target"></param>
    public void setPlayerId(uint player, uint target)
    {
        if (player <= 0)
        {
            Debuger.LogError("player <= 0 :" + player);
            return;
        }
        if (target <= 0)
        {
            Debuger.LogError("target <= 0 :" + target);
            return;
        }
        var agent = getPlayer(player);
        if (agent == null) return;
        agent.setPlayerId(target);
    }

    /// <summary>
    /// 球是否自由
    /// </summary>
    /// <returns></returns>
    public bool isBallFree()
    {
        return world.isBallFree();
    }

    /// <summary>
    /// 获取最近的球员
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public uint getNearPlayer(PlayerType type)
    {
        var id = world.getNearPlayer(team, type);
        return id;
    }

    /// <summary>
    /// 获取进球者
    /// </summary>
    /// <returns></returns>
    public uint getGoaler()
    {
        if (gameInfo.goalTeam != team) return 0;

        var actor = world.getActor(gameInfo.goaler);
        if (actor == null) return 0;
        if (actor.team != team) return 0;
        return gameInfo.goaler;
    }

    /// <summary>
    /// 计算随机进球者移动目标
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    public FixVector2 calculateRandomGoalerMovePosition(Fix64 radius)
    {
        var player = getPlayer(gameInfo.goaler);
        if (player == null) return FixVector2.kZero;
        return player.calculateRandomMovePosition(radius);
    }

    /// <summary>
    /// 计算进球队伍的球员的移动目标
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetPosition"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    public FixVector2 calculateGoalTeamMovePosition(uint id, FixVector2 targetPosition, Fix64 minRadius, Fix64 maxRadius)
    {
        var player = getPlayer(id);
        if (player == null) return targetPosition;
        return player.calculateGoalTeamMovePosition(targetPosition, minRadius, maxRadius);
    }

    /// <summary>
    /// 获取持球者
    /// </summary>
    /// <returns></returns>
    public uint getKeepingBallPlayerId()
    {
        //UnityEngine.Debug.LogError("getKeepingBallPlayerId " + ball.owner.id);
        return ball.hasOwner ? ball.owner.id : 0;
    }

    /// <summary>
    /// 计算跑向最佳射门点的球员
    /// </summary>
    /// <returns></returns>
    public uint calculateBestShootPlayer(Grid mainGrid)
    {
        if (players.Count == 0) return 0;

        var scores = new List<int>();
        var maxScore = int.MinValue;
        var id = 0u;
        var keepingBallPlayerId = getKeepingBallPlayerId();
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var position = player.actor.getPosition();
            var score = calculateShootScore4BestCatchBallPlayer(mainGrid, world.btConfig.getGrid(position, enemyDoorOnLeft), position);
            scores.Add(score);
            if (maxScore == score)
            {
                if (player.actor.id == keepingBallPlayerId)
                {
                    id = player.actor.id;
                }
            }
            else if (maxScore < score)
            {
                id = player.actor.id;
                maxScore = score;
            }
        }
        if (id == keepingBallPlayerId) return id;

        maxScore = int.MinValue;
        id = 0;
        var keepingBallPlayerPosition = ball.owner.getPosition();
        var keepingBallPlayerGrid = world.btConfig.getGrid(keepingBallPlayerPosition, enemyDoorOnLeft);
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var playerPosition = player.actor.getPosition();
            var playerGrid = world.btConfig.getGrid(playerPosition, enemyDoorOnLeft);
            var score = 0;
            if (player.actor.id == keepingBallPlayerId)
            {
                score = world.btConfig.shootPlayerSelfPassBallScore;
            }
            else
            {
                score = calculatePassScore4BestCatchBallPlayer(keepingBallPlayerGrid, keepingBallPlayerPosition, playerGrid, playerPosition);
            }
            scores[i] += score;
            if (maxScore == scores[i])
            {
                if (player.actor.id == keepingBallPlayerId)
                {
                    id = player.actor.id;
                }
            }
            else if (maxScore < scores[i])
            {
                id = player.actor.id;
                maxScore = scores[i];
            }
        }
        //UnityEngine.Debug.LogError("calculateBestShootPlayer " + id);
        return id;
    }

    /// <summary>
    /// 计算前半场最佳射门点
    /// </summary>
    /// <returns></returns>
    public Grid calculateBestShootGrid()
    {
        var maxScore = int.MinValue;
        var bestGrid = new Grid();
        for (int i = 0; i < world.btConfig.row2; i++)
        {
            for (int j = beginCol; j < endCol; j++)
            {
                var score = calculateShootScore(world.btConfig.getPosition(i, j));
                if (score == maxScore)
                {
                    if (isCloseDoor(bestGrid.row, bestGrid.col, i, j))
                    {
                        bestGrid.row = i;
                        bestGrid.col = j;
                    }
                }
                else if (maxScore < score)
                {
                    maxScore = score;
                    bestGrid.row = i;
                    bestGrid.col = j;
                }
            }
        }
        return bestGrid;
    }

    /// <summary>
    /// 检测射门
    /// </summary>
    /// <returns></returns>
    public bool shootCheck()
    {
        if (!ball.hasOwner) return false;
        var shootScore = calculateShootScore(ball.owner.getPosition());
        //UnityEngine.Debug.LogError("shoot check " + shootScore);
        return shootScore >= world.btConfig.shootScoreStandard;
    }

    /// <summary>
    /// 获取接球球员
    /// </summary>
    /// <returns></returns>
    public uint getBallReceiver()
    {
        //UnityEngine.Debug.LogError("world.passBallTargetId " + world.passBallTargetId);

        return world.passBallTargetId;
    }

    /// <summary>
    /// 转换为坐标
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    public FixVector2 covertToPosition(Grid grid)
    {
        return world.btConfig.getPosition(grid);
    }

    /// <summary>
    /// 获取离球最近的格子
    /// </summary>
    /// <returns></returns>
    public Grid getBallGrid()
    {
        return world.btConfig.getGrid(ball.getPosition(), enemyDoorOnLeft);
    }

    /// <summary>
    /// 获取防御的球员列表
    /// </summary>
    /// <param name="ballState"></param>
    /// <param name="attackPlayerNearBall"></param>
    /// <param name="defensePlayerNearBall"></param>
    /// <returns></returns>
    public DefenseStrategy getDefenseStrategy(BallState ballState, uint attackPlayerNearBall, uint defensePlayerNearBall)
    {
        if (teamMates == null || teamMates.Count == 0 || enemys == null || enemys.Count == 0) return default(DefenseStrategy);

        var defenseIds = teamMates.Where(a => !a.isDoorKeeper() && a.id != defensePlayerNearBall)
                                    .Select(b => b.id)
                                    .ToArray();
        var attackIds = enemys.Where(a => !a.isDoorKeeper() && a.id != attackPlayerNearBall)
                                    .Select(b => b.id)
                                    .ToArray();
        if (defenseIds == null || defenseIds.Length == 0 || attackIds == null || attackIds.Length == 0) return default(DefenseStrategy);

        var defensePermutations = PermutationAndCombination<uint>.GetPermutation(defenseIds, defenseIds.Length);
        var attackPermutations = PermutationAndCombination<uint>.GetPermutation(attackIds, attackIds.Length);
        uint[] bestDefenseIds = null;
        uint[] bestAttackIds = null;
        Fix64 minDistance = Fix64.MaxValue;
        for (int i = 0; i < defensePermutations.Count; i++)
        {
            var tmpDefenseIds = defensePermutations[i];
            for (int j = 0; j < attackPermutations.Count; j++)
            {
                var tmpAttackIds = attackPermutations[j];
                if (isPermutationCalculated(bestDefenseIds, bestAttackIds, tmpDefenseIds, tmpAttackIds)) continue;

                var distanceSum = calculateDefenseDistaneSum(tmpDefenseIds, tmpAttackIds);
                if (distanceSum < minDistance)
                {
                    minDistance = distanceSum;
                    bestDefenseIds = tmpDefenseIds;
                    bestAttackIds = tmpAttackIds;
                }
            }
        }
        return new DefenseStrategy { defensePlayer = bestDefenseIds.ToList(), attackPlayer = bestAttackIds.ToList() };
    }

    /// <summary>
    /// 获取防守目标
    /// </summary>
    /// <param name="defensePlayer"></param>
    /// <param name="defenseStrategy"></param>
    /// <returns></returns>
    public uint getDefenseTarget(uint defensePlayer, DefenseStrategy defenseStrategy)
    {
        if (defenseStrategy.defensePlayer == null || defenseStrategy.attackPlayer == null) return 0;
        for (int i = 0; i < defenseStrategy.defensePlayer.Count; i++)
        {
            if (defenseStrategy.defensePlayer[i] == defensePlayer) return defenseStrategy.attackPlayer[i];
        }
        return 0;
    }

    /// <summary>
    /// 获取离球最近的队员
    /// </summary>
    /// <returns></returns>
    public uint getNearBallPlayer()
    {
        var id = getNearBallActor(teamMates);
        //UnityEngine.Debug.LogError("getNearBallPlayer:" + id);
        return getNearBallActor(teamMates);
    }

    /// <summary>
    /// 获取离球最近的敌人
    /// </summary>
    /// <returns></returns>
    public uint getNearBallEnemy()
    {
        var id = getNearBallActor(enemys);
        return id;
    }

    /// <summary>
    /// 获取离球最近的球员(不包含门将)
    /// </summary>
    /// <returns></returns>
    public uint getNearBallActor(List<FBActor> actors)
    {
        if (actors == null || actors.Count == 0)
        {
            //Debuger.LogError("actors is null");
            return 0;
        }

        var pos = ball.getPosition();
        var minDistance = Fix64.MaxValue;
        uint id = 0;
        for (int i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];
            if (actor.isDoorKeeper()) continue;

            var distance = actor.getPosition().squareDistance(pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                id = actor.id;
            }
        }
        return id;
    }

    /// <summary>
    /// 获取离球员最近的格子
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Grid getPlayerGrid(uint id)
    {
        var player = getPlayer(id);
        if (player == null) return default(Grid);
        return world.btConfig.getGrid(player.actor.getPosition(), enemyDoorOnLeft);
    }

    /// <summary>
    /// 检测是否为AI
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool isAI(uint id)
    {
        if (id <= 0) return false;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].actor.id == id) return true;
        }
        return false;
    }

    public BallState getBallState()
    {
        var state = ball.ballState;
        if (state == BallState.PassBall && getBallReceiver() == 0)
            return BallState.Free;
        return state;
    }

    /// <summary>
    /// 获取球员跑动位置
    /// </summary>
    /// <param name="mainGrid"></param>
    /// <param name="mainId"></param>
    /// <returns></returns>
    public PersetPosition getAllPlayerDistributePositionNormalBall(Grid mainGrid, Grid ballGrid, uint mainId)
    {
        if (!hasAiPlayer) return default(PersetPosition);

        if (mainId <= 0)
        {
            Debuger.LogError("mainId <=0 " + mainId);
            return default(PersetPosition);
        }

        var playerDic = sortPlayers(mainGrid, mainId);
        if (playerDic == null) return default(PersetPosition);

        var targetPositionDic = calculateTargetPosition(mainGrid, ballGrid, mainId, playerDic[mainId]);
        if (targetPositionDic == null) return default(PersetPosition);

        return new PersetPosition
        {
            playerIds = playerDic.OrderBy(a => a.Value).Select(b => b.Key).Where(c => c != mainId).ToList(),
            targetPosition = targetPositionDic.OrderBy(a => a.Key).Select(b => b.Value).ToList()
        };
    }

    /// <summary>
    /// 球在门将是获取球员的跑动位置
    /// </summary>
    /// <returns></returns>
    public PersetPosition getAllPlayerDistributePositionGKBall()
    {
        var gk = getGK();
        if (gk == null)
        {
            Debuger.LogError("getAllPlayerDistributeAreaGKBall 门将不存在！");
            return default(PersetPosition);
        }

        var gkGrid = world.btConfig.getGrid(gk.getPosition(), enemyDoorOnLeft);
        var mainGrid = calculateBestMoveGrid(world.btConfig.getGKBackTopGrids(enemyDoorOnLeft), gkGrid);
        var playerIds = getSortActors();

        if (playerIds.Count == 0) return default(PersetPosition);

        var mainId = playerIds[Math.Max(0, playerIds.Count - 2)];
        var persetPosition = getAllPlayerDistributePositionNormalBall(mainGrid, gkGrid, mainId);
        persetPosition.playerIds.Add(mainId);
        persetPosition.targetPosition.Add(BTCoach.convertToVector3(world.btConfig.getPosition(mainGrid)));
        return persetPosition;
    }

    public int getPlayerCountExcludeGK()
    {
        return teamMatesExcludeGK.Count;
    }

    public bool isCharging()
    {
        var player = getKeepingBallPlayer();
        if (player == null) return false;
        return player.state == PlayerState.Charge;
    }

    public BT.Vector3 getBestReceiveBallPosition(uint playerId, PersetPosition attackReceiveBallPosition)
    {
        if (playerId <= 0)
        {
            Debuger.LogError("playerId <= 0 " + playerId);
            return default(BT.Vector3);
        }
        if (attackReceiveBallPosition.playerIds == null) return default(BT.Vector3);
        if (attackReceiveBallPosition.targetPosition == null) return default(BT.Vector3);
        if (!attackReceiveBallPosition.playerIds.Contains(playerId)) return default(BT.Vector3);

        var idx = attackReceiveBallPosition.playerIds.IndexOf(playerId);
        if (attackReceiveBallPosition.targetPosition.Count <= idx) return default(BT.Vector3);

        return attackReceiveBallPosition.targetPosition[idx];
    }

    #endregion
}
