using System;
using FixMath.NET;
using BW31.SP2D;

public enum Location
{
    kNormal = 0,
    kLeftDoor,
    kRightDoor
}

public struct EstimateHeightInfo
{
    public Fix64 destHeight;
    public int landedTimes;
};

public abstract class BallParticle : Particle
{

    public Location location = Location.kNormal;
    public event Action<BallParticle> onLocationChanged;
    public abstract Fix64 height { get; }

    public void setCollidedEventFlag() { m_collidedEventFlag = true; }
    public void clearCollidedEventFlag() { m_collidedEventFlag = false; }

    protected bool m_collidedEventFlag = false;

    public abstract Fix64 calculateHeight(Fix64 time, out int landedCount);

    // 根据当前位置 和 球场大小，改变球的Location
    public void checkLocation(Fix64 mainExtentX, Fix64 doorExtentY)
    {
        if (position.y > doorExtentY || position.y < -doorExtentY)
        {
            changeLocation(Location.kNormal);
        }
        else if (position.x >= mainExtentX)
        {
            changeLocation(Location.kRightDoor);
        }
        else if (position.x <= -mainExtentX)
        {
            changeLocation(Location.kLeftDoor);
        }
        else
        {
            changeLocation(Location.kNormal);
        }
    }

    public void changeLocation(Location loc)
    {
        if (location != loc)
        {
            location = loc;
            if (onLocationChanged != null)
            {
                onLocationChanged.Invoke(this);
            }
        }
    }

    public virtual void reset(FixVector2 position, Location location)
    {
        this.position = position;
        this.location = location;
        velocity = FixVector2.kZero;
        m_collidedEventFlag = false;
    }

    public override void updateMovement(Fix64 deltaTime) {
        var oldVelocity = velocity;
        var oldPosition = position;
        base.updateMovement(deltaTime);
        deltaPosition += (velocity - oldVelocity) * deltaTime * (Fix64)0.5f;
        position = oldPosition + deltaPosition;
    }

    public static void calculateInitialVelocity(FixVector2 start, FixVector2 target, Fix64 arrivedSpeed, Fix64 dampingAcceleration, out FixVector2 normalizedInitialVelocity, out Fix64 initialSpeed)
    {
        if (arrivedSpeed < Fix64.Zero)
        {
            arrivedSpeed = Fix64.Zero;
        }
        var d = target - start;
        var s = d.length;
        if (s == Fix64.Zero)
        {
            initialSpeed = arrivedSpeed;
            normalizedInitialVelocity = FixVector2.kZero;
        }
        else
        {
            initialSpeed = Fix64.Sqrt(arrivedSpeed * arrivedSpeed + dampingAcceleration * s * (Fix64)2);
            normalizedInitialVelocity = d / s;
        }
    }

    public static FixVector2 calculateInitialVelocityByTime(FixVector2 start, FixVector2 target, Fix64 dampingAceleration, Fix64 time)
    {
        if (time <= Fix64.Zero)
        {
            return FixVector2.kZero;
        }
        var d = target - start;
        var s = d.length;
        if (s == Fix64.Zero)
        {
            return FixVector2.kZero;
        }
        return d * (Fix64.One / time + dampingAceleration * (Fix64)0.5f * time / s);
    }

    public static bool calculateTime(FixVector2 start, FixVector2 target, Fix64 initialSpeed, Fix64 dampingAcceleration, out Fix64 time)
    {
        if (initialSpeed < Fix64.Zero)
        {
            initialSpeed = Fix64.Zero;
        }
        var d = target - start;
        var s = d.length;
        var squareSpeed = s * -dampingAcceleration * (Fix64)2 + initialSpeed * initialSpeed;
        if (squareSpeed < Fix64.Zero)
        {
            time = Fix64.Zero;
            return false;
        }
        else if (squareSpeed == Fix64.Zero)
        {
            time = initialSpeed / dampingAcceleration;
        }
        else
        {
            time = (Fix64.Sqrt(squareSpeed) - initialSpeed) / -dampingAcceleration;
        }
        return true;
    }

    public static bool calculateTime(FixVector2 start, FixVector2 target, Fix64 initialSpeed, Fix64 dampingAcceleration, Fix64 angle, out Fix64 time) {
        if (angle == Fix64.Zero) {
            return calculateTime(start, target, initialSpeed, dampingAcceleration, out time);
        }

        if (initialSpeed < Fix64.Zero) {
            initialSpeed = Fix64.Zero;
        }

        var d = (target - start).length;
        var angleAbs = Fix64.FastAbs(angle);
        var s = d / Fix64.Sin(angleAbs) * angleAbs;
        var squareSpeed = s * -dampingAcceleration * (Fix64)2 + initialSpeed * initialSpeed;
        if (squareSpeed < Fix64.Zero) {
            time = Fix64.Zero;
            return false;
        }
        else if (squareSpeed == Fix64.Zero) {
            time = initialSpeed / dampingAcceleration;
        }
        else {
            time = (Fix64.Sqrt(squareSpeed) - initialSpeed) / -dampingAcceleration;
        }
        return true;
    }

    public static Fix64 calculateHeightInitialVelocity(Fix64 initialHeight, Fix64 finalHeight, Fix64 gravity, Fix64 time)
    {
        if (time <= Fix64.Zero)
        {
            return Fix64.Zero;
        }
        var s = finalHeight - initialHeight;
        return s / time + gravity * time * (Fix64)0.5f;
    }
}

//public class LandBallParticle : BallParticle
//{
//    public override Fix64 height { get { return radius; } }
//    public override Fix64 calculateHeight(Fix64 time, out int landedCount) { landedCount = 0; return radius; }
//}

//public class AirBallParticle : BallParticle
//{

//    public Fix64 landingDamping = Fix64.One;
//    public Fix64 dampingAcceleration_land = Fix64.Zero;
//    public Fix64 dampingAcceleration_air = Fix64.Zero;
//    public event Action<int> onLanded;

//    public Fix64 ceilHeight = (Fix64)1024;
//    public Fix64 ceilDumping = Fix64.Zero;

//    public int willLandedCount = 0;

//    int landedTimes;

//    public override Fix64 height
//    {
//        get { return m_height; }
//    }

//    public override void reset(FixVector2 position, Location location)
//    {
//        base.reset(position, location);
//        m_height = radius;
//        m_heightVelocity = Fix64.Zero;
//        m_landing = true;
//    }

//    public void fly(Fix64 initialHeight, Fix64 initialHeightVelocity, Fix64 gravity)
//    {
//        if (gravity <= Fix64.Zero)
//        {
//            return;
//        }
//        if (initialHeight < radius) {
//            initialHeight = radius;
//        }
//        m_height = initialHeight;
//        m_heightVelocity = initialHeightVelocity;
//        m_heightAcceleration = -gravity;
//        if (m_height == radius && _calcMaxHeight() <= World.kEpsilon)
//        {
//            m_landing = true;
//        }
//        else
//        {
//            m_landing = false;
//        }
//        landedTimes = 0;
//    }

//    public override void updateMovement(Fix64 deltaTime)
//    {
//        dampingAcceleration = m_landing ? dampingAcceleration_land : dampingAcceleration_air;
//        base.updateMovement(deltaTime);

//        if (!m_landing)
//        {
//            if (m_heightBeforeAdjust != null) {
//                m_height = (Fix64)m_heightBeforeAdjust;
//                m_heightBeforeAdjust = null;
//            }
//            m_height += m_heightVelocity * deltaTime + m_heightAcceleration * deltaTime * deltaTime * (Fix64)0.5f;
//            if (m_height - radius <= Fix64.Zero && m_heightVelocity < Fix64.Zero)
//            {
//                ++landedTimes;
//                if (willLandedCount > 0) {
//                    --willLandedCount;
//                }
//                if (!onLanded.isNull())
//                {
//                    onLanded(landedTimes);
//                }
//                m_height = radius;
//                m_heightVelocity = landingDamping * -m_heightVelocity;
//                setCollidedEventFlag();
//                if (_calcMaxHeight() <= (Fix64)0.2f)
//                {
//                    m_landing = true;
//                }
//                else
//                {
//                    m_heightVelocity += m_heightAcceleration * deltaTime;
//                }
//            }
//            else
//            {
//                if (location != Location.kNormal) {

//                    Fix64 h = ceilHeight - radius;

//                    if (m_height >= h && m_heightVelocity > Fix64.Zero) {
//                        setCollidedEventFlag();
//                        m_height = h;
//                        m_heightVelocity = ceilDumping * -m_heightVelocity;
                        
//                    }

//                }

//                m_heightVelocity += m_heightAcceleration * deltaTime;
//            }
//        }
//    }

//    public override Fix64 calculateHeight(Fix64 time, out int landedCount) {
//        landedCount = 0;
//        if (m_landing) {
//            return radius;
//        }
//        var lastH = m_height;
//        var lastV = m_heightVelocity;
//        Fix64 retval = Fix64.Zero;
//        for (;;) {
//            retval = lastH + lastV * time + m_heightAcceleration * (Fix64)(0.5f) * time * time;
//            if (retval >= radius) {
//                break;
//            }
//            var vt = lastV * lastV + m_heightAcceleration * (radius - lastH) * (Fix64)2;
//            if (vt <= Fix64.Zero) {
//                vt = Fix64.Zero;
//            }
//            else {
//                vt = -Fix64.Sqrt(vt);
//            }
//            var t = (vt - lastV) / m_heightAcceleration;
//            time -= t;
//            if (time <= Fix64.Zero) {
//                return radius;
//            }
//            lastH = radius;
//            lastV = -vt * landingDamping;
//            ++landedCount;
//        }
//        return retval;
//    }
     
//    public void adjustHeight(Fix64 target, Fix64 percent) {
//        if (m_landing) {
//            return;
//        }
//        if (percent >= Fix64.One) {
//            m_heightBeforeAdjust = m_height;
//            m_height = target;
//            return;
//        }
//        if (percent <= Fix64.Zero) {
//            return;
//        }
//        m_heightBeforeAdjust = m_height;
//        m_height = m_height * (Fix64.One - percent) + target * percent;
//        if (m_height < radius) {
//            m_height = radius;
//        }
//    }

//    Fix64 _calcMaxHeight()
//    {
//        var t = -m_heightVelocity / m_heightAcceleration;
//        return m_heightVelocity * t + m_heightAcceleration * (Fix64)(0.5f) * t * t;
//    }

//    Fix64 m_height = Fix64.Zero;
//    Fix64? m_heightBeforeAdjust = null;
//    protected bool m_landing = true;
//    Fix64 m_heightVelocity = Fix64.Zero;
//    Fix64 m_heightAcceleration = Fix64.Zero;
//}

//public class BandBallParticle : AirBallParticle {

//    public Fix64 angularVelocity = Fix64.Zero;

//    public override void reset(FixVector2 position, Location location) {
//        base.reset(position, location);
//        angularVelocity = Fix64.Zero;
//    }

//    public override void updateMovement(Fix64 deltaTime) {
//        var oldVelocity = velocity;
//        var oldPosition = position;
//        base.updateMovement(deltaTime);
//        if (m_collidedEventFlag) {
//            angularVelocity = Fix64.Zero;
//            m_collidedEventFlag = false;
//            return;
//        }
//        if (m_landing) {
//            return;
//        }
//        var deltaAngle = angularVelocity * deltaTime;
//        if (deltaAngle == Fix64.Zero) {
//            return;
//        }
//        var radius = deltaPosition.length / Fix64.FastAbs(deltaAngle);
//        if (radius == Fix64.Zero) {
//            return;
//        }
//        var oldDirection = oldVelocity.normalized;
//        var cos = Fix64.Cos(deltaAngle);
//        var sin = Fix64.Sin(deltaAngle);
//        var rot1 = new FixVector2(cos, -sin);
//        var rot2 = new FixVector2(sin, cos);
//        var newDirection = new FixVector2() {
//            x = FixVector2.dot(oldDirection, rot1),
//            y = FixVector2.dot(oldDirection, rot2)
//        };

//        FixVector2 center;
//        if (deltaAngle > Fix64.Zero) {
//            center = oldPosition + new FixVector2(-oldDirection.y * radius, oldDirection.x * radius);
//        }
//        else {
//            center = oldPosition + new FixVector2(oldDirection.y * radius, -oldDirection.x * radius);
//        }

//        var localPos = oldPosition - center;

//        position = center + new FixVector2() {
//            x = FixVector2.dot(localPos, rot1),
//            y = FixVector2.dot(localPos, rot2)
//        };
//        deltaPosition = position - oldPosition;
//        velocity = newDirection * velocity.length;

//    }

//}

public class NewBallParticle : Particle {

    public NewBallParticle() {
        landing = true;
    }

    public Location location = Location.kNormal;
    private Fix64 _height = Fix64.Zero;
    public Fix64 height 
    {
        get{ return _height; }
        set
        { 
            _height = value;
        }
    }
    public Fix64 gravity { get { return -m_heightAcceleration; } set { m_heightAcceleration = -Fix64.FastAbs(value); } }
    public bool landing { get; private set; }
    public Fix64 linearDamping_land = Fix64.Zero;
    public Fix64 linearDamping_air = Fix64.Zero;
    public Fix64 landHitDamping = Fix64.One;
    public Fix64 ceilHitDamping = Fix64.One;
    public Fix64 ceilHeight = Fix64.MaxValue;

    public Fix64 angularVelocity = Fix64.Zero;
    public Fix64 angularDamping = Fix64.Zero;

    public Action<int,Fix64> onLanded;

    public void reset(FixVector2 position, Fix64 height) {
        location = Location.kNormal;
        this.position = position;
        if (height <= radius) {
            landing = true;
            this.height = radius;
        }
        else {
            this.height = height;
            landing = false;
        }
        this.velocity = FixVector2.kZero;
        this.m_heightVelocity = Fix64.Zero;
        m_movementMode = MM_Basic.instance;
        m_landedTimes = 0;
    }

    public void notify_catched() {
        m_movementMode = null;
        m_landedTimes = 0;
    }

    public void notify_collided() {
        if (m_movementMode != MM_Basic.instance) {
            m_movementMode = MM_Basic.instance;
        }
        // TODO:
        angularVelocity = Fix64.Zero;
    }

    public void notify_kickout(Fix64 height, Fix64 heightVelocity) {
        m_movementMode = MM_Basic.instance;
        m_heightVelocity = heightVelocity;
        if (height <= radius) {
            this.height = radius;
            landing = heightVelocity == Fix64.Zero;
        }
        else {
            this.height = height;
            landing = false;
        }


    }

    public void notify_landBall(FixVector2 start, FixVector2 target, Fix64 time, Fix64 dampingAcceleration) {
        m_movementMode = MM_LandBall.create(this, start, target, time, dampingAcceleration);
        if (m_movementMode == null) {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }

    public void notify_airBall(FixVector2 startP, Fix64 startH, FixVector2 targetP, Fix64 targetH, Fix64 time, Fix64 dampingAcceleration) {
        m_movementMode = MM_AirBall.create(this, startP, startH, targetP, targetH, time, dampingAcceleration);
        if (m_movementMode == null) {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }

    public void notify_airLandBall(FixVector2 startP, Fix64 startH, FixVector2 targetP, Fix64 airTime, Fix64 landTime, Fix64 airDampingAcceleration, Fix64 landDampingAcceleration, Fix64 landHitDamping) {
        m_movementMode = MM_AirLandBall.create(this, startP, startH, targetP, airTime, landTime, airDampingAcceleration, landDampingAcceleration, landHitDamping);
        if (m_movementMode == null) {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }

    public void notify_bandBall(FixVector2 startP, Fix64 startH, FixVector2 targetP, Fix64 targetH, Fix64 time, Fix64 dampingAcceleration, Fix64 angle) {
        m_movementMode = MM_BandBall.create(this, startP, startH, targetP, targetH, time, dampingAcceleration, angle);
        if (m_movementMode == null) {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }

    public void notify_willCatchInLand(FixVector2 target, Fix64 time) {
        if (landing) {
            m_movementMode = MM_LandCatch.create(this, target, time);
        }
        else {
            m_movementMode = MM_AirCatch.create(this, target, time, radius);
        }
        if (m_movementMode == null) {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }

    public void notify_willCatchInAir(FixVector2 target, Fix64 time, Fix64 targetH) {
        m_movementMode = MM_AirCatch.create(this, target, time, targetH);
        if (m_movementMode == null) {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }

    public void notify_willFreeCatch(FixVector2 target, Fix64 time, Fix64 targetH, Fix64 dampingAcceleration)
    {
        m_movementMode = MM_CatchFreeBall.create(this, target, targetH, time, dampingAcceleration);
        if (m_movementMode == null)
        {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }



    public void notify_willBeFreezed() {
        m_movementMode = MM_Freeze.create(this);
        if (m_movementMode == null) {
            // TODO:

            m_movementMode = MM_Basic.instance;
        }
    }

    public void notify_collideWith(FixVector2 normal, Fix64 restitution) {
        var cs = FixVector2.dot(velocity, normal);
        if (cs >= Fix64.Zero) {
            return;
        }
        var ds = (-cs * restitution) - cs;
        velocity += normal * ds;
        notify_collided();
    }

    public EstimateHeightInfo estimateHeight(Fix64 time)
    {
        EstimateHeightInfo info = new EstimateHeightInfo();
        info.landedTimes = 0;

        if (landing) {
            info.destHeight = radius;
            return info;
        }
        var lastH = height;
        var lastV = m_heightVelocity;
        Fix64 retval = Fix64.Zero;
        for (;;) {
            retval = lastH + lastV * time + m_heightAcceleration * (Fix64)(0.5f) * time * time;
            if (retval >= radius) {
                break;
            }
            var vt = lastV * lastV + m_heightAcceleration * (radius - lastH) * (Fix64)2;
            if (vt < Fix64.Zero) {
                vt = Fix64.Zero;
            }
            else {
                vt = -Fix64.Sqrt(vt);
            }
            var t = (vt - lastV) / m_heightAcceleration;
            if (t == Fix64.Zero)
            {
                info.destHeight = lastH;
                return info;
            }
            time -= t;
            if (time <= Fix64.Zero) {
                info.destHeight = radius;
                return info;
            }
            ++info.landedTimes;
            lastH = radius;
            lastV = -vt * landHitDamping;
        }

        info.destHeight = retval;
        return info;
    }

    public bool estimateHit(ref FixVector2 edgeN, ref Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height) {
        if (m_movementMode == null) {
            time = Fix64.Zero;
            point = FixVector2.kZero;
            height = Fix64.Zero;
            return false;
        }
        return m_movementMode.estimateHit(this, ref edgeN, ref edgeD, out time, out point, out height);
    }
    public bool estimate(FixVector2 target, out Fix64 time, out Fix64 dampingAcceleration )
    {
        if (m_movementMode == null)
        {
            time = Fix64.Zero;
            dampingAcceleration = Fix64.Zero;
            return false;
        }
        return m_movementMode.estimate(this, target, out time, out dampingAcceleration);
    }

    public override void updateMovement(Fix64 deltaTime) {
        if (m_movementMode != null) {
            m_movementMode.update(this, deltaTime);
        }
    }

    Fix64 m_heightVelocity;
    public Fix64 heightVelocity { get { return m_heightVelocity; } }
    Fix64 m_heightAcceleration;
    int m_landedTimes = 0;

    void _onLandHit(Fix64 preHeightVelocity) {
        ++m_landedTimes;
        if (onLanded != null) {
            onLanded(m_landedTimes, preHeightVelocity);
        }
    }

    void _onCeilHit() {

    }

    void _processHeight(Fix64 deltaTime) {
        if (landing) {
            height = radius;
        }
        else {
            height += m_heightVelocity * deltaTime;

            if (height <= radius && m_heightVelocity < Fix64.Zero) {
                // landed
                height = radius;
                var preHeightVelocity = m_heightVelocity;
                m_heightVelocity = -m_heightVelocity * landHitDamping;

                var t = m_heightVelocity / gravity;
                var maxHeight = m_heightVelocity * t + m_heightAcceleration * t.square * (Fix64)0.5f;
                if (maxHeight <= (Fix64)0.2f) {
                    landing = true;
                    m_heightVelocity = Fix64.Zero;
                    angularVelocity = Fix64.Zero;
                }
                _onLandHit(preHeightVelocity);
            }
            else if (location != Location.kNormal) {
                var ceil = ceilHeight - radius;
                if (height >= ceil && m_heightVelocity > Fix64.Zero) {
                    // ceil hit.
                    height = ceil;
                    m_heightVelocity = -m_heightVelocity * ceilHitDamping;
                    _onCeilHit();
                }
                else {
                    m_heightVelocity += m_heightAcceleration * deltaTime;
                }
            }
            else {
                m_heightVelocity += m_heightAcceleration * deltaTime;
            }
        }
    }

    abstract class MovementMode {
        public abstract void update(NewBallParticle particle, Fix64 deltaTime);
        public virtual bool estimateHit(NewBallParticle particle, ref FixVector2 edgeN, ref Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height) {
            time = Fix64.Zero;
            height = Fix64.Zero;
            point = FixVector2.kZero;
            return false;
        }

        public virtual bool estimate(NewBallParticle particle, FixVector2 target, out Fix64 time, out Fix64 dampingAcceleration )
        {
            time = Fix64.Zero;
            dampingAcceleration = Fix64.Zero;
            return false;
        }

    }

    MovementMode m_movementMode = null;

    class MM_Basic : MovementMode {
        public static readonly MovementMode instance = new MM_Basic();

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            var direction = FixVector2.kZero;
            var speed = Fix64.Zero;
            var lastPos = particle.position;
            if (particle.velocity != FixVector2.kZero) {
                speed = particle.velocity.length;
                if (speed <= World.kEpsilon) {
                    speed = Fix64.Zero;
                    particle.deltaPosition = FixVector2.kZero;
                    particle.velocity = FixVector2.kZero;
                }
                else {
                    particle.deltaPosition = particle.velocity * deltaTime;
                    particle.position += particle.deltaPosition;
                    direction = particle.velocity / speed;
                    speed *= Fix64.One / (Fix64.One + deltaTime * (particle.landing ? particle.linearDamping_land : particle.linearDamping_air));
                    if (speed <= World.kEpsilon) {
                        particle.velocity = FixVector2.kZero;
                        speed = Fix64.Zero;
                    }
                    else {
                        particle.velocity = direction * speed;
                    }
                }
            }
            else {
                particle.deltaPosition = FixVector2.kZero;
            }

            particle._processHeight(deltaTime);

            if (!particle.landing) { 

                if (particle.angularVelocity != Fix64.Zero) {
                    var deltaAngle = particle.angularVelocity * deltaTime;
                    particle.angularVelocity *= Fix64.One / (Fix64.One + deltaTime * particle.angularDamping);
                    if (Fix64.FastAbs(particle.angularVelocity) <= World.kEpsilon) {
                        particle.angularVelocity = Fix64.Zero;
                    }
                    var deltaAngleAbs = Fix64.FastAbs(deltaAngle);
                    if (deltaAngleAbs > World.kEpsilon) {
                        var deltaLength = particle.deltaPosition.length;
                        if (deltaLength > World.kEpsilon) {
                            var bandRadius = deltaLength / deltaAngleAbs;
                            var cos = Fix64.Cos(deltaAngle);
                            var sin = Fix64.Sin(deltaAngle);
                            var rot1 = new FixVector2(cos, -sin);
                            var rot2 = new FixVector2(sin, cos);
                            var newDirection = new FixVector2() {
                                x = FixVector2.dot(direction, rot1),
                                y = FixVector2.dot(direction, rot2)
                            };

                            FixVector2 center;
                            if (deltaAngle > Fix64.Zero) {
                                center = lastPos + new FixVector2(-direction.y * bandRadius, direction.x * bandRadius);
                            }
                            else {
                                center = lastPos + new FixVector2(direction.y * bandRadius, -direction.x * bandRadius);
                            }

                            var localPos = lastPos - center;
                            particle.position = center + new FixVector2() {
                                x = FixVector2.dot(localPos, rot1),
                                y = FixVector2.dot(localPos, rot2)
                            };
                            particle.deltaPosition = particle.position - lastPos;
                            particle.velocity = newDirection * speed;
                        }
                    }
                }
            }
        }

        public override bool estimate(NewBallParticle particle, FixVector2 target, out Fix64 time, out Fix64 dampingAcceleration )
        {
            time = Fix64.Zero;
            dampingAcceleration = Fix64.Zero;

            Fix64 v1 = particle.velocity.length;
            if (v1 == Fix64.Zero)
            {
                return false;
            }
            Fix64 deltaTime = particle.deltaPosition.length / v1;
            Fix64 v2 = particle.velocity.length * Fix64.One / (Fix64.One + deltaTime * (particle.landing ? particle.linearDamping_land : particle.linearDamping_air));

            if (deltaTime == Fix64.Zero)
                return false;

            dampingAcceleration = (v2 - v1) / deltaTime;
            Fix64 s = (target - particle.position).length;
            Fix64 vt2 = v1.square + (Fix64)2.0f * s * dampingAcceleration;
            //球不能运动到交点
            if (vt2 <= Fix64.Zero)
            {
                return false;
            }
            Fix64 vt = Fix64.Sqrt(vt2);
            time = (vt - v1) / dampingAcceleration;

            return true;

        }

    }

    class MM_LandBall : MovementMode {

        public static MovementMode create(NewBallParticle particle, FixVector2 start, FixVector2 target, Fix64 time, Fix64 dampingAcceleration) {
            var d = target - start;
            var s = d.length;
            if (s <= World.kEpsilon || time <= Fix64.Zero) {
                particle.position = target;
                particle.height = particle.radius;
                particle.landing = true;
                return null;
            }
            var dv = dampingAcceleration * time;
            var v0 = s / time + dv * (Fix64)0.5f;
            var vt = v0 - dv;
            var di = d / s;
            var ret = new MM_LandBall();
            ret.m_startP = start;
            ret.m_targetP = target;
            ret.m_startV = di * v0;
            ret.m_targetV = di * vt;
            ret.m_accelection = di * -dampingAcceleration;
            ret.m_time = time;

            particle.position = start;
            particle.height = particle.radius;
            particle.landing = true;
            particle.velocity = ret.m_startV;

            return ret;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            m_timer += deltaTime;
            if (m_timer >= m_time) {
                particle.deltaPosition = m_targetP - particle.position;
                particle.position = m_targetP;
                particle.velocity = m_targetV;
                particle.height = particle.radius;
                particle.landing = true;
                particle.m_movementMode = MM_Basic.instance;
                return;
            }

            particle.velocity = m_startV + m_timer * m_accelection;
            var pos = m_startP + m_startV * m_timer + m_accelection * m_timer.square * (Fix64)0.5f;
            particle.deltaPosition = pos - particle.position;
            particle.position = pos;
            particle.height = particle.radius;
        }

        public override bool estimateHit(NewBallParticle particle, ref FixVector2 edgeN, ref Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height) {
            height = particle.radius;
            var d = m_targetP - m_startP;
            var t = -((FixVector2.dot(m_startP, edgeN) + edgeD) / FixVector2.dot(d, edgeN));
            if (t < Fix64.Zero || t > Fix64.One) {
                time = Fix64.Zero;
                point = FixVector2.kZero;
                return false;
            }

            var local = d * t;
            var s = local.length;

            point = m_startP + local;

            var v0_2 = m_startV.squareLength;
            var a = -m_accelection.length;
            var vt = Fix64.Sqrt(v0_2 + (Fix64)2 * a * s);

            time = (vt - Fix64.Sqrt(v0_2)) / a;
            if (time < m_timer) {
                return false;
            }
            return true;
        }

        Fix64 m_time;
        Fix64 m_timer;
        FixVector2 m_startP, m_targetP;
        FixVector2 m_startV, m_targetV;
        FixVector2 m_accelection;
    }

    class MM_AirBall : MovementMode {

        public static MovementMode create(NewBallParticle particle, FixVector2 startP, Fix64 startH, FixVector2 targetP, Fix64 targetH, Fix64 time, Fix64 dampingAcceleration) {
            if (startH < particle.radius) {
                startH = particle.radius;
            }
            if (targetH < particle.radius) {
                targetH = particle.radius;
            }
            var d = targetP - startP;
            var s = d.length;
            if (time <= Fix64.Zero) {
                particle.position = targetP;
                particle.velocity = FixVector2.kZero;
                if (targetH == particle.radius) {
                    particle.height = particle.radius;
                    particle.landing = true;
                }
                else {
                    particle.height = targetH;
                    particle.landing = false;
                    particle.m_heightVelocity = Fix64.Zero;
                    particle.angularVelocity = Fix64.Zero;
                }
                return null;
            }

            var dv = dampingAcceleration * time;
            var v0 = s / time + dv * (Fix64)0.5f;
            var vt = v0 - dv;
            var di = d / s;

            var ret = new MM_AirBall();

            ret.m_startP = startP;
            ret.m_targetP = targetP;
            ret.m_startV = di * v0;
            ret.m_targetV = di * vt;
            ret.m_accelection = di * -dampingAcceleration;
            ret.m_time = time;

            ret.m_startH = startH;
            ret.m_targetH = targetH;

            var dhv = particle.m_heightAcceleration * time;
            ret.m_startHV = (targetH - startH) / time - dhv * (Fix64)0.5f;
            ret.m_targetHV = ret.m_startHV + dhv;

            particle.position = startP;
            particle.height = startH;
            particle.landing = false;
            particle.angularVelocity = Fix64.Zero;
            particle.velocity = ret.m_startV;
            particle.m_heightVelocity = ret.m_startHV;

            return ret;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            m_timer += deltaTime;
            if (m_timer >= m_time) {
                particle.deltaPosition = m_targetP - particle.position;
                particle.position = m_targetP;
                particle.velocity = m_targetV;
                particle.height = m_targetH;
                particle.m_heightVelocity = m_targetHV;
                particle.m_movementMode = MM_Basic.instance;
                return;
            }

            var halfSquareTime = m_timer.square * (Fix64)0.5f;

            particle.velocity = m_startV + m_timer * m_accelection;
            var pos = m_startP + m_startV * m_timer + m_accelection * halfSquareTime;
            particle.deltaPosition = pos - particle.position;
            particle.position = pos;

            particle.height = m_startH + m_startHV * m_timer + particle.m_heightAcceleration * halfSquareTime;
            particle.m_heightVelocity = m_startHV + particle.m_heightAcceleration * m_timer;
        }

        public override bool estimate(NewBallParticle particle, FixVector2 target, out Fix64 time, out Fix64 dampingAcceleration)
        {
            time = Fix64.Zero;
            dampingAcceleration = Fix64.Zero;

            Fix64 a = -m_accelection.length;

            Fix64 vt2 = particle.velocity.squareLength + (Fix64)2 * a * (target - particle.position).length;
            if (vt2 <= Fix64.Zero)
                return false;

            Fix64 vt = Fix64.Sqrt(vt2);
            time = (vt - particle.velocity.length) / a;
            dampingAcceleration = a;
            return true;
        }


        public override bool estimateHit(NewBallParticle particle, ref FixVector2 edgeN, ref Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height) {
            height = particle.height;
            var d = m_targetP - m_startP;
            var t = -((FixVector2.dot(m_startP, edgeN) + edgeD) / FixVector2.dot(d, edgeN));
            if (t < Fix64.Zero || t > Fix64.One) {
                Debuger.Log("Time not in zero-one zone:" + (float)t);
                time = Fix64.Zero;
                point = FixVector2.kZero;
                return false;
            }

            var local = d * t;
            var s = local.length;

            point = m_startP + local;

            var v0_2 = m_startV.squareLength;
            var a = -m_accelection.length;
            var vt = Fix64.Sqrt(v0_2 + (Fix64)2 * a * s);

            time = (vt - Fix64.Sqrt(v0_2)) / a;
            if (time < m_timer)
            {
                Debuger.Log("Time < m_timer" + (float)time + " <" + (float)m_timer);
                                return false;
            }

            height = m_startH + m_startHV * time + particle.m_heightAcceleration * time.square * (Fix64)0.5f;

            return true;
        }

        Fix64 m_time;
        Fix64 m_timer;
        FixVector2 m_startP, m_targetP;
        FixVector2 m_startV, m_targetV;
        Fix64 m_startH, m_targetH;
        Fix64 m_startHV, m_targetHV;
        FixVector2 m_accelection;
    }

    class MM_AirLandBall : MovementMode {
        public static MovementMode create(NewBallParticle particle, FixVector2 startP, Fix64 startH, FixVector2 targetP, Fix64 airTime, Fix64 landTime, Fix64 airDampingAcceleration, Fix64 landDampingAcceleration, Fix64 landHitDamping) {
            if (startH < particle.radius) {
                startH = particle.radius;
            }
            var ret = new MM_AirLandBall();

            ret.m_startP = startP;
            ret.m_startH = startH;
            ret.m_targetP = targetP;
            ret.m_time1 = airTime;
            ret.m_time2 = landTime;

            var d = targetP - startP;
            var s = d.length;
            var di = d / s;
            var v0 = (s + (Fix64)0.5f * (airDampingAcceleration * airTime.square + landDampingAcceleration * landTime.square) + airDampingAcceleration * airTime * landHitDamping * landTime) / (airTime + landHitDamping * landTime);

            ret.m_v0 = di * v0;

            ret.m_a0 = di * -airDampingAcceleration;

            ret.m_v1 = di * ((v0 - airDampingAcceleration * airTime) * landHitDamping);
            ret.m_a1 = di * -landDampingAcceleration;

            ret.m_startHV = (particle.radius - startH) / airTime - particle.m_heightAcceleration * airTime * (Fix64)0.5f;

            particle.height = startH;
            particle.position = startP;
            particle.landing = false;
            particle.angularVelocity = Fix64.Zero;
            particle.m_heightVelocity = ret.m_startHV;

            return ret;
        }

        public override bool estimate(NewBallParticle particle, FixVector2 target, out Fix64 time, out Fix64 dampingAcceleration)
        {
            time = Fix64.Zero;
            dampingAcceleration = Fix64.Zero;

            FixVector2 startPosition = particle.position;
            Fix64 a = -m_a0.length;
            Fix64 totalTime = Fix64.Zero;
            FixVector2 hitPoint = m_startP + m_v1 * m_time1 + m_a1 * m_time1.square * (Fix64)0.5f;

            if (particle.landing)
            {
                a = -m_a1.length;
            }
            else
            {
                //经过弹跳
                if ((target - m_startP).squareLength > (hitPoint - m_startP).squareLength)
                {
                    startPosition = hitPoint;
                    a = -m_a1.length;
                    totalTime = m_time1 - m_timer;
                }
            }


            Fix64 vt2 = particle.velocity.squareLength + (Fix64)2 * a * (target - startPosition).length;
            if (vt2 <= Fix64.Zero)
                return false;

            Fix64 vt = Fix64.Sqrt(vt2);
            totalTime += ((vt - particle.velocity.length) / a);
            dampingAcceleration = a;
            time = totalTime;

            return true;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            FixVector2 oldPos = particle.position;
            do {
                if (particle.landing) 
                {
                    m_timer += deltaTime;
                    if (m_timer >= m_time2) 
                    {
                        particle.position = m_targetP;
                        particle.velocity = m_v1 + m_a1 * m_time2;
                        particle.m_movementMode = MM_Basic.instance;
                    }
                    else 
                    {
                        particle.position = m_startP + m_v1 * m_timer + m_a1 * m_timer.square * (Fix64)0.5f;
                        particle.velocity = m_v1 + m_a1 * m_timer;
                    }
                    break;
                }
                
                if (m_timer + deltaTime > m_time1) {
                    deltaTime -= m_time1 - m_timer;
                    m_timer = m_time1;
                }
                else
                {
                    m_timer += deltaTime;
                    deltaTime = Fix64.Zero;
                }
                var halfSquareTime = m_timer.square * (Fix64)0.5f;
                particle.position = m_startP + m_v0 * m_timer + m_a0 * halfSquareTime;
                particle.velocity = m_v0 + m_a0 * m_timer;
                particle.m_heightVelocity = m_startHV + particle.m_heightAcceleration * m_timer;
                particle.height = m_startH + m_startHV * m_timer + particle.m_heightAcceleration * halfSquareTime;

                if (m_timer == m_time1) 
                {
                    m_startP = particle.position;
                    m_timer = Fix64.Zero;
                    particle.landing = true;
                    particle.height = particle.radius;
                    var preHeightVelocity = particle.m_heightVelocity;
                    particle.m_heightVelocity = Fix64.Zero;
                    particle._onLandHit(preHeightVelocity);
                }
            } while (deltaTime > Fix64.Zero);
            particle.deltaPosition = particle.position - oldPos;
        }


        FixVector2 m_startP;
        FixVector2 m_targetP;
        Fix64 m_startH;
        Fix64 m_startHV;
        Fix64 m_time1;
        Fix64 m_time2;
        Fix64 m_timer = Fix64.Zero;
        FixVector2 m_v0;
        FixVector2 m_a0;
        FixVector2 m_v1;
        FixVector2 m_a1;
    }

    class MM_BandBall : MovementMode {
        public static MovementMode create(NewBallParticle particle, FixVector2 startP, Fix64 startH, FixVector2 targetP, Fix64 targetH, Fix64 time, Fix64 dampingAcceleration, Fix64 angle) {
            var angleAbs = Fix64.FastAbs(angle);
            if (angleAbs <= World.kEpsilon) {
                return MM_AirBall.create(particle, startP, startH, targetP, targetH, time, dampingAcceleration);
            }

            if (startH < particle.radius) {
                startH = particle.radius;
            }
            if (targetH < particle.radius) {
                targetH = particle.radius;
            }
            var d = targetP - startP;
            var s = d.length;
            if (s <= World.kEpsilon || time <= Fix64.Zero) {
                particle.position = targetP;
                if (targetH == particle.radius) {
                    particle.height = particle.radius;
                    particle.landing = true;
                }
                else {
                    particle.height = targetH;
                    particle.landing = false;
                    particle.m_heightVelocity = Fix64.Zero;
                    particle.angularVelocity = Fix64.Zero;
                }
                return null;
            }

            var di = d / s;

            var ret = new MM_BandBall();

            ret.m_targetP = targetP;
            ret.m_time = time;
            ret.m_a = -dampingAcceleration;

            ret.m_radius = s * (Fix64)0.5f / Fix64.Sin(angleAbs);
            ret.m_center = (startP + targetP) * (Fix64)0.5f;
            if (angle > Fix64.Zero) {
                ret.m_rotFlag = false;
                ret.m_center += new FixVector2(di.y, -di.x) * (Fix64.Cos(angleAbs) * ret.m_radius);
            }
            else {
                ret.m_rotFlag = true;
                ret.m_center += new FixVector2(-di.y, di.x) * (Fix64.Cos(angleAbs) * ret.m_radius);
            }

            ret.m_maxAngle = angleAbs * (Fix64)2;
            var arcLength = ret.m_maxAngle * ret.m_radius;
            ret.m_v0 = arcLength / time + dampingAcceleration * time * (Fix64)0.5f;

            ret.m_startLocalEdge = startP - ret.m_center;

            var dhv = particle.m_heightAcceleration * time;
            ret.m_startHV = (targetH - startH) / time - dhv * (Fix64)0.5f;
            ret.m_targetHV = ret.m_startHV + dhv;
            ret.m_targetH = targetH;
            ret.m_startH = startH;

            particle.position = startP;
            particle.height = startH;
            particle.landing = false;
            particle.angularVelocity = -angle / time;
            particle.m_heightVelocity = ret.m_startHV;

            var vi = (ret.m_rotFlag ? new FixVector2(-ret.m_startLocalEdge.y, ret.m_startLocalEdge.x) : new FixVector2(ret.m_startLocalEdge.y, -ret.m_startLocalEdge.x)) / ret.m_radius;
            particle.velocity = vi * ret.m_v0;

            return ret;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            m_timer += deltaTime;
            FixVector2 vi;
            if (m_timer >= m_time) {
                particle.deltaPosition = m_targetP - particle.position;
                particle.position = m_targetP;

                var vt = m_v0 + m_a * m_time;
                var tartetLocalEdge = (m_targetP - m_center);
                vi = new FixVector2(tartetLocalEdge.y, -tartetLocalEdge.x) / m_radius;
                particle.velocity = vi * vt;

                particle.height = m_targetH;
                particle.m_heightVelocity = m_targetHV;
                particle.landing = false;
                particle.m_movementMode = MM_Basic.instance;

                return;
            }

            var lastPos = particle.position;
            var halfSquareTime = m_timer.square * (Fix64)0.5f;
            var s = m_v0 * m_timer + m_a * halfSquareTime;
            var v = m_v0 + m_a * m_timer;
            m_currentAngle = (m_rotFlag ? s : -s) / m_radius;
            var cos = Fix64.Cos(m_currentAngle);
            var sin = Fix64.Sin(m_currentAngle);
            var localEdge = new FixVector2() {
                x = FixVector2.dot(m_startLocalEdge, new FixVector2(cos, -sin)),
                y = FixVector2.dot(m_startLocalEdge, new FixVector2(sin, cos))
            };
            particle.position = m_center + localEdge;
            particle.deltaPosition = particle.position - lastPos;

            vi = (m_rotFlag ? new FixVector2(-localEdge.y, localEdge.x) : new FixVector2(localEdge.y, -localEdge.x)) / m_radius;
            particle.velocity = vi * v;

            particle.height = m_startH + m_startHV * m_timer + particle.m_heightAcceleration * halfSquareTime;
            particle.m_heightVelocity = m_startHV + particle.m_heightAcceleration * m_timer;
        }

        public override bool estimateHit(NewBallParticle particle, ref FixVector2 edgeN, ref Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height) {
            var rs = edgeN * -edgeD;
            var rd = new FixVector2(-edgeN.y, edgeN.x);
            var rs_c = rs - m_center;
            var fa = rd.squareLength;
            var fb = FixVector2.dot(rs_c, rd) * (Fix64)2;
            var fc = rs_c.squareLength - m_radius.square;
            var fd = fb.square - (Fix64)4 * fa * fc;
            if (fd < Fix64.Zero) {
                time = Fix64.Zero;
                point = FixVector2.kZero;
                height = Fix64.Zero;
                return false;
            }

            Fix64 angle;
            if (fd == Fix64.Zero) {
                var rt = -fb / ((Fix64)2 * fa);
                point = rs + rd * rt;
                var localPoint = point - m_center;
                var ncos = FixVector2.dot(localPoint, m_startLocalEdge);
                var nsin = m_rotFlag ? FixVector2.cross(m_startLocalEdge, localPoint) : FixVector2.cross(localPoint, m_startLocalEdge);
                angle = Fix64.Atan2(nsin, ncos);
                if (angle < Fix64.Zero) {
                    angle += Fix64.PiTimes2;
                }
            }
            else {
                fd = Fix64.Sqrt(fd);
                var fa2 = (Fix64)2 * fa;
                var rt1 = (-fb + fd) / fa2;
                var rt2 = (-fb - fd) / fa2;

                var pt1 = rs + rd * rt1;
                var localPt1 = pt1 - m_center;
                var ncos = FixVector2.dot(localPt1, m_startLocalEdge);
                var nsin = m_rotFlag ? FixVector2.cross(m_startLocalEdge, localPt1) : FixVector2.cross(localPt1, m_startLocalEdge);
                var a1 = Fix64.Atan2(nsin, ncos);
                if (a1 < Fix64.Zero) {
                    a1 += Fix64.PiTimes2;
                }
                var pt2 = rs + rd * rt2;
                var localPt2 = pt2 - m_center;
                ncos = FixVector2.dot(localPt2, m_startLocalEdge);
                nsin = m_rotFlag ? FixVector2.cross(m_startLocalEdge, localPt2) : FixVector2.cross(localPt2, m_startLocalEdge);
                var a2 = Fix64.Atan2(nsin, ncos);
                if (a2 < Fix64.Zero) {
                    a2 += Fix64.PiTimes2;
                }
                if (a1 > a2) {
                    var at = a1; a1 = a2; a2 = at;
                    var ptt = pt1; pt1 = pt2; pt2 = ptt;
                }
                if (a1 < m_currentAngle) {
                    angle = a2;
                    point = pt2;
                }
                else {
                    angle = a1;
                    point = pt1;
                }
            }

            if (angle < m_currentAngle || angle > m_maxAngle) {
                time = Fix64.Zero;
                height = Fix64.Zero;
                return false;
            }

            var arcLength = angle * m_radius;
            var vt = Fix64.Sqrt(m_v0.square + (Fix64)2 * m_a * arcLength);

            time = (vt - m_v0) / m_a;
            height = m_startH + m_startHV * time + particle.m_heightAcceleration * time.square * (Fix64)0.5f;

            return true;
        }

        Fix64 m_time;
        Fix64 m_timer;
        FixVector2 m_targetP;
        Fix64 m_startH, m_targetH;
        Fix64 m_startHV, m_targetHV;
        Fix64 m_v0, m_a;
        FixVector2 m_center;
        Fix64 m_radius;
        FixVector2 m_startLocalEdge;
        bool m_rotFlag;

        Fix64 m_currentAngle;
        Fix64 m_maxAngle;
    }

    class MM_LandCatch : MovementMode {

        public static MovementMode create(NewBallParticle particle, FixVector2 target, Fix64 time) {
            var d = target - particle.position;
            var s = d.length;
            if (s <= World.kEpsilon || time <= Fix64.Zero) {
                particle.position = target;
                particle.height = particle.radius;
                particle.landing = true;
                return null;
            }

            var vi = d / s;

            var ret = new MM_LandCatch();
            ret.m_start = particle.position;
            ret.m_target = target;
            ret.m_time = time;

            particle.velocity = vi * (s / time);
            particle.height = particle.radius;
            particle.landing = true;

            return ret;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            m_timer += deltaTime;
            if (m_timer >= m_time) {
                particle.deltaPosition = m_target - particle.position;
                particle.position = m_target;
                particle.m_movementMode = MM_Basic.instance;
                return;
            }
            var f = m_timer / m_time;
            var pos = m_start * (Fix64.One - f) + m_target * f;
            particle.deltaPosition = pos - particle.position;
            particle.position = pos;
        }

        FixVector2 m_start, m_target;
        Fix64 m_time, m_timer;
    }

    class MM_AirCatch : MovementMode {

        public static MovementMode create(NewBallParticle particle, FixVector2 targetP, Fix64 time, Fix64 targetH) {
            if (targetH < particle.radius) {
                targetH = particle.radius;
            }
            if (time <= Fix64.Zero) {
                particle.position = targetP;
                particle.height = targetH;
                return null;
            }

            var d = targetP - particle.position;
            var s = d.length;
            var vi = s <= World.kEpsilon ? FixVector2.kZero : d / s;

            var ret = new MM_AirCatch();
            ret.m_start = particle.position;
            ret.m_target = targetP;
            ret.m_time = time;
            ret.m_height = particle.height;
            ret.m_targetH = targetH;

            particle.velocity = vi * (s / time);
            particle.angularVelocity = Fix64.Zero;

            return ret;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            m_timer += deltaTime;
            if (m_timer >= m_time) {
                particle.deltaPosition = m_target - particle.position;
                particle.position = m_target;
                particle.height = m_targetH;
                particle.m_movementMode = MM_Basic.instance;
                return;
            }
            var f = m_timer / m_time;
            var inv = Fix64.One - f;
            var pos = m_start * inv + m_target * f;
            particle.deltaPosition = pos - particle.position;
            particle.position = pos;

            particle.height = m_height;
            particle._processHeight(deltaTime);
            m_height = particle.height;

            particle.height = m_height * inv + m_targetH * f;
        }

        FixVector2 m_start, m_target;
        Fix64 m_time, m_timer;
        Fix64 m_targetH;
        Fix64 m_height;
    }



    class MM_CatchFreeBall : MovementMode
    {
        public static MovementMode create(NewBallParticle particle, FixVector2 targetP, Fix64 targetH, Fix64 time, Fix64 dampingAcceleration)
        {
            FixVector2 startP = particle.position;
            Fix64 startH = particle.height;
            Fix64 startHV = particle.m_heightVelocity;

            if (startH < particle.radius)
            {
                startH = particle.radius;
            }
            if (targetH < particle.radius)
            {
                targetH = particle.radius;
            }
            var d = targetP - startP;
            var s = d.length;
            if (time <= Fix64.Zero)
            {
                particle.position = targetP;
                particle.velocity = FixVector2.kZero;
                if (targetH == particle.radius)
                {
                    particle.height = particle.radius;
                    particle.landing = true;
                }
                else
                {
                    particle.height = targetH;
                    particle.landing = false;
                    particle.m_heightVelocity = startHV;
                    particle.angularVelocity = Fix64.Zero;
                }
                return null;
            }

            var dv = dampingAcceleration * time;
            var v0 = s / time + dv * (Fix64)0.5f;
            var vt = v0 - dv;
            var di = d / s;

            var ret = new MM_CatchFreeBall();

            ret.m_startP = startP;
            ret.m_targetP = targetP;
            ret.m_startV = di * v0;
            ret.m_targetV = di * vt;
            ret.m_accelection = di * -dampingAcceleration;
            ret.m_time = time;

            ret.m_startH = startH;
            ret.m_targetH = targetH;


            ret.m_startHV = startHV;
            ret.m_acclectionH = ((targetH - startH) - startHV * time) * (Fix64)2 / time.square; ;
            var dhv = ret.m_acclectionH * time;
            ret.m_targetHV = ret.m_startHV + dhv;

            particle.position = startP;
            particle.height = startH;
            if ( startH > particle.radius )
                particle.landing = false;
            particle.angularVelocity = Fix64.Zero;
            particle.velocity = ret.m_startV;

            return ret;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime)
        {
            m_timer += deltaTime;
            if (m_timer >= m_time)
            {
                particle.deltaPosition = m_targetP - particle.position;
                particle.position = m_targetP;
                particle.velocity = m_targetV;
                particle.height = m_targetH;
                particle.m_heightVelocity = m_targetHV;
                particle.m_movementMode = MM_Basic.instance;
                return;
            }

            var halfSquareTime = m_timer.square * (Fix64)0.5f;

            particle.velocity = m_startV + m_timer * m_accelection;
            var pos = m_startP + m_startV * m_timer + m_accelection * halfSquareTime;
            particle.deltaPosition = pos - particle.position;
            particle.position = pos;

            Fix64 thisH = m_startH + m_startHV * m_timer + m_acclectionH * halfSquareTime;
            if (thisH < particle.radius)
                thisH = particle.radius;
            particle.height = thisH;

            particle.m_heightVelocity = m_startHV + m_acclectionH * m_timer;
        }

        public override bool estimateHit(NewBallParticle particle, ref FixVector2 edgeN, ref Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height)
        {
            height = particle.height;
            var d = m_targetP - m_startP;
            var t = -((FixVector2.dot(m_startP, edgeN) + edgeD) / FixVector2.dot(d, edgeN));
            if (t < Fix64.Zero || t > Fix64.One)
            {
                Debuger.Log("Time not in zero-one zone:" + (float)t);
                time = Fix64.Zero;
                point = FixVector2.kZero;
                return false;
            }

            var local = d * t;
            var s = local.length;

            point = m_startP + local;

            var v0_2 = m_startV.squareLength;
            var a = -m_accelection.length;
            var vt = Fix64.Sqrt(v0_2 + (Fix64)2 * a * s);

            time = (vt - Fix64.Sqrt(v0_2)) / a;
            if (time < m_timer)
            {
                Debuger.Log("Time < m_timer" + (float)time + " <" + (float)m_timer);
                return false;
            }

            height = m_startH + m_startHV * time + m_acclectionH * time.square * (Fix64)0.5f;

            return true;
        }

        Fix64 m_time;
        Fix64 m_timer;
        FixVector2 m_startP, m_targetP;
        FixVector2 m_startV, m_targetV;
        Fix64 m_startH, m_targetH;
        Fix64 m_startHV, m_targetHV;
        FixVector2 m_accelection;
        Fix64 m_acclectionH;
    }



    class MM_Freeze : MovementMode {

        public static MovementMode create(NewBallParticle particle) {
            if (particle.m_movementMode == null || particle.m_movementMode == MM_Basic.instance) {
                return null;
            }
            var ret = new MM_Freeze();
            ret.m_original = particle.m_movementMode;
            return ret;
        }

        public override void update(NewBallParticle particle, Fix64 deltaTime) {
            m_original.update(particle, deltaTime);
            if (particle.m_movementMode != this) {
                particle.m_movementMode = this;
            }
        }

        public override bool estimateHit(NewBallParticle particle, ref FixVector2 edgeN, ref Fix64 edgeD, out Fix64 time, out FixVector2 point, out Fix64 height) {
            return m_original.estimateHit(particle, ref edgeN, ref edgeD, out time, out point, out height);
        }

        MovementMode m_original;
    }
}
