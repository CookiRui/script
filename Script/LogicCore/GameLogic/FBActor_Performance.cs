using BW31.SP2D;
using FixMath.NET;

public partial class FBActor
{
    class WarmUp : State
    {
        public static readonly State instance = new WarmUp();

        public override void enter(FBActor actor)
        {
            actor.world.onActorWarmUp(actor);
            actor.m_timer = actor.world.config.enterShowTime;
        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.setToMovementState();
            }
        }
    }

    class Taunt : State
    {
        public static readonly State instance = new Taunt();

        public override bool canBreak(FBActor actor, State state) { return false; }

        public override void enter(FBActor actor)
        {
            actor.world.onActorTaunt(actor);
            actor.m_timer = actor.configuration.tauntTime;
        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.m_nextState = WarmUp.instance;
            }
        }
    }

    class CheerStand : State
    {
        public static readonly State instance = new CheerStand();

        public override void enter(FBActor actor) { actor.world.onActorCheerStand(actor); }

        public override void update(FBActor actor, Fix64 deltaTime) { }
    }

    class CheerUnique : State
    {
        public static readonly State instance = new CheerUnique();

        public override bool canBreak(FBActor actor, State state) { return false; }

        public override void enter(FBActor actor)
        {
            actor.world.onActorCheerUnique(actor);
            actor.m_timer = actor.configuration.cheerUniqueTime;
        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            actor.m_timer -= deltaTime;
            if (actor.m_timer <= Fix64.Zero)
            {
                actor.m_nextState = CheerStand.instance;
            }
        }
    }

    class Dismay : State
    {
        public static readonly State instance = new Dismay();

        public override void enter(FBActor actor) { actor.world.onActorDismay(actor); }

        public override void update(FBActor actor, Fix64 deltaTime) { }
    }
}