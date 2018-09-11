namespace FBCamera
{
    public enum CameraActionType
    {
        LookAt = 1,
        FOV,
        Move,
    }

    public abstract class CameraActionBase
    {
        public CameraActionType type = CameraActionType.LookAt;
        public float? time = null;
        public int? weight = null;
    }
}