using UnityEngine;
using LuaInterface;

public class UIProxy : MonoBehaviour
{
    public LuaTable tableCtrl { set; private get; }
    public LuaTable tableView { set; private get; }


    void Awake()
    {
        //jlx2017.04.20-log:放在Awakw内部赋值tableCtrl和tableView

        tableView = LuaProxy.instance.getLuaTable(name + "View");
        if (tableView == null)
        {
            Debuger.LogError("UIProxy tableView get error!");
            return;
        }
        tableCtrl = LuaProxy.instance.getLuaTable(name + "Ctrl");
        if (tableCtrl == null)
        {
            Debuger.LogError("createUI tableCtrl get error!");
            return;
        }

        tableView["transform"] = transform;
        tableView["ctrl"] = tableCtrl;

        tableCtrl["transform"] = transform;
        tableCtrl["gameObject"] = gameObject;
        tableCtrl["view"] = tableView;

        LuaFunction viewInit = tableView.GetLuaFunction("onInit");
        if (viewInit != null)
        {
            viewInit.Call(tableView);
            viewInit.Dispose();
            viewInit = null;
        }


        LuaFunction viewAwake = tableView.GetLuaFunction("onCreate");
        if (viewAwake != null)
        {
            viewAwake.Call(tableView);
            viewAwake.Dispose();
            viewAwake = null;
        }


        LuaFunction ctrlInit = tableView.GetLuaFunction("onInit");
        if (ctrlInit != null)
        {
            ctrlInit.Call(tableCtrl);
            ctrlInit.Dispose();
            ctrlInit = null;
        }

        LuaFunction ctrlAwake = tableCtrl.GetLuaFunction("onCreate");
        if (ctrlAwake != null)
        {
            ctrlAwake.Call(tableCtrl);
            ctrlAwake.Dispose();
            ctrlAwake = null;
        }

    }

    void OnDestroy()
    {
        if (tableView == null || tableCtrl == null)
        {
            return;
        }

        LuaFunction viewDestroy = tableView.GetLuaFunction("onDestroy");
        if (viewDestroy != null)
        {
            viewDestroy.Call(tableView);
            viewDestroy.Dispose();
            viewDestroy = null;
        }

        LuaFunction viewUnInit = tableView.GetLuaFunction("onUnInit");
        if (viewUnInit != null)
        {
            viewUnInit.Call(tableView);
            viewUnInit.Dispose();
            viewUnInit = null;
        }



        LuaFunction ctrlDestroy = tableCtrl.GetLuaFunction("onDestroy");
        if (ctrlDestroy != null)
        {
            ctrlDestroy.Call(tableCtrl);
            ctrlDestroy.Dispose();
            ctrlDestroy = null;
        }

        LuaFunction ctrlUnInit = tableView.GetLuaFunction("onUnInit");
        if (ctrlUnInit != null)
        {
            ctrlUnInit.Call(tableCtrl);
            ctrlUnInit.Dispose();
            ctrlUnInit = null;
        }

        tableView.Dispose();
        tableView = null;

        tableCtrl.Dispose();
        tableCtrl = null;
    }
}
