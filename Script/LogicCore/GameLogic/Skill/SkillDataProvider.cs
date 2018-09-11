using BW31.SP2D;
using FixMath.NET;
using System.Collections.Generic;

using ML.SkillEdit.Runtime;

namespace Skill {

    class TBool : IBool
    {
        public TBool(bool v) { _value = v; }
        bool _value = false;
        public bool value
        {
            get { return _value; }
            set { _value = value; }
        }
        public static explicit operator TBool(bool value)
        {
            return new TBool(value);
        }
    };

    class TInt : IInt
    {
        public TInt() { }
        public TInt(int v) { _value = v; }
        int _value = 0;
        public int value
        {
            get { return _value; }
            set { _value = value; }
        }
        public static explicit operator TInt(int value)
        {
            return new TInt(value);
        }
    };

    class TFloat : IFloat
    {
        public TFloat() { value = Fix64.Zero; }
        public TFloat(Fix64 value) { this.value = value; }
        public Fix64 value;
        float IFloat.value { get { return (float)value; } set { this.value = (Fix64)value; } }
    };

    class TString : IString
    {
        public TString() { _value = string.Empty; }
        public TString(string v) { _value = v; }
        string _value;
        public string value
        {
            get { return _value; }
            set { _value = value; }
        }
        public static explicit operator TString(string value)
        {
            return new TString(value);
        }
    };

    class TVector2 : IVector2
    {
        public TVector2() { value = FixVector2.kZero; }
        public TVector2(FixVector2 value) { this.value = value; }
        public FixVector2 value;

        IFloat IVector2.x()
        {
            return new TFloat(value.x);
        }

        IFloat IVector2.y()
        {
            return new TFloat(value.y);
        }

        public IVector2 direction(IVector2 from)
        {
            FixVector2 ret = value;
            if (from is TVector2)
            {
                ret -= ((TVector2)from).value;
            }
            else {
                ret -= new FixVector2((Fix64)from.x().value, (Fix64)from.y().value);
            }
            return new TVector2(ret.normalized);
        }

        public static explicit operator TVector2(FixVector2 value)
        {
            return new TVector2(value);
        }
    };

    class TActorList2 : IActorList2
    {

        List<FBSkillActor> m_list = new List<FBSkillActor>();

        IInt IActorList2.count()
        {
            return new TInt(m_list.Count);
        }

        bool IActorList2.insert(IActor2 actor)
        {
            var a = actor as FBSkillActor;
            if (a == null) {
                return false;
            }
            for (int i = 0; i < m_list.Count; ++i) {
                if (m_list[i].actor == a.actor) {
                    return false;
                }
            }
            m_list.Add(a);
            return true;
        }

        bool IActorList2.remove(IActor2 actor)
        {
            var a = actor as FBSkillActor;
            if (a == null) {
                return false;
            }
            for (int i = 0; i < m_list.Count; ++i) {
                if (m_list[i].actor == a.actor) {
                    m_list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        void IActorList2.clear()
        {
            m_list.Clear();
        }
    }
 
    class TTimer : ITimer
    {
        Fix64 _totalTime = Fix64.Zero;
        Fix64 _currentTime = Fix64.Zero;
        void ITimer.reset(IFloat totalTime)
        {
            _totalTime = totalTime.toFix64();
            _currentTime = Fix64.Zero;
        }

        bool ITimer.update(IFloat deltaTime)
        {
            Fix64 thisDeltaTime = deltaTime.toFix64();
            _currentTime += thisDeltaTime;
            if (_currentTime >= _totalTime)
            {
                //Debuger.Log("Time Over:" + (float)_totalTime + " id:" + timerCursor);
                return false;
            }

            return true;
        }

        IFloat ITimer.progress
        {
            get
            {
                Fix64 percent = _currentTime / _totalTime;
                return new TFloat(percent);
            }
        }
    }

    public class SkillActorLock2 : IActorLock2 {

        public SkillActorLock2(FBActor actor) {
            m_actor = actor;
        }

        

        public void destroy() {
            if (m_actor != null) {
                m_available = false;
                m_actor.releaseLock(this);
            }
        }

        public virtual void _released() {
            m_actor = null;
            m_available = false;
        }

        public bool available { get { return m_available; } }

        protected FBActor m_actor;
        private bool m_available = true;
    }

    public class SkillActorMovement : SkillActorLock2, IActorMovement2 {

        public FixVector2 velocity = FixVector2.kZero;

        public SkillActorMovement(FBActor actor) : base(actor) {}

        public void setVelocity(IVector2 direction, IFloat speed) {
            velocity = direction.toFixVector2() * speed.toFix64();
        }

        public override void _released() {
            m_actor.releaseMovement(this);
            base._released();
        }
    }

    public class SkillActorCollisionDetector2 : IActorCollisionDetector2
    {
        List<FBSkillActor> _colliedActorList = null;
        int colliedActorIndex = -1;
        FBActor actor = null;

        public SkillActorCollisionDetector2(FBActor actor)
        {
            this.actor = actor;
        }

        public void colliedActor(FBSkillActor actor)
        {
            if (_colliedActorList == null)
                _colliedActorList = new List<FBSkillActor>();
            _colliedActorList.Add(actor);
        }

        public bool moveMext()
        {
            if (_colliedActorList == null)
                return false;
            ++colliedActorIndex;
            if (colliedActorIndex >= _colliedActorList.Count)
            {
                colliedActorIndex = -1;
                _colliedActorList.Clear();
                return false;
            }
            return true;
        }

        public IActor2 current
        {
            get
            {
                return _colliedActorList[colliedActorIndex];
            }
        }

        public void destroy()
        {
            actor.removeCollisionDectector2(this);
        }
    }

    static class DataProviderExtend {
        public static Fix64 toFix64(this IFloat value) {
            var t = value as TFloat;
            return t != null ? t.value : (Fix64)value.value;
        }

        public static FixVector2 toFixVector2(this IVector2 value) {
            var t = value as TVector2;
            return t != null ? t.value : new FixVector2(value.x().toFix64(), value.y().toFix64());
        }
    }
}