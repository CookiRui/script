using FixMath.NET;
using BW31.SP2D;
using System;
using System.Collections.Generic;

public partial class FBWorld
{
    public class BTConfiguration
    {
        public int testIntConst;
        public Fix64 testFloatConst;
        public bool testBoolConst;

        #region 射门分数相关
        /// <summary>
        /// 计算射门分数：基础分数M。计算公式：S=M-a*L-b0*K1-b1*K2-b2*K3
        /// </summary>
        public int shootBaseScore;
        /// <summary>
        /// 射门标准分数，判定持球球员是否射门
        /// </summary>
        public int shootScoreStandard;
        /// <summary>
        /// 计算射门分数：距离参数a。计算公式：S=M-a*L-b0*K1-b1*K2-b2*K3
        /// </summary>
        public Fix64 shootPositionDistanceRate;
        /// <summary>
        /// 计算前往最佳射门点的球员分数时，球员周围危险分数所占比例
        /// </summary>
        public Fix64 shootPlayerAroundDangerScoreRate;
        /// <summary>
        /// 计算前往最佳射门点的球员分数时候，持球者自身的传球分数
        /// </summary>
        public int shootPlayerSelfPassBallScore;
        /// <summary>
        /// 计算前往最佳射门点的球员分数时，传球分数所占比例
        /// </summary>
        public Fix64 shootPlayerPassBallScoreRate;
        /// <summary>
        /// 计算射门分数时，射门区域内敌方球员的影响因数
        /// </summary>
        public Fix64 shootAreaEnemyEffect;
        /// <summary>
        /// 计算射门分数时，射门区域内己方球员的影响因数
        /// </summary>
        public Fix64 shootAreaTeamEffect;
        /// <summary>
        /// 计算射门分数时，射门区域周围区域内敌方球员的影响因数
        /// </summary>
        public Fix64 shootAreaAroundEnemyEffect;
        /// <summary>
        /// 射门时足球速度
        /// </summary>
        public Fix64 ballShootSpeed;

        /// <summary>
        /// 射门角度其中的K
        ///  M-a∙L+2θ⁄π∙K
        /// </summary>
        public Fix64 shootPositionAngleRate;

        #endregion

        #region 传球分数相关
        /// <summary>
        /// 计算传球分数：基础分数M。计算公式S=M-a*|L-L0|-k*n
        /// </summary>
        public int passBallBaseScore;
        /// <summary>
        /// 计算传球分数的距离比例系数a  计算公式S=M-a*|L-L0|-k*n
        /// </summary>
        public Fix64 passBallDistanceRate;
        /// <summary>
        /// 计算传球分数的最佳传球距离L0   计算公式S=M-a*|L-L0|-k*n
        /// </summary>
        public Fix64 passBallBestDistance;
        /// <summary>
        /// 计算传球分数的敌人影响因数k 计算公式S=M-a*|L-L0|-k*n
        /// </summary>
        public Fix64 passBallEnemyEffect;
        /// <summary>
        /// 判定传球为短传还是长传
        /// </summary>
        public Fix64 passBallTypeDistance;
        /// <summary>
        /// 计算传球分数时：目标点的射门分数影响因数
        /// </summary>
        public Fix64 passBallScoreShootScoreEffect;

        #endregion

        #region 周围危险情况
        /// <summary>
        /// 计算周围危险程度：基础分数K。计算公式：S=K-a*L1+K-a*L2
        /// </summary>
        public int aroundDangerBaseScore;
        /// <summary>
        /// 计算周围危险分数的范围
        /// </summary>
        public Fix64 aroundDangerRange;
        /// <summary>
        /// 计算周围危险分数的距离比例系数a S=K-a*L1-a*L2
        /// </summary>
        public Fix64 aroundDangerDistanceRate;

        #endregion

        /// <summary>
        /// 行数
        /// </summary>
        public int row;
        /// <summary>
        /// 列数
        /// </summary>
        public int col;

        /// <summary>
        /// 球员移动速度
        /// </summary>
        public Fix64 playerSpeed;

        public string coachBTPath;
        public string playerBTPath;
        public string gkBTPath;

        public FixVector2[,] points;
        public int row2;
        public int col2;
        FixVector2 unitCell;
        Fix64[,] distances;

        List<Grid> leftGKFrontTopGrids;
        List<Grid> leftGKFrontBottomGrids;
        List<Grid> leftGKBackTopGrids;
        List<Grid> leftGKBackBottomGrids;
        List<Grid> rightGKFrontTopGrids;
        List<Grid> rightGKFrontBottomGrids;
        List<Grid> rightGKBackTopGrids;
        List<Grid> rightGKBackBottomGrids;

        /// <summary>
        /// 教练更新间隔
        /// </summary>
        public byte coachUpdateInterval;

        /// <summary>
        /// 门将教练更新间隔
        /// </summary>
        public byte gkCoachUpdateInterval;

        /// <summary>
        /// 门将传球后摇时间
        /// </summary>
        public Fix64 gkPassBallWaitTime;

        /// <summary>
        /// 最小蓄力时间
        /// </summary>
        public Fix64 minChargeTime;

        /// <summary>
        /// 小红点的半径(单位：格子)
        /// 1、球员的控制范围X
        ///     球员周围距离其长度小于等于X的点不作为传球目标点。
        /// </summary>
        public Fix64 mainGridRadius;

        /// <summary>
        /// 小红点真实半径(单位：米)
        /// </summary>
        public Fix64 mainGridWorldRadius;

        /// <summary>
        /// 前方或者后方延展的大小(单位：格子)
        /// 2、同方（同上方或者同下方）另一名球员目标点选取范围X
        ///     以已经确定的球员所在列为基准向后数X个单位长度，同方小于等于X的点，作为后面球员的选取范围。
        /// </summary>
        public byte sideExtendSize;

        /// <summary>
        /// 离球场边界最小列(单位：格子)
        /// 3、最小距离X
        ///     已经确定位置的球员中心点距离底线小于等于X个单位长度，则同方另一个点在其他方向进行选取
        /// </summary>
        public byte minColWithBorder;

        public BTConfiguration(FixVector2 worldSize)
        {
            testIntConst = 1;
            testFloatConst = Fix64.Zero;
            testFloatConst = (Fix64)0;
            testBoolConst = true;


            //射门相关参数
            shootBaseScore = 500;
            shootScoreStandard = 380;
            shootPositionDistanceRate = (Fix64)5;
            shootPlayerAroundDangerScoreRate = (Fix64)1;
            shootPlayerSelfPassBallScore = 150;
            shootPlayerPassBallScoreRate = (Fix64)0.8;
            shootAreaEnemyEffect = (Fix64)20;
            shootAreaTeamEffect = (Fix64)5;
            shootAreaAroundEnemyEffect = (Fix64)5;
            shootPositionAngleRate = (Fix64)0;

            //球员周围危险分数相关参数
            aroundDangerBaseScore = 40;
            aroundDangerRange = (Fix64)10;
            //aroundDangerRange *= aroundDangerRange;
            aroundDangerDistanceRate = (Fix64)4;
            //传球相关参数
            passBallBaseScore = 150;
            passBallDistanceRate = (Fix64)10;
            passBallBestDistance = (Fix64)28;
            //passBallBestDistance *= passBallBestDistance;
            passBallEnemyEffect = (Fix64)10;
            passBallTypeDistance = (Fix64)15;
            passBallScoreShootScoreEffect = (Fix64)0.3;
            //基础参数
            playerSpeed = (Fix64)7;
            ballShootSpeed = (Fix64)50;


            coachBTPath = "Coach/CoachSubTree/";
            playerBTPath = "Coach/Player/";
            gkBTPath = "GKCoach/GK";
            coachUpdateInterval = 5;
            gkCoachUpdateInterval = 5;
            gkPassBallWaitTime = (Fix64)1;

            minChargeTime = (Fix64)0.2;

            mainGridRadius = (Fix64)2.5;

            sideExtendSize = 5;
            minColWithBorder = 3;

            initPoints(worldSize);
            initDistances();
            initGrids();


        }

        void initPoints(FixVector2 worldSize)
        {
            row = 3;
            col = 6;
            row2 = row << 1;
            col2 = col << 1;
            points = new FixVector2[row2, col2];
            unitCell = new FixVector2 { x = worldSize.x / (Fix64)col, y = worldSize.y / (Fix64)row };
            for (int i = 0; i < row2; i++)
            {
                var y = -worldSize.y + (Fix64)(i + 0.5f) * unitCell.y;
                for (int j = 0; j < col2; j++)
                {
                    var x = -worldSize.x + (Fix64)(j + 0.5f) * unitCell.x;
                    points[i, j] = new FixVector2(x, y);
                }
            }

            mainGridWorldRadius = mainGridRadius * unitCell.x;
        }

        void initDistances()
        {
            distances = new Fix64[row2, col2];
            for (int i = 0; i < row2; i++)
            {
                for (int j = i; j < col2; j++)
                {
                    var row = (Fix64)i * unitCell.y;
                    var col = (Fix64)j * unitCell.x;
                    distances[i, j] = Fix64.Sqrt(row * row + col * col);
                }
            }
        }

        void initGrids()
        {
            leftGKFrontTopGrids = getGrids(row, 5, row2, 8);
            leftGKFrontBottomGrids = getGrids(0, 5, row, 8);
            leftGKBackTopGrids = getGrids(row, 2, row2, 5);
            leftGKBackBottomGrids = getGrids(0, 2, row, 5);

            rightGKFrontTopGrids = getGrids(row, 4, row2, 7);
            rightGKFrontBottomGrids = getGrids(0, 4, row, 7);
            rightGKBackTopGrids = getGrids(row, 7, row2, 10);
            rightGKBackBottomGrids = getGrids(0, 7, row, 10);
        }

        public Grid getGrid(FixVector2 position, bool enemyDoorOnLeft)
        {
            var offset = position - points[0, 0];

            var realRow = offset.y / unitCell.y;
            var realCol = offset.x / unitCell.x;

            if (realRow < Fix64.Zero) realRow = Fix64.Zero;
            if (realCol < Fix64.Zero) realCol = Fix64.Zero;

            var smallRow = (Fix64)(int)realRow;
            var smallCol = (Fix64)(int)realCol;
            var offsetRow = realRow - smallRow;
            var offsetCol = realCol - smallCol;

            var tmpRow = 0;
            var tmpCol = 0;
            if (offsetRow < (Fix64)0.5
                || (int)smallRow == row2 - 1
                || (offsetRow == (Fix64)0.5
                    && Fix64.FastAbs(smallRow - ((Fix64)row - (Fix64)0.5f))
                        <= Fix64.FastAbs(smallRow + Fix64.One - ((Fix64)row - (Fix64)0.5f))))
            {
                tmpRow = (int)smallRow;
            }
            else
            {
                tmpRow = (int)smallRow + 1;
            }
            if (tmpRow >= row2)
            {
                Debuger.LogError(string.Format("出现计算异常。position:{0}  enemyDoorOnLeft:{1}  tmpRow:{2}", position, enemyDoorOnLeft, tmpRow));
                tmpRow = row2 - 1;
            }

            if (offsetCol < (Fix64)0.5
            || (int)smallCol == col2 - 1
            || (offsetCol == (Fix64)0.5 && enemyDoorOnLeft))
            {
                tmpCol = (int)smallCol;
            }
            else
            {
                tmpCol = (int)smallCol + 1;
            }
            if (tmpCol >= col2)
            {
                Debuger.LogError(string.Format("出现计算异常。position:{0}  enemyDoorOnLeft:{1}  tmpCol:{2}", position, enemyDoorOnLeft, tmpCol));
                tmpCol = col2 - 1;
            }
            return new Grid { row = tmpRow, col = tmpCol };
        }

        public Fix64 calculateGridDistance(int row, int col)
        {
            return row <= col ? getDistance(row, col) : getDistance(col, row);
        }

        Fix64 getDistance(int row, int col)
        {
            if (row < 0 || distances.GetLength(0) <= row)
            {
                Debuger.LogError("row is out of range:" + row);
                return Fix64.Zero;
            }
            if (col < 0 || distances.GetLength(1) <= col)
            {
                Debuger.LogError("col is out of range:" + col);
                return Fix64.Zero;
            }
            return distances[row, col];
        }

        public Fix64 calculateGridDistance(Grid grid1, Grid grid2)
        {
            var row = Math.Abs(grid1.row - grid2.row);
            var col = Math.Abs(grid1.col - grid2.col);
            if (row >= row2 || col >= col2)
            {
                Debuger.LogError("grid1 : " + grid1.row + "  " + grid1.col);
                Debuger.LogError("grid2 : " + grid2.row + "  " + grid2.col);
            }
            return calculateGridDistance(row, col);
        }

        public Fix64 calculateGridDistance(Grid grid, FixVector2 position, bool enemyDoorOnLeft)
        {
            return calculateGridDistance(grid, getGrid(position, enemyDoorOnLeft));
        }

        public Fix64 calculateGridDistance(FixVector2 position1, FixVector2 position2, bool enemyDoorOnLeft)
        {
            return calculateGridDistance(getGrid(position2, enemyDoorOnLeft), getGrid(position2, enemyDoorOnLeft));
        }

        public FixVector2 getPosition(Grid grid)
        {
            return getPosition(grid.row, grid.col);
        }

        public FixVector2 getPosition(int row, int col)
        {
            if (row < 0 || row2 <= row || col < 0 || col2 <= col) return FixVector2.kZero;
            return points[row, col];
        }

        public List<Grid> getGrids(int beginRow, int beginCol, int endRow, int endCol)
        {
            var grids = new List<Grid>();
            for (int i = beginRow; i < endRow; i++)
                for (int j = beginCol; j < endCol; j++)
                    grids.Add(new Grid { row = i, col = j });
            return grids;
        }

        public List<Grid> getGKFrontTopGrids(bool enemyDoorOnLeft)
        {
            return enemyDoorOnLeft ? rightGKFrontTopGrids : leftGKFrontTopGrids;
        }

        public List<Grid> getGKFrontBottomGrids(bool enemyDoorOnLeft)
        {
            return enemyDoorOnLeft ? rightGKFrontBottomGrids : leftGKFrontBottomGrids;
        }

        public List<Grid> getGKBackTopGrids(bool enemyDoorOnLeft)
        {
            return enemyDoorOnLeft ? rightGKBackTopGrids : leftGKBackTopGrids;
        }

        public List<Grid> getGKBackBottomGrids(bool enemyDoorOnLeft)
        {
            return enemyDoorOnLeft ? rightGKBackBottomGrids : leftGKBackBottomGrids;
        }
    }
}