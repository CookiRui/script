using UnityEngine;

namespace FBCamera
{
    abstract class CameraFollowStateBase
    {
        protected CameraCtrl cameraCtrl { get; private set; }
        protected float startY { get; private set; }
        protected float startAngle { get; private set; }

        public CameraFollowStateBase(CameraCtrl ctrl)
        {
            cameraCtrl = ctrl;
        }

        #region abstract methods
        protected abstract Vector3 getLookAtPos();
        protected abstract float calculateY();
        protected abstract float calculateAngle();
        #endregion

        #region virtual methods

        public virtual bool smoothXZ { get { return true; } }
        public virtual bool smoothY { get { return true; } }
        public virtual bool directlyPosition { get { return false; } }

        public virtual float xzVelocity { get { return cameraCtrl.xzVelocity; } }
        public virtual float yVelocity { get; protected set; }
        public virtual float yVelocityConstB { get; protected set; }

        protected virtual float clampX(float x)
        {
            return Mathf.Clamp(x, cameraCtrl.config.leftBorder, cameraCtrl.config.rightBorder);
        }

        protected virtual float clampY(float y)
        {
            return Mathf.Clamp(y, cameraCtrl.config.yMinBorder, cameraCtrl.config.yMaxBorder);
        }

        public virtual void enter()
        {
            startY = cameraCtrl.transform.position.y;
            startAngle = cameraCtrl.transform.localEulerAngles.x;
        }

        public virtual void exit() { }

        #endregion

        #region private methods

        float clampZ(float z)
        {
            return Mathf.Clamp(z, cameraCtrl.config.bottomBorder, cameraCtrl.config.topBorder);
        }

        float clampAngle(float angle)
        {
            return Mathf.Clamp(angle, cameraCtrl.config.chargingAngle, cameraCtrl.config.defaultAngle);
        }

        #endregion

        #region protected methods

        protected Vector3 getPlayerViewport()
        {
            return getActorViewport(cameraCtrl.player);
        }

        protected Vector3 getBallViewport()
        {
            return cameraCtrl.cam.WorldToViewportPoint(cameraCtrl.ball.transform.position);
        }

        protected Vector3 getActorViewport(ActorView actor)
        {
            if (actor == null)
            {
                Debug.LogError("actor is null");
                return Vector3.zero;
            }
            return cameraCtrl.cam.WorldToViewportPoint(actor.getCenterPosition());
        }

        /// <summary>
        /// jlx2017.04.28-log:offset的计算公式： f(y) = a - (y - y0) * k
        /// </summary>
        /// <returns></returns>
        protected Vector3 calculateLookAtOffset()
        {
            var symbol = 0;
            var offset = 0f;
            var constK = 0f;
            if (cameraCtrl.camp == CampType.Attack)
            {
                symbol = cameraCtrl.doorOnLeft ? 1 : -1;
                offset = cameraCtrl.config.attackOffset;
                constK = cameraCtrl.config.attackConstK;
            }
            else
            {
                symbol = cameraCtrl.doorOnLeft ? -1 : 1;
                offset = cameraCtrl.config.defenceOffset;
                constK = cameraCtrl.config.defenceConstK;
            }
            return new Vector3 { x = symbol * (offset - (cameraCtrl.transform.position.y - cameraCtrl.config.yMinBorder) * constK) };
        }

        protected Vector3 viewport2WorldPoint(Vector3 viewport)
        {
            return cameraCtrl.cam.ViewportToWorldPoint(viewport);
        }

        protected Vector2 calculateIntersection(Vector2 begin1, Vector2 end1, Vector2 begin2, Vector2 end2)
        {
            var k1 = (begin1.x == end1.x) ? (float?)null : ((end1.y - begin1.y) / (end1.x - begin1.x));
            var k2 = (begin2.x == end2.x) ? (float?)null : ((end2.y - begin2.y) / (end2.x - begin2.x));
            Vector2 intersection;
            if (k1.HasValue)
            {
                if (k2.HasValue)
                {
                    if (k1.Value == k2.Value)
                    {
                        intersection = begin2;
                    }
                    else
                    {
                        var x = (k1.Value * begin1.x - k2.Value * begin2.x - begin1.y + begin2.y) / (k1.Value - k2.Value);
                        var y = k1.Value * (x - begin1.x) + begin1.y;
                        intersection = new Vector2(x, y);
                    }
                }
                else
                {
                    intersection = new Vector2(begin2.x, k1.Value * (begin2.x - begin1.x) + begin1.y);
                }
            }
            else
            {
                if (k2.HasValue)
                {
                    intersection = new Vector2(begin1.x, k2.Value * (begin1.x - begin2.x) + begin2.y);
                }
                else
                {
                    intersection = begin2;
                }
            }
            return intersection;
        }

        /// <summary>
        /// 射线于平面是否相交
        /// http://www.cnblogs.com/graphics/archive/2009/10/17/1585281.html
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        protected bool rayViewport2WorldPoint(Vector2 viewport, out Vector3 point)
        {
            var ray = Camera.main.ViewportPointToRay(viewport);
            var p0 = ray.origin;
            var u = ray.direction.normalized;

            var n = Vector3.up;
            var p1 = Vector3.zero;
            var t = (Vector3.Dot(n, p1) - Vector3.Dot(n, p0)) / Vector3.Dot(n, u);
            if (t >= 0)
            {
                point = p0 + u * t;
                return true;
            }
            point = Vector3.zero;
            return false;
        }

        #endregion

        #region public methods

        public virtual Vector3 calculatePosition()
        {
            var lookAtPos = getLookAtPos();
            var y = calculateY();
            return new Vector3
            {
                x = clampX(lookAtPos.x),
                y = clampY(y),
                z = clampZ(lookAtPos.z),
            };
        }

        public virtual Vector3 calculateAngles()
        {
            var angle = calculateAngle();
            return new Vector3 { x = clampAngle(angle) };
        }

        #endregion
    }
}