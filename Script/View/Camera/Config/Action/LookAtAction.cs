namespace FBCamera
{
    public enum LookAtType
    {
        Ball = 1,
        Player,
        GK,
    }

    public class LookAtAction : CameraActionBase
    {
        public LookAtType lookAtType = LookAtType.Ball;
    }
}