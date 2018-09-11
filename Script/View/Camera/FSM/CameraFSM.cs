using System.Collections.Generic;

namespace FBCamera
{
    class CameraFSM
    {
        Dictionary<GameState, CameraStateBase> states = new Dictionary<GameState, CameraStateBase>();
        public GameState curStateType { get; private set; }
        CameraStateBase curState;
        CameraCtrl cameraCtrl;

        public CameraFSM(CameraCtrl ctrl)
        {
            cameraCtrl = ctrl;
        }

        CameraStateBase getState(GameState type)
        {
            CameraStateBase state;
            if (states.TryGetValue(type, out state))
            {
                return state;
            }

            switch (type)
            {
                case GameState.Enter: state = new EnterState(cameraCtrl); break;
                case GameState.Gaming: state = new GamingState(cameraCtrl); break;
                case GameState.Goal: state = new GoalState(cameraCtrl); break;
                case GameState.Replay: state = new ReplayState(cameraCtrl); break;
                case GameState.Over: state = new OverState(cameraCtrl); break;
            }
            states.Add(type, state);
            return state;
        }

        public void execute()
        {
            if (curState == null) return;
            curState.execute();
        }

        public void changeState(GameState state)
        {
            if (curStateType == state && curState != null) return;

            if (curState != null)
            {
                curState.exit();
            }
            curState = getState(state);
            curState.enter();
            curStateType = state;
        }

        public void clear()
        {
            curState = null;
        }
    }
}