
using FixMath.NET;
using BW31.SP2D;

public partial class FBActor 
{
    static void processRotation(FBActor actor, FixVector2 moveDirection, Fix64 maxAngle)
    {
        var cos = FixVector2.dot(actor.m_direction, moveDirection);
        var cos_max = Fix64.Cos(maxAngle);

        if (cos >= cos_max)
        {
            actor.m_direction = moveDirection;
        }
        else
        {
            var sin_max =
                FixVector2.cross(actor.m_direction, moveDirection) >= Fix64.Zero ?
                Fix64.Sin(maxAngle) : Fix64.Sin(-maxAngle);

            actor.m_direction = new FixVector2()
            {
                x = FixVector2.dot(actor.m_direction, new FixVector2(cos_max, -sin_max)),
                y = FixVector2.dot(actor.m_direction, new FixVector2(sin_max, cos_max))
            };
        }
    }

    class DefendMovement : State
    {
        public static readonly State instance = new DefendMovement();

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            if (Movement.checkCatchingBall(actor))
            {
                return;
            }          

            actor.updateMovingState();

            if (actor.m_stateActor == null && actor.m_stateBall == null)
            {
                actor.m_RunTimeMovementState = null;
                actor.setToMovementState();
                return;
            }

            FixVector2 targetPosition = actor.m_stateActor != null ? actor.m_stateActor.getPosition() : actor.m_stateBall.getPosition();
            //处理旋转
            var moveDirection = targetPosition - actor.getPosition();
            moveDirection.normalize();
            processRotation(actor, moveDirection, deltaTime * actor.m_configuration.m1_angularSpeed);

            // 如果不移动，则减速
            if (actor.m_movePower == Fix64.Zero) {
                actor.m_particle.dampingAcceleration =
                    actor.world.ball.owner != actor ?
                    actor.m_configuration.m1_stopDampingAcceleration :
                    actor.m_configuration.m1_stopDampingAcceleration_ball;
                return;
            }

            //处理位移
            actor.m_particle.dampingAcceleration = Fix64.Zero;
            actor.defendMoveDirection = actor.getMoveDirection();
            actor.m_particle.maxSpeed = actor.m_configuration.dm1_maxSpeed[(int)actor.defendMoveDirection-1];
            actor.m_particle.addForce(actor.moveDirection * (actor.m_configuration.m1_moveForce * actor.m_movePower));
        }

        public override void enter(FBActor actor)
        {
            actor.m_RunTimeMovementState = instance;
            actor.updateMovingState();
        }
        public override void leave(FBActor actor)
        {
            actor._setMovingState(MovingState.kAction);

            actor.defendMoveDirection = DefendMoveDirection.None;
        }

    }
}