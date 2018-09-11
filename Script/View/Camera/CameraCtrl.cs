using UnityEngine;
using FBCamera;
using FixMath.NET;
using System.Collections;
using Cratos;

partial class CameraCtrl : MonoBehaviour
{
    #region 调试用
    public bool drawRect;
    #endregion

    public CameraConfig config { get; private set; }
    public BallView ball { get { return SceneViews.instance.getCurFBScene().ball; } }
    public Camera cam { get; private set; }
    public CameraFSM fsm { get; private set; }

    public SkyView skyView { get; set; }

    #region MonoBehaviour methods

    void Awake()
    {
        config = new CameraConfig(ConfigResourceLoader.inst.loadConfig("Config/camera.xml").ToXml());
        cam = GetComponent<Camera>();
        cam.fieldOfView = config.defaultFOV;
        fsm = new CameraFSM(this);
        fsm.changeState(GameState.Enter);

        LogicEvent.add("onBeginReplay", this, "onBeginReplay");
        if (WithoutEnterShow_4Test_EditorOnly.instance != null)
        {
            fsm.changeState(GameState.Gaming);
        }
        else
        {
            fsm.changeState(GameState.Enter);
        }
        LogicEvent.add("onGameReady", this, "onGameReady");
        LogicEvent.add("onGameOver", this, "onGameOver");
    }

    void OnDestroy()
    {
        LogicEvent.remove(this);
    }

    //void FixedUpdate()
    //{
    //    UnityEngine.Debug.LogError("CameraCtrl FixedUpdate");

    //    if (fsm == null) return;
    //    if (fsm.curStateType != GameState.Gaming) return;
    //    fsm.execute();

    //}
    //jlx2017.07.17-log:暂时使用LateUpdate，如果效果不流畅，考虑使用FixedUpdate
    void LateUpdate()
    {
        if (skyView != null)
        {
            skyView.transform.position = transform.position;
        }

        if (fsm != null)
        {
            fsm.execute();
        }
    }

#if UNITY_EDITOR

    void OnGUI()
    {
        if (!drawRect) return;
        draw(config.outRect);
        draw(config.inRect);
        drawLine(config.leftBottomBegin, config.leftBottomEnd);
        drawLine(config.rightBottomBegin, config.rightBottomEnd);

        //drawDiagonal();
    }

    void draw(Rect rect)
    {
        var point1 = cam.ViewportToScreenPoint(new Vector2 { x = rect.xMin, y = 1 - rect.yMin });
        var point2 = cam.ViewportToScreenPoint(new Vector2 { x = rect.xMin, y = 1 - rect.yMax });
        var point3 = cam.ViewportToScreenPoint(new Vector2 { x = rect.xMax, y = 1 - rect.yMax });
        var point4 = cam.ViewportToScreenPoint(new Vector2 { x = rect.xMax, y = 1 - rect.yMin });
        GUILine.draw(point1, point2);
        GUILine.draw(point2, point3);
        GUILine.draw(point3, point4);
        GUILine.draw(point4, point1);
        GUILine.draw(point4, point1);
    }

    void drawDiagonal()
    {
        var point1 = cam.ViewportToScreenPoint(Vector2.zero);
        var point2 = cam.ViewportToScreenPoint(new Vector2 { x = 1 });
        var point3 = cam.ViewportToScreenPoint(new Vector2 { y = 1 });
        var point4 = cam.ViewportToScreenPoint(Vector2.one);
        GUILine.draw(point1, point4, Color.red);
        GUILine.draw(point2, point3, Color.red);
    }

    void drawLine(Vector2 begin, Vector2 end)
    {
        var point1 = cam.ViewportToScreenPoint(new Vector2 { x = begin.x, y = 1 - begin.y });
        var point2 = cam.ViewportToScreenPoint(new Vector2 { x = end.x, y = 1 - end.y });
        GUILine.draw(point1, point2, Color.red);
    }

    [ContextMenu("Reload Config")]
    void reloadConfig()
    {
        config = new CameraConfig(ConfigResourceLoader.inst.loadConfig("Config/camera.xml").ToXml());
    }

#endif
    #endregion

    #region events

    void onBeginReplay(Location door,
                    uint gkId,
                    float positionRandomValue,
                    float shootRandomValue,
                    float goalRandomValue)
    {
        replayDoor = door;
        replayGK = SceneViews.instance.getCurFBScene().getActor(gkId);
        this.positionRandomValue = (float)shootRandomValue;
        this.shootRandomValue = (float)shootRandomValue;
        this.goalRandomValue = (float)goalRandomValue;
        fsm.changeState(GameState.Replay);
    }

    void onGameReady(CampType camp)
    {
        this.camp = camp;
        fsm.changeState(GameState.Gaming);
    }

    void onGameOver()
    {
        fsm.changeState(GameState.Over);
    }

    #endregion
}
