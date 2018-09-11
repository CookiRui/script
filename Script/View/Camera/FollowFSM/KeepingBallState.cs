using UnityEngine;
namespace FBCamera
{
    class KeepingBallState : CameraFollowStateBase
    {
        bool inCorner;
        public KeepingBallState(CameraCtrl ctrl) : base(ctrl) { }

        #region override methods

        protected override float calculateY()        {            return cameraCtrl.config.yMinBorder;        }


        public override float yVelocity        {            get            {                return cameraCtrl.config.fallVelocity;            }        }

        public override float yVelocityConstB        {            get            {                return cameraCtrl.config.fallConstB;            }        }

        protected override float calculateAngle()
        {
            var t = 0f;
            if (startY == cameraCtrl.config.yMinBorder)
            {
                t = 0.1f;
            }
            else
            {
                t = (cameraCtrl.transform.position.y - startY) / (cameraCtrl.config.yMinBorder - startY);
            }
            return Mathf.Lerp(startAngle, cameraCtrl.config.defaultAngle, t);
        }

        protected override float clampX(float x)
        {
            if (inCorner) return x;
            return base.clampX(x);
        }

        protected override Vector3 getLookAtPos()
        {
            var playerViewport = getPlayerViewport();
            var offset = calculateCornerOffset(playerViewport);
            inCorner = offset.HasValue;
            var lookAtViewport = new Vector3
            {
                x = inCorner ? (0.5f + offset.Value) : (playerViewport.x + calculateLookAtOffset().x),
                y = playerViewport.y,
                z = playerViewport.z
            };
            return viewport2WorldPoint(lookAtViewport);
        }
        #endregion

        #region private methods

        float? calculateCornerOffset(Vector3 playerViewport)
        {
            var inCorner = false;
            var intersection = Vector2.zero;
            var constX = 1f;
            if (cameraCtrl.transform.position.x <= cameraCtrl.config.leftBorder + constX)
            {
                intersection = calculateIntersection(
                    cameraCtrl.config.leftBottomBegin,
                    cameraCtrl.config.leftBottomEnd,
                    playerViewport,
                    new Vector2 { x = playerViewport.x + constX, y = playerViewport.y });
                if (cameraCtrl.transform.position.x < cameraCtrl.config.leftBorder - constX)
                {
                    inCorner = true;
                }
                else
                {
                    inCorner = playerViewport.x <= intersection.x;
                }
            }
            else if (cameraCtrl.transform.position.x >= cameraCtrl.config.rightBorder - constX)
            {
                intersection = calculateIntersection(
                    cameraCtrl.config.rightBottomBegin,
                    cameraCtrl.config.rightBottomEnd,
                    playerViewport,
                    new Vector2 { x = playerViewport.x + constX, y = playerViewport.y });
                if (cameraCtrl.transform.position.x > cameraCtrl.config.rightBorder + constX)
                {
                    inCorner = true;
                }
                else
                {
                    inCorner = playerViewport.x >= intersection.x;
                }
            }
            if (inCorner)
            {
                return playerViewport.x - intersection.x;
            }
            return null;
        }

        /// <summary>
        /// x = x1 + (x2 - x1) / (y2-y1) * (y - y1)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        float calculateXOnLineByY(Vector2 begin, Vector2 end, float y)
        {
            if (begin.y == end.y
                || begin.y == y) return begin.x;

            return begin.x + (end.x - begin.x) / (end.y - begin.y) * (y - begin.y);
        }

        #endregion

    }
}