using UnityEngine;
namespace FBCamera
{
    public class MoveAction : CameraActionBase
    {
        public float? beginTime = null;
        public Vector3 target = Vector3.zero;
        public Vector3 getTarget(bool inverse = false)
        {
            return inverse ? new Vector3 { x = -target.x, y = target.y, z = target.z } : target;
        }
    }

}