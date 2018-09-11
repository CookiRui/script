using BW31.SP2D;
using FixMath.NET;
using System.Collections.Generic;

using ML.SkillEdit.Runtime;

namespace Skill
{

    public class SkillSubContext : Context
    {
        public SkillSubContext(NodeMapInfo map, IContextObject owner, IDataScope globalData)
            : base(map, owner, globalData)
        { 
        }
        public override ITimer createTimer()
        {
            return new TTimer();
        }

        public override IContext createContext(NodeMapInfo map, IContextObject owner, IDataScope globalData)
        {
            return new SkillSubContext(map, owner, globalData);
        }

        public override IActorList2 createActorList2() {
            return new TActorList2();
        }

        public override CustomNodePrefab loadPrefab(string path) {
            if (owner != null) {
                return owner.context.loadPrefab(path);
            }
            return null;
        }
    }

    public class SkillContext : SkillSubContext
    {
        public new GlobalScope globalData { get { return base.globalData as GlobalScope; } } 
        
        public class GlobalScope : IDataScope
        {
            IList<IDataProvider> dataContents = new List<IDataProvider>();

            IDataProvider IDataScope.this[int index]
            {
                get
                {
                    return dataContents[index];
                }
                set
                {
                }
            }

            public void add( IDataProvider dataProvider )
            {
                dataContents.Add(dataProvider);
            }

            int IDataScope.count
            {
                get { return dataContents.Count; }
            }
        }

        public override CustomNodePrefab loadPrefab(string path) {
            return world.loadSkillNodePrefab(path);
        }

        public SkillContext(FBWorld world, NodeMapInfo map)
            :base(map, null, new GlobalScope())
        {
            m_world = world;
        }

        public FBWorld world { get { return m_world; } }

        protected FBWorld m_world;
    }


}