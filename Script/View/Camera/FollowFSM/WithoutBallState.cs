using UnityEngine;
namespace FBCamera
{
    class WithoutBallState : CameraFollowStateBase
    {
        enum MoveType { None, Fall, Rise }
        enum AreaType { OutSide, Ring, In, Corner }
        readonly Vector2 viewportCenter = Vector2.one * 0.5f;
        public WithoutBallState(CameraCtrl ctrl) : base(ctrl) { }

        protected virtual bool checkMinDistance { get { return true; } }

        #region override methods

        protected override Vector3 getLookAtPos()
        {
            return viewport2WorldPoint((getPlayerViewport() + getBallViewport()) * 0.5f + calculateLookAtOffset());
        }

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

        protected override float calculateY()
        {
            var currentY = cameraCtrl.transform.position.y;

            var playerViewport = cameraCtrl.cam.WorldToViewportPoint(cameraCtrl.player.getCenterPosition());
            var ballViewport = cameraCtrl.cam.WorldToViewportPoint(cameraCtrl.ball.transform.position);
            var playerBallDistance = Vector2.Distance(playerViewport, ballViewport);

            var yRatio = 0f;
            if (checkMinDistance && playerBallDistance < cameraCtrl.config.fallDistance)
            {
                yRatio = playerBallDistance / cameraCtrl.config.fallDistance;
                yVelocity = cameraCtrl.config.fallVelocity;
                yVelocityConstB = cameraCtrl.config.fallConstB;
            }
            else
            {
                Vector3 targetViewport;
                var moveType = getMoveType(playerViewport, ballViewport, out targetViewport);
                if (moveType == MoveType.None)
                {
                    yRatio = 1;
                }
                else
                {
                    var distance1 = calculateWorldDistance(getRectMin(moveType, targetViewport), viewportCenter);
                    var distance2 = calculateWorldDistance(converViewport(targetViewport), viewportCenter);
                    if (distance1 == 0 || distance2 == 0)
                    {
                        yRatio = 1;
                    }
                    else
                    {
                        yRatio = distance2 / distance1;
                        if (moveType == MoveType.Fall)
                        {
                            yVelocity = cameraCtrl.config.fallVelocity;
                            yVelocityConstB = cameraCtrl.config.fallConstB;

                        }
                        else
                        {
                            yVelocity = cameraCtrl.config.riseVelocity;
                            yVelocityConstB = cameraCtrl.config.riseConstB;
                        }
                    }
                }
            }
            return clampY(currentY * yRatio);
        }

        #endregion

        #region private methods

        MoveType getMoveType(Vector3 viewport1, Vector3 viewport2, out Vector3 resultViewport)
        {
            var arena1 = getAreaType(viewport1);
            var arena2 = getAreaType(viewport2);

            var moveType = MoveType.None;
            resultViewport = Vector3.zero;
            switch (arena1)
            {
                case AreaType.OutSide:
                    moveType = MoveType.Rise;
                    switch (arena2)
                    {
                        case AreaType.OutSide:
                            resultViewport = filterOutViewport(viewport1, viewport2);
                            break;
                        case AreaType.Ring:
                        case AreaType.In:
                        case AreaType.Corner:
                            resultViewport = viewport1;
                            break;
                    }
                    break;
                case AreaType.Ring:
                    switch (arena2)
                    {
                        case AreaType.OutSide:
                        case AreaType.Corner:
                            moveType = MoveType.Rise;
                            resultViewport = viewport2;
                            break;
                        case AreaType.Ring: break;
                        case AreaType.In: break;
                    }
                    break;
                case AreaType.In:
                    switch (arena2)
                    {
                        case AreaType.OutSide:
                        case AreaType.Corner:
                            moveType = MoveType.Rise;
                            resultViewport = viewport2;
                            break;
                        case AreaType.Ring: break;
                        case AreaType.In:
                            moveType = MoveType.Fall;
                            resultViewport = filterInViewport(viewport1, viewport2);
                            break;
                    }
                    break;
                case AreaType.Corner:
                    switch (arena2)
                    {
                        case AreaType.OutSide:
                            moveType = MoveType.Rise;
                            resultViewport = viewport2;
                            break;
                        case AreaType.Ring:
                        case AreaType.In:
                            moveType = MoveType.Rise;
                            resultViewport = viewport1;
                            break;
                        case AreaType.Corner:
                            moveType = MoveType.Rise;
                            resultViewport = filterCornerViewport(viewport1, viewport2);
                            break;
                    }
                    break;
            }
            return moveType;
        }

        AreaType getAreaType(Vector2 viewport)
        {
            AreaType area;
            if (cameraCtrl.config.inRect.Contains(viewport))
            {
                area = AreaType.In;
            }
            else if (cameraCtrl.config.outRect.Contains(viewport))
            {
                area = AreaType.Ring;
            }
            else
            {
                area = AreaType.OutSide;
            }
            if (area != AreaType.OutSide && isInCorner(viewport))
            {
                area = AreaType.Corner;
            }
            return area;
        }

        Vector3 filterInViewport(Vector3 viewport1, Vector3 viewport2)
        {
            var rect = cameraCtrl.config.inRect;
            var length1 = getMinLengthWithRectEdge(viewport1, rect);
            var length2 = getMinLengthWithRectEdge(viewport2, rect);
            return length1 < length2 ? viewport1 : viewport2;
        }

        Vector3 filterOutViewport(Vector3 viewport1, Vector3 viewport2)
        {
            var rect = cameraCtrl.config.inRect;
            var length1 = getMinLengthWithRectEdge(viewport1, rect);
            var length2 = getMinLengthWithRectEdge(viewport2, rect);
            return length1 > length2 ? viewport1 : viewport2;
        }

        Vector3 filterCornerViewport(Vector3 viewport1, Vector3 viewport2)
        {
            var min1 = calcaulteCornerRectMin(viewport1);
            var min2 = calcaulteCornerRectMin(viewport2);
            return min1.x < min2.x ? min1 : min2;
        }

        float getMinLengthWithRectEdge(Vector3 point, Rect rect)
        {
            var ratio = 0f;
            if (point.x <= point.y)
            {
                if (point.x <= 1 - point.y)
                {
                    ratio = (point.x - rect.xMin) / (rect.center.x - point.x);
                }
                else
                {
                    ratio = (point.y - rect.yMax) / (rect.center.y - point.y);
                }
            }
            else
            {
                if (point.x < 1 - point.y)
                {
                    ratio = (point.y - rect.yMin) / (rect.center.y - point.y);
                }
                else
                {
                    ratio = (point.x - rect.xMax) / (rect.center.x - point.x);
                }
            }
            return Mathf.Abs(ratio) * Vector2.Distance(point, rect.center);
        }

        /// <summary>
        /// 转换 Viewport
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        Vector3 converViewport(Vector3 viewport)
        {
            var x = 0f;
            if (viewport.x <= viewport.y)
            {
                if (viewport.x <= 1 - viewport.y)
                {
                    x = viewport.x;
                }
                else
                {
                    x = 1 - viewport.y;
                }
            }
            else
            {
                if (viewport.x < 1 - viewport.y)
                {
                    x = viewport.y;
                }
                else
                {
                    x = 1 - viewport.x;
                }
            }
            return new Vector3 { x = x, y = x };
        }

        /// <summary>
        /// 计算世界中的距离
        /// </summary>
        /// <param name="viewport1"></param>
        /// <param name="viewport2"></param>
        /// <returns></returns>
        float calculateWorldDistance(Vector2 viewport1, Vector2 viewport2)
        {
            Vector3 position1;
            if (!rayViewport2WorldPoint(viewport1, out position1))
            {
                Debug.Log("position1 没有交点:" + viewport1);
                return 0;
            }

            Vector3 position2;
            if (!rayViewport2WorldPoint(viewport2, out position2))
            {
                Debug.Log("viewport2 没有交点:" + viewport2);
                return 0;
            }
            return Vector3.Distance(position1, position2);
        }

        /// <summary>
        /// 获取区域的最小顶点
        /// </summary>
        /// <param name="type"></param>
        /// <param name="viewport"></param>
        /// <returns></returns>
        Vector2 getRectMin(MoveType type, Vector3 viewport)
        {
            var min = Vector2.zero;
            switch (type)
            {
                case MoveType.Fall:
                    min = cameraCtrl.config.inRect.min;
                    break;
                case MoveType.Rise:
                    var min1 = cameraCtrl.config.outRect.min;
                    if (isInCorner(viewport))
                    {
                        var min2 = calcaulteCornerRectMin(viewport);
                        if (min1.x < min2.x)
                        {
                            min = min2;
                        }
                        else
                        {
                            min = min1;
                        }
                    }
                    else
                    {
                        min = min1;
                    }
                    break;
            }
            return min;
        }

        /// <summary>
        /// 计算通过角落交点构造的区域的最小顶点
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        Vector2 calcaulteCornerRectMin(Vector3 viewport)
        {
            Vector2 begin1;
            Vector2 end1;
            if (viewport.x < 0.5)
            {
                begin1 = cameraCtrl.config.leftBottomBegin;
                end1 = cameraCtrl.config.leftBottomEnd;
            }
            else
            {
                begin1 = cameraCtrl.config.rightBottomBegin;
                end1 = cameraCtrl.config.rightBottomEnd;
            }
            var intersection = calculateIntersection(begin1, end1, viewport, viewportCenter);
            return converViewport(intersection);
        }

        /// <summary>
        /// 是否在角落里
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        bool isInCorner(Vector2 viewport)
        {
            if (viewport.x < 0.5f)
            {
                var intersection = calculateIntersection(
                 cameraCtrl.config.leftBottomBegin,
                 cameraCtrl.config.leftBottomEnd,
                 viewport,
                 new Vector2 { x = viewport.x + 1, y = viewport.y });
                return intersection.x >= viewport.x;

            }
            else
            {
                var intersection = calculateIntersection(
                       cameraCtrl.config.rightBottomBegin,
                       cameraCtrl.config.rightBottomEnd,
                       viewport,
                       new Vector2 { x = viewport.x + 1, y = viewport.y });
                return intersection.x <= viewport.x;
            }
        }

        #endregion
    }
}