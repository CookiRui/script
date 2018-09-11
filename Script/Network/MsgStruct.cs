using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cratos;
using System;
using System.Text;

public class ClientFrameMsg : IMsg
{
    public short angle = 0;
    public byte[] keys = new byte[2];

    public override byte[] marshal(BytesStream stream)
    {
        stream.Write(angle);
        stream.Write(keys);
        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        angle = stream.ReadInt16();
        keys = stream.ReadBytes(keys.Length);
        return true;
    }
}

public class ServerFrameInputEvent : ClientFrameMsg
{
    byte _objectID;
    public UInt32 objectID
    {
        get
        {
            return (UInt32)(_objectID & 0x7F);
        }
        set
        {
            _objectID = (byte)value;
        }
    }
    //最高位表示是否被AI托管
    public bool isAITakeOver
    {
        get { return _objectID > 8; }
    }

    public override byte[] marshal(BytesStream stream)
    {
        stream.Write(_objectID);
        base.marshal(stream);
        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        _objectID = stream.ReadByte();
        base.unMarshal(stream);
        return true;
    }
}

public class ServerFrameMsg : IMsg
{
    //当前帧
    public UInt16 frameNum;
    //帧时间列表
    public List<ServerFrameInputEvent> events = new List<ServerFrameInputEvent>();

    public override byte[] marshal(BytesStream stream)
    {
        stream.Write(frameNum);
        byte count = (byte)events.Count;
        stream.Write(count);
        for (int i = 0; i < events.Count; ++i)
        {
            events[i].marshal(stream);
        }

        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        frameNum = stream.ReadUInt16();
        uint count = stream.ReadByte();
        for (uint i = 0; i < count; ++i)
        {
            ServerFrameInputEvent evt = new ServerFrameInputEvent();
            evt.unMarshal(stream);
            events.Add(evt);
        }
        return true;
    }
}

//某个时间段的所有帧消息,服务器下发的帧消息队列
public class FramesMsg : IMsg
{
    //帧时间列表
    public List<ServerFrameMsg> allFrameMessages = new List<ServerFrameMsg>();

    public override byte[] marshal(BytesStream stream)
    {
        stream.Write((ushort)allFrameMessages.Count);

        for (int i = 0; i < allFrameMessages.Count; ++i)
        {
            allFrameMessages[i].marshal(stream);
        }

        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        var frameCount = stream.ReadUInt16();
        while (stream.Pos < stream.Buf.Length - 1 && allFrameMessages.Count < frameCount)
        {
            ServerFrameMsg evt = new ServerFrameMsg();
            evt.unMarshal(stream);
            allFrameMessages.Add(evt);
        }

        return true;
    }
}

public class PlayerInfo : IMsg
{
    public UInt64 uid;
    public byte frameId;
    public UInt16 roleId;
    public byte team;
    public string name;
    public bool ai;

    public override byte[] marshal(BytesStream stream)
    {
        stream.Write(uid);
        stream.Write(frameId);
        stream.Write(roleId);
        stream.Write(team);
        stream.WriteStringByte(name);
        stream.Write(ai);
        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        uid = stream.ReadUInt64();
        frameId = stream.ReadByte();
        roleId = stream.ReadUInt16();
        team = stream.ReadByte();
        name = stream.ReadString();
        ai = stream.ReadBoolean();
        return true;
    }
}

public class TableStartNotify : IMsg
{
    public UInt64 tableId;
    public UInt32 mapId;
    //帧时间列表
    public List<PlayerInfo> allPlayerInfos = new List<PlayerInfo>();

    public override byte[] marshal(BytesStream stream)
    {
        stream.Write(tableId);
        stream.Write(mapId);
        stream.Write((byte)allPlayerInfos.Count);

        for (int i = 0; i < allPlayerInfos.Count; ++i)
        {
            allPlayerInfos[i].marshal(stream);
        }

        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        tableId = stream.ReadUInt64();
        mapId = stream.ReadUInt32();
        var playerCount = stream.ReadByte();
        while (stream.Pos < stream.Buf.Length - 1 && allPlayerInfos.Count < playerCount)
        {
            PlayerInfo evt = new PlayerInfo();
            evt.unMarshal(stream);
            allPlayerInfos.Add(evt);
        }

        return true;
    }
}

public class TableUserEndReq : IMsg
{
    public override byte[] marshal(BytesStream stream)
    {
        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        return true;
    }
}

public class TableUserEndNotify : IMsg
{
    public UInt64 playerId;

    public override byte[] marshal(BytesStream stream)
    {
        stream.Write(playerId);
        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        playerId = stream.ReadUInt64();
        return true;
    }
}

class TableClientReadyReq : IMsg
{
    public override byte[] marshal(BytesStream stream)
    {
        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        return true;
    }
}

class TestBinMsg : IMsg
{
    public uint stamp = 0;
    public override byte[] marshal(BytesStream stream)
    {
        stream.Write(stamp);
        return stream.GetUsedBytes();
    }

    public override bool unMarshal(BytesStream stream)
    {
        stamp = stream.ReadUInt32();
        return true;
    }
}
