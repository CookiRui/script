
using FixMath.NET;
using BW31.SP2D;

public partial class FBActor
{

    // 外部数据:
    // m_stateDataIndex, 传球类型索引
    // m_stateVector, 目标朝向

    class PassBall : State
    {

        public override bool canBreak(FBActor actor, State state)
        {
            if (state == ShootBall.instance)
                return false;
            return true;
        }

        public static readonly State instance = new PassBall();

        enum SubState : int
        {
            kBeforePassing,
            kAfterPassing
        }

        public override void enter(FBActor actor)
        {
            actor.m_stateSubState = (int)SubState.kBeforePassing;
            actor.m_timer = actor.m_configuration.pb_beforePassingTime[actor.m_stateDataIndex];
            var cos = FixVector2.dot(actor.m_direction, actor.m_stateVector);
            Fix64 ralativeDirection = Fix64.Zero;
            if (cos < Fix64.One)
            {
                var sin = Fix64.Sqrt(Fix64.One - cos * cos);

                ralativeDirection = FixVector2.cross(actor.m_direction, actor.m_stateVector);
                if (ralativeDirection >= Fix64.Zero)
                {
                    sin = -sin;
                }
                actor.m_stateValue = Fix64.Atan2(sin, cos);
            }
            else
            {
                actor.m_stateValue = Fix64.Zero;
            }

            int passBallFoot = actor.getKickBallFoot(actor.m_stateVector, actor.m_direction);

            actor.world.onPassBallBegin(actor, actor.m_stateDataIndex == 0, passBallFoot == 1);

        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            switch ((SubState)actor.m_stateSubState)
            {
                case SubState.kBeforePassing:
                    _beforePassing(actor, deltaTime);
                    break;

                case SubState.kAfterPassing:
                    _afterPassing(actor, deltaTime);
                    break;
            }
        }

        void _beforePassing(FBActor actor, Fix64 deltaTime)
        {
            actor.m_particle.dampingAcceleration = actor.m_configuration.m1_stopDampingAcceleration_ball;
            actor.m_timer -= deltaTime;
            if (actor.m_timer < Fix64.Zero)
            {
                actor.m_direction = actor.m_stateVector;
                actor.m_particle.velocity = actor.m_direction * actor.m_particle.velocity.length;
                actor.m_stateSubState = (int)SubState.kAfterPassing;
                actor.m_timer = actor.m_configuration.pb_afterPassingTime[actor.m_stateDataIndex];
                actor.m_world.passBallEvent = true;
                return;
            }

            var angle = actor.m_stateValue * (actor.m_timer / actor.m_configuration.pb_beforePassingTime[actor.m_stateDataIndex]);
            var cos = Fix64.Cos(angle);
            var sin = Fix64.Sin(angle);
            actor.m_direction.x = FixVector2.dot(actor.m_stateVector, new FixVector2(cos, -sin));
            actor.m_direction.y = FixVector2.dot(actor.m_stateVector, new FixVector2(sin, cos));
            actor.m_particle.velocity = actor.m_direction * actor.m_particle.velocity.length;
        }

        void _afterPassing(FBActor actor, Fix64 deltaTime)
        {
            actor.m_particle.dampingAcceleration = actor.m_configuration.m1_stopDampingAcceleration_ball;
            actor.m_timer -= deltaTime;
            if (actor.m_timer < Fix64.Zero)
            {
                actor.m_nextState = MoveWaitingState.instance;
            }
        }

    }

}