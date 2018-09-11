using BW31.SP2D;
using FixMath.NET;

public partial class FBActor
{
    //防御状态移动方向
    public enum DefendMoveDirection
    {
        None,
        Forward,
        Back,
        Left,
        Right
    }

    protected FixVector2 lastPosition = FixVector2.kZero;
    protected Fix64 lastHeight = Fix64.Zero;
    protected FixVector2 lastDirection = FixVector2.kZero;
    public bool ignoreDirectionSample = false;
    public bool ignoreDirectionSlerp = false;
    public bool ignorePositionSampleSlerp = false;
    public bool forceCheck = true;
    DefendMoveDirection defendMoveDirection = DefendMoveDirection.None;
    bool endMove;
    //移动高度
    public Fix64 height
    {
        get { return _height; }
        set
        {
            _height = value;
        }
    }
    Fix64 _height = Fix64.Zero;

    DefendMoveDirection getMoveDirection()
    {
        Fix64 cosAngle = FixVector2.dot(direction, moveDirection);
        if (cosAngle > Fix64.Cos(configuration.dm1_angleA))
        {
            return DefendMoveDirection.Forward;
        }
        else if (cosAngle < Fix64.Cos(configuration.dm1_angleB))
        {
            return DefendMoveDirection.Back;
        }
        else
        {
            Fix64 cross = FixVector2.cross(direction, moveDirection);
            if (cross < Fix64.Zero)
            {
                return DefendMoveDirection.Right;
            }
            else
            {
                return DefendMoveDirection.Left;
            }
        }
    }
    public void beginSample()
    {
        lastPosition = getPosition();
        lastHeight = height;
        lastDirection = direction;
    }

    public void endSample()
    {
        FixVector2 curPosition = getPosition();
        var length = (curPosition - lastPosition).squareLength;
        var heightDelta = Fix64.Abs(height - lastHeight);
        if (forceCheck || length > (Fix64)0.0001f || height > Fix64.Zero)
        {
            if (height > Fix64.Zero)
            {
                world.fbGame.generateRenderAction<RAL.ActorMovingAction3D>( id, new FixVector3(curPosition.x, height, curPosition.y).toVector3(), ignorePositionSampleSlerp, (uint)defendMoveDirection );
            }
            else
            {
                world.fbGame.generateRenderAction<RAL.ActorMovingAction>(id, new FixVector2(curPosition.x, curPosition.y).toVector2(), ignorePositionSampleSlerp, (uint)defendMoveDirection);
            }
            if (endMove)
            {
                endMove = false;
            }
        }
        else
        {
            if (!endMove)
            {
                world.fbGame.generateRenderAction<RAL.ActorEndMoveAction>(id);
                endMove = true;
            }
        }

        if (checkMovingState()
            && particle.velocity != FixVector2.kZero
            && length <= (Fix64)1e-3)
        {
            world.fbGame.generateRenderAction<RAL.ChangeActorAnimatorSpeedAction>(id, (float)calculateAnimatorSpeedOfRun(0));
        }
        if (ignorePositionSampleSlerp)
        {
            ignorePositionSampleSlerp = false;
        }

        bool sampleDirection = forceCheck || (!ignoreDirectionSample && FixVector2.dot(direction, lastDirection) < Fix64.One && direction != FixVector2.kZero);
        if (sampleDirection)
        {
            world.fbGame.generateRenderAction<RAL.TurningAction>(this.id, lastDirection.toVector2(), direction.toVector2(), ignoreDirectionSlerp);
        }

        if (ignoreDirectionSample)
            ignoreDirectionSample = false;
        if (ignoreDirectionSlerp)
            ignoreDirectionSlerp = false;

        if (forceCheck)
        {
            forceCheck = false;
        }
    }
}

