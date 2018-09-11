using FixMath.NET;

public partial class FBActor {

    //无球被铲
    class BeSlid_NoBall : State {
        public static readonly State instance = new BeSlid_NoBall();

        public override bool canBreak(FBActor actor, State state) {
            return false;
        }

        public override void enter(FBActor actor) 
        {
            actor.m_timer = actor.m_configuration.bst3_waitingTime;
            actor.world.onBeSliding(actor);
        }

        public override void update(FBActor actor, Fix64 deltaTime) {
            actor.m_particle.velocity = actor.m_direction * actor.m_configuration.bst3_speed;
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero) {
                actor.m_nextState = MoveWaitingState.instance;
            }
        }
    }
}