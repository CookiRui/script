
namespace RenderingProcess {
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RenderActionProcessorAttribute : System.Attribute {
        public System.Type renderActionType { get; private set; }
        public RenderActionProcessorAttribute(System.Type renderActionType) {
            this.renderActionType = renderActionType;
        }
    }


    public class RenderActionProcessorFactory {
        public static RenderActionProcessorFactory instance {
            get {
                if (s_instance == null) {
                    s_instance = new RenderActionProcessorFactory();
                }
                return s_instance;
            }
        }
        private static RenderActionProcessorFactory s_instance;
        private RenderActionProcessorFactory() {

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var ti in assembly.GetTypes()) {
                if (ti.Namespace != "RenderingProcess") {
                    continue;
                }
                foreach (RenderActionProcessorAttribute attrib in ti.GetCustomAttributes(typeof(RenderActionProcessorAttribute), false)) {
                    if (m_registry.ContainsKey(attrib.renderActionType)) {
                        UnityEngine.Debug.Log("!!!");
                    }
                    m_registry.Add(attrib.renderActionType, new ProcessorInfo() {
                        doProgress = ti.GetMethod("doProgress", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public),
                        doDone = ti.GetMethod("doDone", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                    });
                    break;
                }
            }

        }

        public void doProgress(RAL.RenderAction ra, float progress) {
            var info = _queryInfo(ra.GetType());
            if (info != null && info.doProgress != null) 
            {
                try
                {
                    info.doProgress.Invoke(null, new object[] { ra, progress });
                }
                catch
                {
                    Debuger.LogError("do Process invoke failed " + ra.ToString());
                }

            }
        }

        public void doDone(RAL.RenderAction ra) {
            var info = _queryInfo(ra.GetType());
            if (info != null && info.doDone != null)
            {
                try
                {
                    info.doDone.Invoke(null, new object[] { ra });
                }
                catch
                {
                    Debuger.LogError("doDone invoke failed " + ra.ToString());
                }
            }
        }

        class ProcessorInfo {
            public System.Reflection.MethodInfo doProgress;
            public System.Reflection.MethodInfo doDone;
        }

        System.Collections.Generic.Dictionary<System.Type, ProcessorInfo> m_registry = new System.Collections.Generic.Dictionary<System.Type, ProcessorInfo>();

        ProcessorInfo _queryInfo(System.Type type) {
            ProcessorInfo info;
            if (m_registry.TryGetValue(type, out info)) {
                return info;
            }
            if (type.BaseType != null && type.BaseType != typeof(RAL.RenderAction)) {
                return _queryInfo(type.BaseType);
            }
            return null;
        }
    }
}
