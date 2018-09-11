using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cratos;

//登录消息处理
public class LoginMsgHandle
{
    private bool hasLogin = false;

    public ClientTokenInfo clientTokenInfo = null;

    public LoginMsgHandle()
    {
        initEvent();
    }

    void initEvent()
    {
        Events.add(ZeusEvent.LoginSucceed, this, "onLoginSucceed");
        Events.add(ZeusEvent.LoginFailed, this, "onLoginFailed");
        Events.add("onReqTokenSucceed", this, "onReqTokenSucceed");
    }

    public void destroy()
    {
        Events.remove(this);
    }

    public void onLoginSucceed()
    {
        if (!hasLogin)
        {
            hasLogin = true;
            LogicEvent.fire2Lua("onUserLoginOk");
        }
    }

    public void onLoginFailed()
    {
        LogicEvent.fire2Lua("onUserLoginFailed");
    }

    public void onReqTokenSucceed(ClientTokenInfo info)
    {
        clientTokenInfo = info;
        MainCharacter.instance.uid = (uint)info.UID;
        LogicEvent.fire2Lua("onLoginSuccess");
    }
}
