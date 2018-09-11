using Cratos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Profiler : UnityAllSceneSingletonVisible<Profiler>
{
    public class Status
    {
        //从发送到收
        public float fromSendToReceiveMsg = 0;

        //从收消息到渲染层处理
        public float fromReceiveMsgToRenderable = 0;
    };

    class PingData
    {
        public float sendTime;
        public float receiveTime;
        public float doneTime;
    };

    public Status status = new Status();

    void Awake()
    {
        LogicEvent.add("onTestBinMsg", this, "onTestBinMsg");
        LogicEvent.add("onPingActionDone", this, "onPingActionDone");
    }

    void OnDestroy()
    {
        LogicEvent.remove(this);
    }

    void onPingActionDone(uint stamp)
    {
        if (!sendPingList.ContainsKey(stamp))
            return;

        status.fromReceiveMsgToRenderable = Time.unscaledTime - sendPingList[stamp].receiveTime;

        Debug.Log("Ping Action fromSendToReceiveMsg:" + status.fromSendToReceiveMsg + " fromReceiveMsgToRenderable:" + status.fromReceiveMsgToRenderable);
    }
    void onTestBinMsg(TestBinMsg msg)
    {
        if (!sendPingList.ContainsKey(msg.stamp))
            return;

        sendPingList[msg.stamp].receiveTime = Time.unscaledTime;

        status.fromSendToReceiveMsg = sendPingList[msg.stamp].receiveTime - sendPingList[msg.stamp].sendTime;

        Game.instance.fbGame.generateRenderAction<RAL.ProfilerAction>(msg.stamp);
    }

    public void startPing()
    {
        _TestPingData = true;
        StartCoroutine(sendTestPingData());
    }
    public void stopPing()
    {
        _TestPingData = false; 
    }

    bool _TestPingData = false;
    IEnumerator sendTestPingData()
    {
        while (_TestPingData)
        {
            currentPingIndex++;


            PingData data = new PingData();
            data.sendTime = Time.unscaledTime;
            sendPingList.Add(currentPingIndex, data);

            TestBinMsg msg = new TestBinMsg();
            msg.stamp = currentPingIndex;
            RoomSession.inst.send(msg);

            yield return new WaitForSeconds(2.0f);
        }
    }

    Dictionary<uint, PingData> sendPingList = new Dictionary<uint, PingData>();
    uint currentPingIndex = 0;
}