
namespace RAL {

    public class RenderActionFactory {

        public static RenderActionFactory instance {
            get {
                if (s_instance == null) {
                    s_instance = new RenderActionFactory();
                }
                return s_instance;
            }
        }
        private static RenderActionFactory s_instance;


        public System.Type getRenderActionType(RenderableActionID name) {
            System.Type ret;
            m_registry.TryGetValue(name, out ret);
            return ret;
        }

        public void forEach(System.Action<RenderableActionID, System.Type> iterator)
        {
            foreach (var kv in m_registry) {
                iterator(kv.Key, kv.Value);
            }
        }

        private RenderActionFactory() {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var ti in assembly.GetTypes()) {
                if (ti.Namespace != "RAL") {
                    continue;
                }
                foreach (RenderActionAttribute attrib in ti.GetCustomAttributes(typeof(RenderActionAttribute), false)) {
                    m_registry.Add(attrib.id, ti);
                    break;
                }
            }
        }

        private System.Collections.Generic.Dictionary<RenderableActionID, System.Type> m_registry = new System.Collections.Generic.Dictionary<RenderableActionID, System.Type>();
    }


    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RenderActionAttribute : System.Attribute {

        public RenderableActionID id { get; private set; }

        public RenderActionAttribute(RenderableActionID id)
        {
            this.id = id;
        }

    }

}