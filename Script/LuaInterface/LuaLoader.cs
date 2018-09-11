namespace LuaInterface
{
    public class LuaLoader : LuaClient
    {

        new void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            LuaFileUtils.Instance.beZip = false;
#else
        LuaFileUtils.Instance.beZip = true;
#endif

            if (LuaFileUtils.Instance.beZip)
            {
                loadLuaAB();
            }
            else
            {
                startLua();
            }
        }

        void loadLuaAB()
        {
            LuaABLoader labl = gameObject.AddComponent<LuaABLoader>();
            if(labl != null)
            {
                labl.startLoad();
            }
        }

        public void startLua()
        {
            //int lua bridge
            LuaBridge lb = gameObject.GetComponent<LuaBridge>();
            if (lb == null)
            {
                lb = gameObject.AddComponent<LuaBridge>();
            }
            else
            {
                lb.resetLuaBridge();
            }
            

            //execute main.lua
            OnLoadFinished();
        }

        //after hotupdate
        public void resetLua()
        {
            if(LuaFileUtils.Instance.beZip)
            {
                LuaFileUtils.Instance.ClearSearchBundle();
                loadLuaAB();
            }
        }
    }
}


