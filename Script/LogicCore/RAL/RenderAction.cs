using System.Collections.Generic;
using System.Reflection;
namespace RAL {

    public class RenderActionGenerator {

        public RenderAction createRenderAction(System.Type type, object[] parameters) {
            RenderActionPool pool;
            if (!m_pools.TryGetValue(type, out pool)) {
                pool = new RenderActionPool();
                foreach (var mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)) {
                    if (mi.Name == "init") {
                        pool.init = mi;
                        break;
                    }
                }
            }
            return _createFromPool(type, pool, parameters);
        }

        public void releaseRenderAction(RenderAction ra) {
            RenderActionPool pool;
            if (m_pools.TryGetValue(ra.GetType(), out pool)) {
                // TODO:
            }
        }

        private RenderAction _createFromPool(System.Type type, RenderActionPool pool, object[] parameters) {
            // TODO:
            var ret = type.GetConstructor(System.Type.EmptyTypes).Invoke(null) as RenderAction;
            if (pool.init != null) {
                pool.init.Invoke(ret, parameters);
            }
            return ret;
        } 

        class RenderActionPool {
            public LinkedList<RenderAction> pool = new LinkedList<RenderAction>();
            public System.Reflection.MethodInfo init;
        }

        private Dictionary<System.Type, RenderActionPool> m_pools = new Dictionary<System.Type, RenderActionPool>();
    }


    [RenderAction(RenderableActionID.None)]
    public class None : RenderAction {
        public void init() {
            m_time = UnityEngine.Time.realtimeSinceStartup;
            m_frameCount = UnityEngine.Time.frameCount;
        }

        public void dump()
        {
            //UnityEngine.Debug.Log(string.Format("NRA, cf: {0}, pf: {1} dt: {2}", m_frameCount, UnityEngine.Time.frameCount, UnityEngine.Time.realtimeSinceStartup - m_time));
        }

        private int m_frameCount;
        private float m_time;
    }
}
