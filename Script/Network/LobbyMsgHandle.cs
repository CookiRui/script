using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cratos;

//大厅消息处理
public class LobbyMsgHandle
{
    public LobbyMsgHandle()
    {
        initEvent();
    }

    void initEvent()
    {
        Events.add("TableStartNotify", this, "onTableStartNotify");
        Events.add("TableUserEndNotify", this, "onTableUserEndNotify");
    }

    public void destroy()
    {
        Events.remove(this);
    }

    public void onTableStartNotify(TableStartNotify cmd)
    {
        LogicEvent.fire2Lua("onTableStartNotify");

        Game.instance.newFBGame();
        Game.instance.fbGame.setupFBGame(cmd.tableId, cmd.mapId);

        Game.instance.fbGame.createCoach(string.Format("{0}/Resources/Config/Behaviac", UnityEngine.Application.dataPath));

        cmd.allPlayerInfos.ForEach(a =>
        {
            var mainCharacter = a.uid == MainCharacter.instance.uid;
            if (mainCharacter)
            {
                MainCharacter.instance.team = (FBTeam)a.team;
            }
            Game.instance.fbGame.createPlayer(a.uid, a.frameId, a.roleId, a.team, a.name, mainCharacter, a.ai);
        });

        Game.instance.fbGame.fbGameCreateCompleted();
    }

    public void onTableUserEndNotify(TableUserEndNotify cmd)
    {
        if (cmd.playerId == MainCharacter.instance.uid)
        {
            LogicEvent.fire2Lua("onTableUserEndNotify");

#if FRAME_RECORDING
            if (FrameRecording.instance != null)
            {
                FrameRecording.instance.saveServerMessages();
            }
#endif
            Game.instance.deleteFBGame();
        }
        else
        {
            Debuger.Log("其他人退出房间 ： " + cmd.playerId);
        }
    }
}
