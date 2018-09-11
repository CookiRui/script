
using BW31.SP2D;
using FixMath.NET;

public partial class FBActor {

    //铲球
    class Sliding : State
    {
        public static readonly State instance = new Sliding();

        enum SubState
        {
            kSliding,
            kWaiting
        }

        public override bool canBreak(FBActor actor, State state)
        {
            return false;
        }

        public override void enter(FBActor actor)
        {
            actor.ignoreCollision = true;
            actor.m_stateSubState = (int)SubState.kSliding;
            //if (actor.m_movePower != Fix64.Zero) {
            //    actor.m_stateVector = actor.m_moveDirection * actor.m_configuration.st_initialSpeed;
            //}
            //else {
            actor.m_stateVector = actor.m_direction * actor.m_configuration.st_initialSpeed;
            //}
            actor.m_timer = actor.m_configuration.st_dampingToZeroTime;
            actor.sliding = true;
            actor.world.onSlidingBegin(actor);
        }

        public override void leave(FBActor actor)
        {
        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            switch ((SubState)actor.m_stateSubState)
            {
                case SubState.kSliding:
                    _sliding(actor, deltaTime);
                    break;

                case SubState.kWaiting:
                    _waiting(actor, deltaTime);
                    break;
            }
        }

        void _sliding(FBActor actor, Fix64 deltaTime)
        {
            actor._updateDirectionByVelocity();
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.m_particle.velocity = FixVector2.kZero;
                actor.m_stateSubState = (int)SubState.kWaiting;
                actor.m_timer = actor.m_configuration.st_waitingTime;
                actor.ignoreCollision = false;
                actor.sliding = false;
                //actor.world.onSlidingWaiting(actor, actor.m_timer);
                actor.m_slidingTargets.Clear();
            }
            else
            {
                actor.m_particle.velocity = actor.m_stateVector * actor.m_timer;
            }
        }

        void _waiting(FBActor actor, Fix64 deltaTime)
        {
            actor.m_particle.velocity = FixVector2.kZero;
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.m_nextState = MoveWaitingState.instance;
            }
        }

    }
}