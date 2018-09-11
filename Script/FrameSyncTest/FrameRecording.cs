using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cratos;

class FrameRecording : UnityAllSceneSingletonVisible<FrameRecording>
{
    string serverMessagesName = "FrameRecord/serverFrames.bytes";

    public override void onInit()
    {

    }
    public override void onUninit()
    {

    }

    public float renderPlayerSpeed = 2.0f;

    public void beginSimulation()
    {
        loadServerMessages();
        StartCoroutine(simulateServerMessage());

        fbGame.frameSync.start();
    }
    FBGame fbGame = null;
    public void stopSimulation()
    {
        fbGame.frameSync.over();
    }

    LinkedList<ServerFrameMsg> serverMessages = new LinkedList<ServerFrameMsg>();
    public void pushServerMessage(ServerFrameMsg cmd)
    {
        serverMessages.AddLast(cmd);
    }

    public void saveServerMessages()
    {
        var tsn = new TableStartNotify();
        var ra = fbGame.gamePlayers.GetEnumerator();
        while (ra.MoveNext())
        {
            tsn.allPlayerInfos.Add(new PlayerInfo
            {
                uid = 0,
                frameId = (byte)ra.Current.Value.actor.id,
                roleId = (ushort)ra.Current.Value.actor.roleId,
                team = (byte)ra.Current.Value.actor.team,
                name = ra.Current.Value.actor.name,
                ai = ra.Current.Value.ai

            });
        }
        var fm = new FramesMsg();
        LinkedListNode<ServerFrameMsg> node = serverMessages.First;
        while (node != null)
        {
            fm.allFrameMessages.Add(node.Value);
            node = node.Next;
        }
        BytesStream stream = new BytesStream();
        tsn.marshal(stream);
        fm.marshal(stream);
//         tsn.serialize(stream);
//         fm.serialize(stream);
        ResourceManager.inst.saveToFile(stream, serverMessagesName, ResourceManager.PathType.PT_Resources);
    }

    void loadServerMessages()
    {
        if( fbGame != null )
        {
            fbGame.destory();
            fbGame = null;
        }

        //byte[] data = ResourcesManager.instance.getFileData(serverMessagesName);
        byte[] data = ConfigResourceLoader.inst.getFileData(serverMessagesName);
        BytesStream stream = new BytesStream(data);

        serverMessages.Clear();

        var tsn = new TableStartNotify();
        tsn.unMarshal(stream);
        //tsn.unserialize(stream);

        fbGame = new FBGame();
        LogicEvent.fire("onFBGameNewed", fbGame );
        fbGame.setupFBGame(tsn.tableId, tsn.mapId);

        fbGame.createCoach(string.Format("{0}/Resources/Config/Behaviac", UnityEngine.Application.dataPath));

        //创建玩家
        var playerCount = tsn.allPlayerInfos.Count;
        for (int i = 0; i < playerCount; ++i)
        {
            var player = tsn.allPlayerInfos[i];
            fbGame.createPlayer(0,player.frameId, player.roleId, player.team,"给我来段FreeStyle",false, player.ai);
        }

        //处理输入消息
        var fm = new FramesMsg();
        fm.unMarshal(stream);
        //fm.unserialize(stream);
        for (int i = 0; i < fm.allFrameMessages.Count; ++i)
        {
            var msg = fm.allFrameMessages[i];

            pushServerMessage(msg);
        }

        fbGame.changeState(GameState.Ready);
        
    }


    ServerFrameMsg popServerMessage()
    {
        if (serverMessages.Count == 0)
            return null;
        ServerFrameMsg msg = serverMessages.First.Value;
        serverMessages.RemoveFirst();
        return msg;
    }

    public bool isGameOver
    {
        get { return serverMessages.Count == 0; }
    }

    public float PlaySpeed = 1.0f;

    IEnumerator simulateServerMessage()
    {
        fbGame.debugLogicStart();

        while (true)
        {
            for (int i = 0; i < (int)PlaySpeed; ++i)
            {
                ServerFrameMsg serverMsg = popServerMessage();
                if (serverMsg == null)
                    yield break;

                fbGame.frameSync.pushFrameInputEvent(serverMsg);
                fbGame.frameSync.udpateFrame(Time.unscaledTime);

                fbGame.debugFrameLogic();
            }

            yield return new WaitForSeconds((float)FrameSync.LOGIC_UPDATE_TIME);

        }

    }
}