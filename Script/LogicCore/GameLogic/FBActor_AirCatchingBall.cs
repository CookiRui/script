
using FixMath.NET;
using BW31.SP2D;

public partial class FBActor {

    class AirCatchingBall : State {

        public static readonly State instance = new AirCatchingBall();

        enum SubState {
            kBeforeCatching,
            kAfterCatching,
        }

        public override bool canBreak(FBActor actor, State state) {
            return false;
        }

        public override void enter(FBActor actor) {
            actor.m_stateBool = false;
            actor.m_particle.velocity = FixVector2.kZero;

            actor.m_stateSubState = (int)SubState.kBeforeCatching;
            actor.m_timer = actor.m_stateValue;

            actor.world.onActorAirCatchingBallBegin(actor);
        }

        public override void update(FBActor actor, Fix64 deltaTime) {
            switch ((SubState)actor.m_stateSubState) {
                case SubState.kBeforeCatching:
                    _beforeCatching(actor, deltaTime);
                    break;

                case SubState.kAfterCatching:
                    _afterCatching(actor, deltaTime);
                    break;
            }
        }

        void _beforeCatching(FBActor actor, Fix64 deltaTime) {
            if (actor.world.ball.owner != null || !actor.world.ball.willBeCatched) {
                actor.setToMovementState();
                //actor.m_nextState = Movement.instance;
                return;
            }

            actor.m_timer -= deltaTime;

            if (actor.m_timer <= actor.configuration.scb_catchingAniTime[actor.m_stateDataIndex] && !actor.m_stateBool)
            {
                actor.m_stateBool = true;
                actor.world.onActorAirCatchingBall(actor, actor.m_stateDataIndex);
            }

            if (actor.m_timer <= Fix64.Zero) {
                actor.m_direction = actor.m_stateVector;
                actor.world.ball.transferTarget = actor;

                actor.m_stateSubState = (int)SubState.kAfterCatching;
                actor.m_timer = actor.m_configuration.scb_lockTimeAfterCatching[actor.m_stateDataIndex];

                return;
            }

            var cos = FixVector2.dot(actor.m_stateVector, actor.m_direction);
            var sin = FixVector2.cross(actor.m_stateVector, actor.m_direction);
            var angle = Fix64.Atan2(sin, cos) * actor.m_timer / actor.m_stateValue;
            cos = Fix64.Cos(angle);
            sin = Fix64.Sin(angle);
            actor.m_direction.x = FixVector2.dot(actor.m_stateVector, new FixVector2(cos, -sin));
            actor.m_direction.y = FixVector2.dot(actor.m_stateVector, new FixVector2(sin, cos));
        }

        void _afterCatching(FBActor actor, Fix64 deltaTime) {
            actor.m_timer -= deltaTime;
            if (actor.m_timer < Fix64.Zero) {
                actor.m_nextState = MoveWaitingState.instance;
            }
        }

        public override void leave(FBActor actor) {

        }
    }

}