using UnityEngine;
using Cratos;

/*负责lua和c#互相通讯*/
namespace LuaInterface
{
    public partial class LuaBridge : MonoBehaviour
    {
        public static LuaBridge ins { get; private set; }


        LuaFunction luaProtoMsgParseFun;
        LuaFunction luaBinaryMsgParseFun;
        LuaFunction luaEventFun;

        void Awake()
        {
            ins = this;
        }

        void Start()
        {
            initLuaBridge();

            LogicEvent.add("on_ProtoMsgParse", this, "on_ProtoMsgParse");
            LogicEvent.add("on_BinMsgParse", this, "on_BinaryMsgParse");
            LogicEvent.add("on_LuaEvent", this, "on_LuaEvent");

            Zeus.inst.setRawMsgProc((msgID, msgName, msgBuf) =>
            {
                var msgInfo = MsgDef.inst.getMsgByID(msgID);
                if (msgInfo.msgType != MsgType.protobuf)
                    return;

                LogicEvent.fire("on_ProtoMsgParse", msgName, msgBuf);
            });
        }

        void destroyLuaBridge()
        {
            if (luaProtoMsgParseFun != null)
            {
                luaProtoMsgParseFun.Dispose();
                luaProtoMsgParseFun = null;
            }

            if (luaBinaryMsgParseFun != null)
            {
                luaBinaryMsgParseFun.Dispose();
                luaBinaryMsgParseFun = null;
            }

            if (luaEventFun != null)
            {
                luaEventFun.Dispose();
                luaEventFun = null;
            }
        }
        void initLuaBridge()
        {
            LuaTable lua = LuaLoader.GetMainState().GetTable("EventBridge");
            if (lua != null)
            {
                luaProtoMsgParseFun = lua.GetLuaFunction("onProtoMsgParse");
                luaBinaryMsgParseFun = lua.GetLuaFunction("onBinMsgParse");

                luaEventFun = lua.GetLuaFunction("onLuaEventParse");


                lua.Dispose();
                lua = null;
            }

        }

        public void resetLuaBridge()
        {
            destroyLuaBridge();
            initLuaBridge();
        }

        public void sendProtoMsg(LuaByteBuffer buf, string msgName)
        {
            Zeus.inst.sendToGateway(msgName, buf.buffer);
        }

        /***c# call lua****/
        void on_ProtoMsgParse(string msgName, byte[] buffer)
        {
            //Debug.Log("C# on_ProtoMsgParse " + msgName);
            if (luaProtoMsgParseFun != null)
            {
                luaProtoMsgParseFun.Call(msgName, new LuaByteBuffer(buffer));
            }
        }

        void on_BinaryMsgParse(string msgName, byte[] buffer)
        {
            if (luaBinaryMsgParseFun != null)
            {
                luaBinaryMsgParseFun.Call(msgName, new LuaByteBuffer(buffer));
            }
        }


        void on_LuaEvent(Params args)
        {
            if (luaEventFun != null)
            {
                luaEventFun.Call(args.list);
            }
        }


        public void onLogin(int playerCount, bool useAIPlayer, bool useAIGK)
        {
            string loginURL = getLoginURL(playerCount);
            Zeus.inst.login(SystemInfo.deviceUniqueIdentifier, "+1s", loginURL, null);

            MainCharacter.create();
            MainCharacter.instance.useAIPlayer = useAIPlayer;
            MainCharacter.instance.useAIGK = useAIGK;
        }

        string getLoginURL(int playerCount)
        {
            Debug.Log("当前需要人数：" + playerCount);
            var address = string.Empty;
            if (playerCount >= 4)
            {
                //for 4 players
                address = "http://192.168.150.191:8081/login";
            }
            else if (playerCount >= 2)
            {
                //for 2 players
                address = "http://192.168.150.191:8082/login";
            }
            else
            {
                //for 1 player
                //address = "http://192.168.150.191:8087/login";
                address = "http://192.168.150.191:8080/login";
                //address = "http://192.168.96.123:8080/login";
                //address = "http://118.89.114.201:8080/login"; 
            }

            return address;
        }

        public uint onLobbyConnect()
        {
            Zeus.inst.connectToGateway();
            return MainCharacter.instance.uid;
        }

        public void authorizeUDP(string ip, int port)
        {
            RoomSession.inst.connect(ip, port.ToString());
        }
    }
}

