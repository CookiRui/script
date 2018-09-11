using BW31.SP2D;
using FixMath.NET;

public partial class FBActor
{

    //被攻击
    class BeAttacked : State
    {
        public static readonly State instance = new BeAttacked();

        enum SubState
        {
            kDampingToZero,
            kWaiting
        }

        public override bool canBreak(FBActor actor, State state)
        {
            return false;
        }

        public override void enter(FBActor actor)
        {
            if (actor.m_stateInt != (int)BallDetachType_Attacked.None)
            {
                FixVector2 velocity = actor.direction * (actor.particle.velocity.length + actor.configuration.ballDetachSpeedAttacked[(int)actor.m_stateInt]);
                FixVector3 position = actor.getBallPosition((BallDetachType_Attacked)actor.m_stateInt);
                actor.world.ball.freeByAttacked(position, velocity, Fix64.Zero);
                actor.world.ball.setSampleType(FBBall.SampleType.TimeSlerp, actor.configuration.ballDetachSlerpTimeAttacked[actor.m_stateInt]);
            }


            FixVector2 dir = (actor.getPosition() - actor.m_stateActor.getPosition()).normalized;
            actor.m_stateVector = dir * actor.m_stateActor.configuration.bst1_initialSpeed;

            if (actor.configuration.st_beAttackedFaceBackoff == 1)
                actor.direction = dir;
            else
                actor.direction = -dir;


            actor.m_stateSubState = (int)SubState.kDampingToZero;
            actor.m_timer = actor.m_stateActor.configuration.bst1_dampingToZeroTime;

            //持球队员被攻击
            if (actor.m_stateInt != (int)BallDetachType_Attacked.None)
            {
                actor.world.fbGame.lerpToTimeScale(
                  ConstTable.ActorAttackedSlowPlaySpeed,
                  ConstTable.ActorAttackedSlowPlayTime,
                  ConstTable.ActorAttackedResetTime,
                  () => { actor.world.onBeginHit(actor.m_stateActor.id, actor.id); },
                  () => { actor.world.onEndHit(); },
                  () => { actor.world.onHitCompleted(); }
                  );
            }

            actor.world.onBeSlidDropBallBegin(actor, true);

        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            switch ((SubState)actor.m_stateSubState)
            {
                case SubState.kDampingToZero:
                    _dampingToZero(actor, deltaTime);
                    break;

                case SubState.kWaiting:
                    _waiting(actor, deltaTime);
                    break;
            }
        }

        void _dampingToZero(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.m_particle.velocity = FixVector2.kZero;
                actor.m_timer = actor.m_configuration.bst1_waitingTime;
                actor.m_stateSubState = (int)SubState.kWaiting;
                actor.world.onBeSlidDropBallWaiting(actor);
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
                actor.world.onBeSlidDropBallEnd(actor);
            }
        }

    }
}