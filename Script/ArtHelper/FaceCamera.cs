using UnityEngine;

[ExecuteInEditMode]
class FaceCamera : MonoBehaviour
{
    public bool freezeX = true;
    public bool freezeY = true;
    public bool freezeZ = true;
    private void Update()
    {
        if (Camera.main == null) return;
        var transformEulerAngles = transform.localEulerAngles;
        var cameraEulerAngles = Camera.main.transform.localEulerAngles;
        transform.localEulerAngles = new Vector3
        {
            x = freezeX ? transformEulerAngles.x : (-cameraEulerAngles.x),
            y = freezeY ? transformEulerAngles.y : (cameraEulerAngles.y - 180),
            z = freezeZ ? transformEulerAngles.z : -cameraEulerAngles.z
        };
    }
}
