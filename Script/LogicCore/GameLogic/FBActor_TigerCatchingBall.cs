
using FixMath.NET;
using BW31.SP2D;

public partial class FBActor {

    class TigerCatchingBall : State
    {
        public static readonly State instance = new TigerCatchingBall();

        enum SubState {
            kBeforeCatching,
            kGliding,
            kAfterCatching,
        }

        public override bool canBreak(FBActor actor, State state) {
            return false;
        }

        public override void enter(FBActor actor)
        {
            FixVector2 targetPosition = actor.world.ball.particlePosition + actor.world.ball.particleVelocity * actor.configuration.tcb_normalTime;

            //Debuger.Log("TigerCatchingBall currentBallPos:"
            //    + (UnityEngine.Vector2)actor.world.ball.particlePosition
            //    + " TargetPostion " + targetPosition
            //    + " NeedTime:" + (float)actor.configuration.tcb_normalTime
            //    + " actor.world.ball.particleVelocity:" + (UnityEngine.Vector2)actor.world.ball.particleVelocity);

            actor.world.ball.willCatchBall(actor, targetPosition, actor.configuration.tcb_normalTime);

            FixVector2 targetPositionVelocity = targetPosition - actor.getPosition();
            Fix64 distance = targetPositionVelocity.length;
            targetPositionVelocity = targetPositionVelocity / distance;
            actor.m_stateVector = targetPositionVelocity * (distance / actor.configuration.tcb_normalTime);
            actor.particle.dampingAcceleration = Fix64.Zero;
            actor.direction = targetPositionVelocity;

            actor.m_stateSubState = (int)SubState.kBeforeCatching;
            actor.m_timer = actor.configuration.tcb_normalTime;

            actor.world.onActorTigerCatchingBallBegin(actor);
        }

        public override void update(FBActor actor, Fix64 deltaTime) {
            switch ((SubState)actor.m_stateSubState) {
                case SubState.kBeforeCatching:
                    _beforeCatching(actor, deltaTime);
                    break;
                case SubState.kGliding:
                    _gliding(actor, deltaTime);
                    break;
                case SubState.kAfterCatching:
                    _afterCatching(actor, deltaTime);
                    break;
            }
        }

        void _gliding(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            actor.particle.velocity = actor.m_stateVector * (actor.m_stateValue * actor.m_timer);
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.particle.velocity = FixVector2.kZero;
                actor.m_stateSubState = (int)SubState.kAfterCatching;
                actor.m_timer = actor.m_configuration.tcb_lockTimeAfterCatching;
                return;
            }
        }

        void _beforeCatching(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;

            actor.particle.velocity = actor.m_stateVector;

            if (actor.m_timer <= Fix64.Zero || actor.world.ball.owner != null || !actor.world.ball.willBeCatched )
            {
                if (actor.m_timer <= Fix64.Zero)
                {
                    actor.world.ball.transferTarget = actor;
                }

                actor.m_stateSubState = (int)SubState.kGliding;
                actor.m_timer = actor.m_configuration.tcb_glideTimeAfterCatching;
                //Damping
                actor.m_stateValue = actor.m_stateVector.length / actor.m_configuration.tcb_glideTimeAfterCatching;
                actor.m_stateVector = actor.particle.velocity.normalized;
                return;
            }


        }

        void _afterCatching(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer < Fix64.Zero) {
                actor.m_nextState = MoveWaitingState.instance;
            }
        }

        public override void leave(FBActor actor) 
        {

        }
    }

}