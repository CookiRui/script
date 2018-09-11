using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
public class LuaProxy : Singleton<LuaProxy>
{
    Dictionary<string, LuaTable> luaTableDic = new Dictionary<string, LuaTable>();

	public override void onInit()
    {
        
    }

    public void run( float deltaTime )
    {

    }


    public override void onUninit()
    {

    }



    public void startLuaVM(GameObject go)
    { 
        if(go != null)
        {
            go.AddComponent<LuaLoader>();
        }
    }



    public LuaTable getLuaTable(string name)
    {
        LuaTable tb;
        if(!luaTableDic.TryGetValue(name,out tb))
        {
            tb = LuaLoader.GetMainState().GetTable(name);

            if(tb == null)
            {
                Debugger.LogError("get lua table error,name=="+name);
            }

            luaTableDic.Add(name,tb);
        }

        LuaFunction constructFunction = tb.GetLuaFunction("new");
        if (constructFunction == null)
        {
            Debugger.LogError("there is no contruct function in luaTable=="+name);
            return null;
        }

        LuaTable ret;

        object[] rets = constructFunction.Call(tb);
        ret = (LuaTable)rets[0];

        constructFunction.Dispose();
        constructFunction = null;

        return ret;
    }

    public void clearLuaTableDic()
    {
        foreach(KeyValuePair<string,LuaTable> pair in this.luaTableDic)
        {
            LuaTable tb = pair.Value;
            if(tb != null)
            {
                tb.Dispose();
                tb = null;
            }
        }

        this.luaTableDic.Clear();
    }


    public void callLuaFunction(LuaTable lua, string funName, params object[] args)
    {
        if(lua == null)
        {
            Debuger.LogError("callLuaFunction luatable is nil");
            return;
        }

        LuaFunction fun = lua.GetLuaFunction(funName);
        if(fun == null)
        {
            Debuger.LogError("callLuaFunction fun does not exsit");
            return;
        }

        fun.Call(args);
        fun.Dispose();
        fun = null;
    }


}
