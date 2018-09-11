using BW31.SP2D;
using FixMath.NET;

public partial class FBActor {

    //被铲丢球
    class BeSlid_DropBall : State {
        public static readonly State instance = new BeSlid_DropBall();

        enum SubState {
            kDampingToZero,
            kStandWaiting,
            kWaiting
        }

        public override bool canBreak(FBActor actor, State state) {
            return false;
        }

        public override void enter(FBActor actor) {
            var speed = actor.m_particle.velocity.length;
            if (speed <= World.kEpsilon) {
                actor.m_stateSubState = (int)SubState.kStandWaiting;
                actor.m_timer = actor.m_configuration.bst1_standWaitingTime;
                actor.world.onBeSlidDropBallBegin(actor, false);
            }
            else {
                actor.m_stateSubState = (int)SubState.kDampingToZero;
                actor.m_timer = actor.m_configuration.bst1_dampingToZeroTime;
                actor.m_stateVector = actor.m_particle.velocity;
                actor.world.onBeSlidDropBallBegin(actor, true);
            }
        }

        public override void update(FBActor actor, Fix64 deltaTime) {
            switch ((SubState)actor.m_stateSubState) {
                case SubState.kDampingToZero:
                    _dampingToZero(actor, deltaTime);
                    break;

                case SubState.kStandWaiting:
                    _standWaiting(actor, deltaTime);
                    break;

                case SubState.kWaiting:
                    _waiting(actor, deltaTime);
                    break;
            }
        }

        void _dampingToZero(FBActor actor, Fix64 deltaTime) {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero) {
                actor.m_particle.velocity = FixVector2.kZero;
                actor.m_timer = actor.m_configuration.bst1_waitingTime;
                actor.m_stateSubState = (int)SubState.kWaiting;
                actor.world.onBeSlidDropBallWaiting(actor);
            }
            else {
                actor.m_particle.velocity = actor.m_stateVector * actor.m_timer;
            }
        }

        void _standWaiting(FBActor actor, Fix64 deltaTime) {
            actor.m_particle.velocity = FixVector2.kZero;
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero) {
                actor.m_timer = actor.m_configuration.bst1_waitingTime;
                actor.m_stateSubState = (int)SubState.kWaiting;
                actor.world.onBeSlidDropBallWaiting(actor);
            }
        }

        void _waiting(FBActor actor, Fix64 deltaTime) {
            actor.m_particle.velocity = FixVector2.kZero;
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero) {
                actor.m_nextState = MoveWaitingState.instance;
                actor.world.onBeSlidDropBallEnd(actor);
            }
        }

    }
}