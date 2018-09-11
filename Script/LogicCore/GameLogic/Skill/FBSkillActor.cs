using BW31.SP2D;
using FixMath.NET;
using System.Collections.Generic;

using ML.SkillEdit.Runtime;

namespace Skill
{
    public class FBSkillActor : IActor2, System.IEquatable<FBSkillActor>
    {
        public FBSkillActor(FBActor actor)
        {
            this.actor = actor;
        }
        public FBActor actor { get; set; }

        public override int GetHashCode() {
            return actor.GetHashCode();
        }

        public override bool Equals(object obj) {
            return actor.Equals(obj);
        }

        public bool Equals(FBSkillActor other) {
            return actor == other.actor;
        }

        public IVector2 position()
        {
            return (TVector2)actor.getPosition();
        }

        public IVector2 direction()
        {
            return (TVector2)actor.direction;
        }

        public void setActorIntValue(IString name, IInt value)
        {
            actor.world.onActorAction(actor, name.value, value.value);
        }
        public void setActorTrigger(IString name)
        {
            actor.world.onActorAction(actor, name.value);
        }
        public void setActorFloatValue(IString name, IFloat value)
        {
            actor.world.onActorAction(actor, name.value, value.value);
        }

        public IActorMovement2 createMovement()
        {
            return actor.createMovement();          
        }

        public IActorLock2 createLock()
        {
            return actor.createLock();
        }

        public IActorCollisionDetector2 createCollisionDetector()
        {
            return actor.createCollisionDetector();
        }
    }
}