using UnityEngine;
using UnityEngine.UI;

public class JoystickInitializer : MonoBehaviour
{
    void Awake()
    {
        gameObject.AddComponent<ETCInput>();

        var joystickTrans = transform.Find("Joystick");
        joystickTrans.gameObject.AddComponent<CanvasGroup>();
        var joystick = joystickTrans.gameObject.AddComponent<ETCJoystick>();
        joystick.thumb = joystickTrans.Find("Thumb") as RectTransform;

        joystick.onMove = new ETCJoystick.OnMoveHandler();
        joystick.onMoveEnd = new ETCJoystick.OnMoveEndHandler();
        joystick.onMoveSpeed = new ETCJoystick.OnMoveSpeedHandler();
        joystick.onMoveStart = new ETCJoystick.OnMoveStartHandler();
        joystick.onTouchStart = new ETCJoystick.OnTouchStartHandler();
        joystick.onTouchUp = new ETCJoystick.OnTouchUpHandler();
        joystick.OnDownDown = new ETCJoystick.OnDownDownHandler();
        joystick.OnDownLeft = new ETCJoystick.OnDownLeftHandler();
        joystick.OnDownRight = new ETCJoystick.OnDownRightHandler();
        joystick.OnDownUp = new ETCJoystick.OnDownUpHandler();
        joystick.OnPressDown = new ETCJoystick.OnDownDownHandler();
        joystick.OnPressLeft = new ETCJoystick.OnDownLeftHandler();
        joystick.OnPressRight = new ETCJoystick.OnDownRightHandler();
        joystick.OnPressUp = new ETCJoystick.OnDownUpHandler();

        var btns = transform.Find("Buttons");
        for (int i = 0; i < btns.childCount; ++i)
        {
            var btnTrans = btns.GetChild(i);
            var btn = btnTrans.gameObject.AddComponent<ETCButton>();
            var img = btnTrans.GetComponent<Image>();
            btn.normalSprite = img.sprite;
            btn.pressedSprite = img.sprite;
            btn.normalColor = Color.white;
            btn.pressedColor = Color.gray;
            btn.onDown = new ETCButton.OnDownHandler();
            btn.onPressed = new ETCButton.OnPressedHandler();
            btn.onPressedValue = new ETCButton.OnPressedValueandler();
            btn.onUp = new ETCButton.OnUPHandler();
        }
    }
}
