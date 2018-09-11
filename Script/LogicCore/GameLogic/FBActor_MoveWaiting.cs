using FixMath.NET;

public partial class FBActor {

    //缓冲状态
    class MoveWaitingState : State
    {
        public static readonly State instance = new MoveWaitingState();

        public override bool canBreak(FBActor actor, State state)
        {
            return false;
        }

        public override void enter(FBActor actor)
        {
            actor.updateMovingState();

            actor.m_timer = actor.m_configuration.moveStateWaitingTime;
        }
        public override void update(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.setToMovementState();
                //actor.m_nextState = Movement.instance;
            }
        }

    }
}