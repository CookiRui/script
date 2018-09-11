using Cratos;
using System;

partial class Game : Singleton<Game>
{
    public FBGame fbGame = null;

    public override void onInit()
    {
        base.onInit();

        InputEventTranslator.create();
    }

    public override void onUninit()
    {
        base.onUninit();

        InputEventTranslator.terminate();
    }

    public FBGame newFBGame()
    {
        fbGame = new FBGame();

        LogicEvent.fire("onFBGameNewed");

        return fbGame;
    }

    public void deleteFBGame()
    {
        RoomSession.inst.logout();

        fbGame.destory();

        fbGame = null;
    }

    public void run()
    {
    }

}