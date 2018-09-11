
using FixMath.NET;
using BW31.SP2D;

public partial class FBActor 
{
    class GetPassingBallWhenMoving : State 
    {
        public static readonly State instance = new GetPassingBallWhenMoving();

        public override bool canBreak(FBActor actor, State state) 
        {
            return false;
        }

        public override void enter(FBActor actor) 
        {
            actor.world.ball.willCatchPassingBall(actor);
            //Debuger.Log("QuickGetPassingBall enter：" + actor.world.world.frameCount);
            actor.m_particle.dampingAcceleration = Fix64.Zero;
            actor.m_timer = actor.m_stateValue;
        }

        public override void update(FBActor actor, Fix64 deltaTime) 
        {
            if (actor.world.ball.owner != null || !actor.world.ball.willBeCatched )
            {
                actor.setToMovementState();
                return;
            }

            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                //Debuger.Log("QuickGetPassingBall over get ball：" + actor.world.world.frameCount);
                actor.world.ball.transferTarget = actor;
                actor.setToMovementState();
                return;
            }
        }

         public override void leave(FBActor actor) 
         {

         }
    }

}