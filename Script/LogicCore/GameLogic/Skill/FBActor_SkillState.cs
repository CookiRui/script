
using FixMath.NET;
using BW31.SP2D;
using ML.SkillEdit.Runtime;
using System.Collections.Generic;

using Skill;

public partial class FBActor
{

    public FBSkillActor skillActor { get { if (_skillActor == null) _skillActor = new FBSkillActor(this); return _skillActor; } }
    FBSkillActor _skillActor = null;
    List<SkillActorCollisionDetector2> detectionList = null;
    SkillActorMovement movement = null;

    HashSet<SkillActorLock2> m_locks = new HashSet<SkillActorLock2>();

    public SkillActorLock2 createLock() {
        var lck = new SkillActorLock2(this);
        m_locks.Add(lck);
        if (!isSkilling) {
            doSkill();
        }
        return lck;
    }

    public void releaseLock(SkillActorLock2 lck) {
        if (m_locks.Remove(lck)) {
            lck._released();
        }
    }

    public IActorMovement2 createMovement()
    {
        if (movement != null) {
            movement.destroy();
        }
        movement = new SkillActorMovement(this);
        m_locks.Add(movement);
        if (!isSkilling) {
            doSkill();
        }
        return movement;
    }

    public void releaseMovement(SkillActorMovement m) {
        if (m == movement) {
            movement = null;
        }
    }

    public IActorCollisionDetector2 createCollisionDetector()
    {
        if (detectionList == null)
            detectionList = new List<SkillActorCollisionDetector2>();

        var dec = new SkillActorCollisionDetector2(this);
        detectionList.Add(dec);
        return dec;

    }
    
    public void removeCollisionDectector2(SkillActorCollisionDetector2 dec)
    {
        detectionList.Remove(dec);
    }

    public void onActorCollided(FBActor target)
    {
        if (target == null || detectionList == null || detectionList.Count == 0 )
            return;

        for (int i = 0; i < detectionList.Count; ++i)
        {
            detectionList[i].colliedActor(target.skillActor);
        }
    }

    class Skilling : State
    {

        public static readonly State instance = new Skilling();

        public override bool canBreak(FBActor actor, State state) { return false; }

        public override void leave(FBActor actor) {
            foreach (var lck in actor.m_locks) {
                lck._released();
            }
            actor.m_locks.Clear();
        }

        public override void update(FBActor actor, Fix64 deltaTime)
        {
            actor.m_particle.velocity = actor.movement != null ? actor.movement.velocity : FixVector2.kZero;
            if (actor.m_locks.Count == 0) {
                actor.m_nextState = MoveWaitingState.instance;
            }
        }
    }

}