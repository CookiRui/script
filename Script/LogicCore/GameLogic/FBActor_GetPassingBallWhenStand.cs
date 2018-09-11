
using FixMath.NET;
using BW31.SP2D;

public partial class FBActor 
{
    class GetPassingBallWhenStand : State 
    {
        public static readonly State instance = new GetPassingBallWhenStand();

        public override bool canBreak(FBActor actor, State state) 
        {
            return false;
        }

        public override void enter(FBActor actor)
        {
            //Debuger.Log("GetPassingBallWhenStand enter" + actor.world.world.frameCount);

            actor.m_particle.velocity = FixVector2.kZero;
            actor.stop();
            actor.m_timer = actor.m_stateValue;

            actor.world.onSlowGetPassingBallReady(actor);
        }

        public override void update(FBActor actor, Fix64 deltaTime) 
        {
            if (actor.world.ball.owner != null )
            {
                //Debuger.Log("GetPassingBallWhenStand over except：" + actor.world.world.frameCount);
                actor.setToMovementState();
                return;
            }

            if (Movement.checkCatchingBall(actor))
            {
                //Debuger.Log("GetPassingBallWhenStand over headCathing：" + actor.world.world.frameCount);
                return;
            }

            actor.m_timer -= deltaTime;

            if (actor.m_timer <= Fix64.Zero)
            {
                //Debuger.Log("GetPassingBallWhenStand time over no cathing??????：" + actor.world.world.frameCount);

                actor.setToMovementState();

                return;
            }

            processRotation(actor, actor.m_stateVector, deltaTime * actor.m_configuration.m1_angularSpeed);
        }

         public override void leave(FBActor actor) 
         {
             //Debuger.Log("GetPassingBallWhenStand leave" + actor.world.world.frameCount);
         }
    }

}