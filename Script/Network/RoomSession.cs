using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Cratos
{
    //场景内与服务器kcp的连接,主要用来处理帧消息
    public class RoomSession
    {
        static public RoomSession inst
        {
            get
            {
                if (_inst == null)
                    _inst = new RoomSession();

                return _inst;
            }
        }

        static RoomSession _inst;

        SessState sessState = SessState.offline;

        Sess roomSessionSess = new Sess("tcp", "roomsession");

        RoomSession()
        {

        }

        public void logout()
        {
            roomSessionSess.close();
        }

        public void connect(string ip, string port)
        {
            string addr = ip + ":" + port;
            roomSessionSess.connect(addr, (isSucceed) =>
            {
                Debug.Log("roomseesion connect " + isSucceed);
                if(isSucceed)
                {
                    sessState = SessState.online;

                    //发送ClientVertifyReq消息
                    var vertifyMsg = new ClientVertifyReq();
                    ClientTokenInfo clientTokenInfo = MsgHandle.inst.loginMsgHandle.clientTokenInfo;
                    vertifyMsg.UID = clientTokenInfo.UID;
                    vertifyMsg.Token = Encoding.UTF8.GetBytes(clientTokenInfo.Token);
                    vertifyMsg.Source = 0;

                    send(vertifyMsg);
                }
            });
        }

        public void send(object msg)
        {
            roomSessionSess.send(msg);
        }

        public void send(string msgName, byte[] msgBuf)
        {
            var msgInfo = MsgDef.inst.getMsgByName(msgName);
            if (msgInfo == null)
                return;

            roomSessionSess.send(msgInfo.msgID, msgBuf);
        }
    }
}
