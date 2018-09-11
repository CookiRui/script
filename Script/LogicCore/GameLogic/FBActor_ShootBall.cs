using FixMath.NET;
using BW31.SP2D;

public partial class FBActor
{
    class ShootBall : State
    {
        public static readonly State instance = new ShootBall();

        enum SubState
        {
            kPrepareShooting,
            kBeforeShooting,
            kAfterShooting
        }

        public override bool canBreak(FBActor actor, State state)
        {
            if (!actor.shootBallEvent && state == BeAttacked.instance)
                return true;
            return false;
        }

        public override void enter(FBActor actor)
        {
            actor.m_stateSubState = (int)SubState.kPrepareShooting;
        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            switch ((SubState)actor.m_stateSubState)
            {
                case SubState.kPrepareShooting:
                    _prepareShooting(actor, deltaTime);
                    break;
                case SubState.kBeforeShooting:
                    _beforeShooting(actor, deltaTime);
                    break;
                case SubState.kAfterShooting:
                    _afterShooting(actor, deltaTime);
                    break;
            }
        }

        public override void leave(FBActor actor)
        {
            base.leave(actor);
            actor.shootBallEvent = false;
            actor._shootBallPressed = false;
            actor._shootBallTimeSum = Fix64.Zero;
        }

        void _prepareShooting(FBActor actor, Fix64 deltaTime)
        {
            if (actor.shootBallEvent)
            {
                actor.m_stateSubState = (int)SubState.kBeforeShooting;
                actor.m_timer = actor.m_configuration.sb_beforeShootingTime[actor.m_stateDataIndex];
                var cos = FixVector2.dot(actor.m_direction, actor.m_stateVector);
                if (cos < Fix64.One)
                {
                    var sin = Fix64.Sqrt(Fix64.One - cos * cos);
                    if (FixVector2.cross(actor.m_direction, actor.m_stateVector) >= Fix64.Zero)
                    {
                        sin = -sin;
                    }
                    actor.m_stateValue = Fix64.Atan2(sin, cos);
                }
                else
                {
                    actor.m_stateValue = Fix64.Zero;
                }

                if (actor.m_stateDataIndex == (int)(ShootType.Killer))
                    actor.world.fbGame.logicTimeScale = (Fix64)0.01f;

                //
                actor.world.onShootBallReady(actor);

                return;
            }

            actor.m_particle.dampingAcceleration = actor.m_configuration.m1_stopDampingAcceleration_ball;

            if (actor.m_direction != actor.m_stateVector)
            {
                var cos = FixVector2.dot(actor.m_direction, actor.m_stateVector);
                var maxAngle = actor.m_configuration.sb_angularSpeed * deltaTime;
                var cos_max = Fix64.Cos(maxAngle);
                var speed = actor.m_particle.velocity.length;
                if (cos >= cos_max)
                {
                    actor.m_direction = actor.m_stateVector;
                }
                else
                {
                    var sin_max =
                        FixVector2.cross(actor.m_direction, actor.m_stateVector) >= Fix64.Zero ?
                        Fix64.Sin(maxAngle) : Fix64.Sin(-maxAngle);

                    actor.m_direction = new FixVector2()
                    {
                        x = FixVector2.dot(actor.m_direction, new FixVector2(cos_max, -sin_max)),
                        y = FixVector2.dot(actor.m_direction, new FixVector2(sin_max, cos_max))
                    };
                }
                actor.m_particle.velocity = actor.m_direction * speed;
            }
        }

        void _beforeShooting(FBActor actor, Fix64 deltaTime)
        {
            actor.m_particle.dampingAcceleration = actor.m_configuration.m1_stopDampingAcceleration_ball;
            actor.m_timer -= deltaTime;
            if (actor.m_timer < Fix64.Zero)
            {
                if (actor.m_direction != actor.m_stateVector)
                {
                    actor.m_direction = actor.m_stateVector;
                    actor.m_particle.velocity = actor.m_direction * actor.m_particle.velocity.length;
                }
                actor.m_stateSubState = (int)SubState.kAfterShooting;
                actor.m_timer = actor.m_configuration.sb_afterShootingTime[actor.m_stateDataIndex];

                if (actor.m_stateDataIndex == (int)(ShootType.Killer))
                    actor.world.fbGame.logicTimeScale = (Fix64)1.0f;

                actor.world.shootBallOutEvent = true;

                return;
            }

            if (actor.m_direction != actor.m_stateVector)
            {
                var angle = actor.m_stateValue * (actor.m_timer / actor.m_configuration.sb_beforeShootingTime[actor.m_stateDataIndex]);
                var cos = Fix64.Cos(angle);
                var sin = Fix64.Sin(angle);
                actor.m_direction.x = FixVector2.dot(actor.m_stateVector, new FixVector2(cos, -sin));
                actor.m_direction.y = FixVector2.dot(actor.m_stateVector, new FixVector2(sin, cos));
                actor.m_particle.velocity = actor.m_direction * actor.m_particle.velocity.length;
            }
        }

        void _afterShooting(FBActor actor, Fix64 deltaTime)
        {
            actor.m_particle.dampingAcceleration = actor.m_configuration.m1_stopDampingAcceleration_ball;
            actor.m_timer -= deltaTime;
            if (actor.m_timer < Fix64.Zero)
            {
                actor.shootBallEvent = false;
                actor.m_nextState = MoveWaitingState.instance;
            }
        }
    }

}