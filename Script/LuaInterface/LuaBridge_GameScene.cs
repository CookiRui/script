/*负责lua和c#互相通讯*/

using Cratos;
namespace LuaInterface
{
    public partial class LuaBridge
    {     
        public void changeReplaySpeed()
        {
            if (FBReplay.instance == null) return;
            //FBReplay.instance.changePlaySpeed();
        }

        public int getReplaySpeed()
        {
            return 0;
            //if (FBReplay.instance == null) return 0;
            //return FBReplay.instance.playSpeed;

        }

        public bool createFBReplay()
        {
            return false;
            //FBReplay.create();
            //return FBReplay.instance.createFBReplay();
        }

        public void destroyReplay()
        {
            FBReplay.terminate();
        }

        public void exitFBGame()
        {
            SceneResourceLoader.inst.unLoadSceneMap();
            var req = new TableUserEndReq();
            Zeus.inst.sendToGateway(req);
        }

        public string converNumberToTime(double number)
        {
            return ((int)number).toTime();
        }
    }
}

