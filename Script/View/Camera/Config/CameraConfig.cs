using System.Security;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace FBCamera
{
    public class CameraConfig
    {
        #region border
        public float topBorder;
        public float bottomBorder;
        public float leftBorder;
        public float rightBorder;
        public float yMinBorder;
        public float yMaxBorder;
        #endregion

        #region default
        public float defaultAngle;
        public Vector3 defaultPos;
        public float defaultFOV;
        #endregion

        #region distance
        public float fallDistance;
        #endregion

        #region velocity
        public float xzVelocityMin;
        public float xzVelocityMax;
        public float switchDuration;
        public float xzConstB;
        public float xzDistanceThreshold;

        public float fallVelocity;
        public float riseVelocity;
        public float fallConstB;
        public float riseConstB;
        public float yDistanceThreshold;

        #endregion

        #region charging
        public float chargingYMin;
        public float chargingAngle;
        public float chargingAngleVelocityMax;
        public float chargingDelay;
        public float chargingFallTime;
        #endregion

        #region rect
        public Rect outRect;
        public Rect inRect;
        public Vector2 leftBottomBegin;
        public Vector2 leftBottomEnd;
        public Vector2 rightBottomBegin;
        public Vector2 rightBottomEnd;
        #endregion

        #region offset
        public float attackOffset;
        public float defenceOffset;
        public float attackConstK;
        public float defenceConstK;
        #endregion

        #region replay
        public float lookAtLerpSpeed;
        public float lookAtGKDistance;
        CameraPosition[] positions;
        #endregion

        #region enter
        public Vector3 startPosition;
        public Vector3 showActorOffset;

        public float showEnemyMoveTime;
        public float enemyTauntTime;

        public Vector3 aroundOffset;
        public float aroundRatio;
        public float aroundMoveTime;
        public float aroundStayTime;

        public float selfTauntTime;
        public float showSelfMoveTime;

        #endregion

        #region goal
        public float goalBeginTime;
        public float goalMoveTime;
        public Vector3 goalBeginPosition;
        public Vector3 goalEndPosition;
        #endregion

        #region over
        public Vector3 overPosition;
        public float overMoveTime;
        #endregion

        #region hit
        public float hitYMin;
        public float hitAngle;
        public float hitInTime;
        public float hitInXZVelocity;
        public float hitOutTime;
        public float hitOutXZVelocity;
        #endregion

        public CameraConfig(SecurityElement se)
        {
            if (se.isNull())
            {
                Debuger.LogError("se is null");
                return;
            }

            var borderSE = se.SearchForChildByTag("border");
            topBorder = borderSE.parseFloat("top");
            bottomBorder = borderSE.parseFloat("bottom");
            leftBorder = borderSE.parseFloat("left");
            rightBorder = borderSE.parseFloat("right");
            yMinBorder = borderSE.parseFloat("ymin");
            yMaxBorder = borderSE.parseFloat("ymax");

            var defaultSE = se.SearchForChildByTag("default");
            defaultPos = defaultSE.parseVector3();
            defaultAngle = defaultSE.parseFloat("angle");
            defaultFOV = defaultSE.parseFloat("fov");

            var distanceSE = se.SearchForChildByTag("distance");
            fallDistance = distanceSE.parseFloat("fall");

            var velocitySE = se.SearchForChildByTag("velocity");
            var xzSE = velocitySE.SearchForChildByTag("xz");
            xzVelocityMin = xzSE.parseFloat("min");
            xzVelocityMax = xzSE.parseFloat("max");
            switchDuration = xzSE.parseFloat("duration");
            xzConstB = xzSE.parseFloat("b");
            xzDistanceThreshold = xzSE.parseFloat("distancethreshold");

            var ySE = velocitySE.SearchForChildByTag("y");
            fallVelocity = ySE.parseFloat("fall");
            riseVelocity = ySE.parseFloat("rise");
            fallConstB = ySE.parseFloat("fallb");
            riseConstB = ySE.parseFloat("riseb");
            yDistanceThreshold = ySE.parseFloat("distancethreshold");


            var chargingSE = se.SearchForChildByTag("charging");
            chargingYMin = chargingSE.parseFloat("ymin");
            chargingAngle = chargingSE.parseFloat("angle");
            chargingAngleVelocityMax = chargingSE.parseFloat("angleVelocityMax");
            chargingDelay = chargingSE.parseFloat("delay");
            chargingFallTime = chargingSE.parseFloat("time");

            var rectSE = se.SearchForChildByTag("rect");
            outRect = rectSE.SearchForChildByTag("out").parseRect();
            inRect = rectSE.SearchForChildByTag("in").parseRect();

            var leftBottomSE = rectSE.SearchForChildByTag("leftbottom");
            leftBottomBegin = new Vector2
            {
                x = leftBottomSE.parseFloat("x1"),
                y = leftBottomSE.parseFloat("y1"),
            };
            leftBottomEnd = new Vector2
            {
                x = leftBottomSE.parseFloat("x2"),
                y = leftBottomSE.parseFloat("y2"),
            };

            var rightBottomSE = rectSE.SearchForChildByTag("rightbottom");
            rightBottomBegin = new Vector2
            {
                x = rightBottomSE.parseFloat("x1"),
                y = rightBottomSE.parseFloat("y1"),
            };
            rightBottomEnd = new Vector2
            {
                x = rightBottomSE.parseFloat("x2"),
                y = rightBottomSE.parseFloat("y2"),
            };

            var offsetSE = se.SearchForChildByTag("offset");
            var attackSE = offsetSE.SearchForChildByTag("attack");
            attackOffset = attackSE.parseFloat("base");
            attackConstK = attackSE.parseFloat("k");

            var defenceSE = offsetSE.SearchForChildByTag("defence");
            defenceOffset = defenceSE.parseFloat("base");
            defenceConstK = defenceSE.parseFloat("k");

            var replaySE = se.SearchForChildByTag("replay");
            lookAtLerpSpeed = replaySE.parseFloat("lookatlerpspeed");
            lookAtGKDistance = replaySE.parseFloat("lookatgkdistance");

            var positionsSE = replaySE.SearchForChildByTag("positions");
            var positionCount = positionsSE.Children.Count;
            if (positionCount > 0)
            {
                positions = new CameraPosition[positionCount];
                for (int i = 0; i < positionCount; i++)
                {
                    var positionSE = positionsSE.Children[i] as SecurityElement;
                    var position = new CameraPosition();
                    position.id = positionSE.parseInt("id");
                    position.position = positionSE.parseVector3();
                    position.weight = positionSE.parseInt("weight");

                    foreach (SecurityElement item1 in positionSE.Children)
                    {
                        switch (item1.Tag)
                        {
                            case "action":
                                position.actions.Add(parseCameraAction(item1));
                                break;
                            case "shoot":
                                foreach (SecurityElement item2 in item1.Children)
                                {
                                    position.shootActions.Add(parseCameraAction(item2));
                                }
                                break;
                            case "goal":
                                foreach (SecurityElement item3 in item1.Children)
                                {
                                    position.goalActions.Add(parseCameraAction(item3));
                                }
                                break;
                            default: Debug.LogError("unknow tag:" + item1.Tag); break;
                        }
                    }
                    positions[i] = position;
                }
            }

            var enterSE = se.SearchForChildByTag("enter");
            var startPositionSE = enterSE.SearchForChildByTag("startposition");
            startPosition = startPositionSE.parseVector3();

            var showActorSE = enterSE.SearchForChildByTag("showactor");
            showActorOffset = showActorSE.parseVector3();

            var selfSE = showActorSE.SearchForChildByTag("self");
            selfTauntTime = selfSE.parseFloat("taunttime");
            showSelfMoveTime = selfSE.parseFloat("movetime");

            var enemySE = showActorSE.SearchForChildByTag("enemy");
            showEnemyMoveTime = enemySE.parseFloat("movetime");
            enemyTauntTime = enemySE.parseFloat("taunttime");

            var aroundSE = showActorSE.SearchForChildByTag("around");
            aroundOffset = aroundSE.parseVector3();
            aroundRatio = aroundSE.parseFloat("ratio");
            aroundMoveTime = aroundSE.parseFloat("movetime");
            aroundStayTime = aroundSE.parseFloat("staytime");

            var goalSE = se.SearchForChildByTag("goal");
            goalBeginTime = goalSE.parseFloat("begintime");
            goalMoveTime = goalSE.parseFloat("movetime");
            goalBeginPosition = goalSE.SearchForChildByTag("begin").parseVector3();
            goalEndPosition = goalSE.SearchForChildByTag("end").parseVector3();

            var overSE = se.SearchForChildByTag("over");
            overPosition = overSE.parseVector3();
            overMoveTime = overSE.parseFloat("movetime");

            var hitSE = se.SearchForChildByTag("hit");
            hitYMin = hitSE.parseFloat("ymin"); 
            hitAngle = hitSE.parseFloat("angle");
            hitInTime = hitSE.parseFloat("intime");
            hitInXZVelocity = hitSE.parseFloat("inxzvelocity");
            hitOutTime = hitSE.parseFloat("outtime");
            hitOutXZVelocity = hitSE.parseFloat("outxzvelocity");
        }

        CameraActionBase parseCameraAction(SecurityElement se)
        {
            if (se == null)
            {
                Debuger.LogError("se is null");
                return null;
            }
            CameraActionBase action = null;
            var type = (CameraActionType)se.parseInt("value");

            switch (type)
            {
                case CameraActionType.LookAt:
                    var lookAtAction = new LookAtAction();
                    lookAtAction.lookAtType = (LookAtType)se.parseInt("type");
                    action = lookAtAction;
                    break;
                case CameraActionType.FOV:
                    var fovAction = new FOVAction();
                    fovAction.target = se.parseFloat("target");
                    fovAction.beginTime = se.tryParseFloat("begintime");
                    fovAction.stayTime = se.tryParseFloat("staytime");
                    action = fovAction;
                    break;
                case CameraActionType.Move:
                    var moveAction = new MoveAction();
                    moveAction.beginTime = se.tryParseFloat("begintime");
                    moveAction.target = se.parseVector3();
                    action = moveAction;
                    break;
                default: return null;
            }

            action.type = type;
            action.time = se.tryParseFloat("time");
            action.weight = se.tryParseInt("weight");
            return action;
        }

        public CameraPosition getPosition(int id)
        {
            if (positions.isNullOrEmpty()) return null;
            return positions.SingleOrDefault(a => a.id == id);
        }

        public CameraPosition getRandomPosition(float randomValue)
        {
            if (positions.isNullOrEmpty()) return null;

            var sum = positions.Sum(a => a.weight);
            var min = 0f;
            foreach (var item in positions)
            {
                var max = min + (float)item.weight / sum;
                if (min < randomValue && randomValue <= max)
                {
                    return item;
                }
                min = max;
            }
            return null;
        }
    }
}

public static class SecurityElementExtend
{
    public static uint parseUint(this SecurityElement se, string attribute)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return 0;
        }
        if (attribute == null)
        {
            Debuger.LogError("attribute is null");
            return 0;
        }
        return uint.Parse(se.Attribute(attribute));
    }

    public static int parseInt(this SecurityElement se, string attribute)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return 0;
        }
        if (attribute == null)
        {
            Debuger.LogError("attribute is null");
            return 0;
        }
        return int.Parse(se.Attribute(attribute));
    }

    public static int? tryParseInt(this SecurityElement se, string attribute)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return null;
        }
        if (attribute == null)
        {
            Debuger.LogError("attribute is null");
            return null;
        }
        var str = se.Attribute(attribute);
        if (string.IsNullOrEmpty(str)) return null;
        return int.Parse(str);
    }

    public static float? tryParseFloat(this SecurityElement se, string attribute)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return null;
        }
        if (attribute == null)
        {
            Debuger.LogError("attribute is null");
            return null;
        }
        var str = se.Attribute(attribute);
        if (string.IsNullOrEmpty(str)) return null;
        return float.Parse(str);
    }

    public static Rect parseRect(this SecurityElement se)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return Rect.zero;
        }
        return new Rect
        {
            xMin = parseFloat(se, "xmin"),
            yMin = parseFloat(se, "ymin"),
            xMax = parseFloat(se, "xmax"),
            yMax = parseFloat(se, "ymax")
        };
    }

    public static float parseFloat(this SecurityElement se, string attribute)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return 0;
        }
        if (attribute == null)
        {
            Debuger.LogError("attribute is null");
            return 0;
        }
        return float.Parse(se.Attribute(attribute));
    }

    public static Vector2 parseVector2(this SecurityElement se)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return Vector2.zero;
        }
        return new Vector2
        {
            x = parseFloat(se, "x"),
            y = parseFloat(se, "y"),
        };
    }

    public static Vector3 parseVector3(this SecurityElement se)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return Vector3.zero;
        }
        return new Vector3
        {
            x = parseFloat(se, "x"),
            y = parseFloat(se, "y"),
            z = parseFloat(se, "z"),
        };
    }

}