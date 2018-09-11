using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cratos;

//上层消息处理
public class MsgHandle : MonoBehaviour
{
    public static MsgHandle inst;

    public LoginMsgHandle loginMsgHandle;
    public LobbyMsgHandle lobbyMsgHandle;
    public FrameMsgHanlde frameMsgHandle;

    void Awake()
    {
        inst = this;

        loginMsgHandle = new LoginMsgHandle();
        lobbyMsgHandle = new LobbyMsgHandle();
        frameMsgHandle = new FrameMsgHanlde();
    }

	// Use this for initialization
	void Start ()
    {
        MsgDef.inst.addMsgDef(MsgIDDef.ClientFrameMsgID, "ClientFrameMsg");
        MsgDef.inst.addMsgDef(MsgIDDef.ServerFrameMsgID, "ServerFrameMsg");
        MsgDef.inst.addMsgDef(MsgIDDef.FramesMsgID, "FramesMsg");
    }

    void OnDestroy()
    {
        RoomSession.inst.logout();

        if (loginMsgHandle != null)
        {
            loginMsgHandle.destroy();
            loginMsgHandle = null;
        }

        if(lobbyMsgHandle != null)
        {
            lobbyMsgHandle.destroy();
            lobbyMsgHandle = null;
        }

        if(frameMsgHandle != null)
        {
            frameMsgHandle.destroy();
            frameMsgHandle = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
