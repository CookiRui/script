using BW31.SP2D;
using FixMath.NET;

public partial class FBBall
{
    public enum SampleType
    {
        NormalSample,               //正常差值采样
        IgnorePositionSampleSlerp,  //取消差值直接采样
        TimeSlerp,                  //特定时间采样
    };

    SampleType positionSampleType = SampleType.NormalSample;
    Fix64 slerpTotalTime = Fix64.Zero;

    public void setSampleType(SampleType sampleType)
    {
        positionSampleType = sampleType;
    }
    public void setSampleType(SampleType sampleType, Fix64 time)
    {
        positionSampleType = sampleType;
        slerpTotalTime = time;
    }

    //物理位置取样
    protected Fix64 lastSampleTime = -BW31.SP2D.World.kEpsilon;
    protected FixVector3 lastPosition = FixVector3.kZero;

    public bool forceCheck { private get; set; }

    public void beginSample()
    {
        lastPosition = get3DPosition();
    }

    public void endSample()
    {
        FixVector3 curPosition = get3DPosition();
        if (this.owner == null && 
            (forceCheck || (curPosition - lastPosition).squareLength > (Fix64)0.0001f)
            )
        {
            if (positionSampleType == FBBall.SampleType.TimeSlerp)
            {
                world.fbGame.generateRenderAction<RAL.BallSlerpMoveAction>(curPosition.toVector3(), (float)slerpTotalTime);
            }
            else if (positionSampleType == FBBall.SampleType.IgnorePositionSampleSlerp)
            {
                world.fbGame.generateRenderAction<RAL.InstantBallMovingAction>(curPosition.toVector3());
            }
            else if ( positionSampleType == FBBall.SampleType.NormalSample )
            {
                world.fbGame.generateRenderAction<RAL.BallMovingAction>(curPosition.toVector3());
            }
        }
        if (positionSampleType == SampleType.IgnorePositionSampleSlerp || positionSampleType == SampleType.TimeSlerp )
        {
            positionSampleType = SampleType.NormalSample;
        }
        if (forceCheck)
        {
            forceCheck = false;
        }

    }


    public void willUpdate()
    {
 
    }
}