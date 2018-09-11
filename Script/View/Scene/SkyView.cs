using UnityEngine;

public class SkyView : MonoBehaviour
{
    private void Start()
    {
        if (Camera.main == null) return;
        var ctrl = Camera.main.GetComponent<CameraCtrl>();
        if (ctrl == null) return;

        ctrl.skyView = this;
    }

}